// =============================================================================
// MOCK EXAM #10 — ANSWER KEY
// Smells fixed:
//   1. Global mutable state  → ApiClient class; token stored as private instance field
//   2. Hardcoded base URL    → ApiClientConfig { baseUrl: string; getToken(): string | null } injected
//   3. DRY violation         → private request<T>() helper used by all public methods
//   4. Unhandled rejection   → async/await with try/catch; ApiError class thrown on HTTP errors
//   5. Any return type       → generic get<T>(), post<T>(), put<T>(), delete<T>() methods
//   6. Tight coupling        → ILogger { info(msg: string): void } injected via constructor
// =============================================================================

// ---------------------------------------------------------------------------
// Supporting types and classes
// ---------------------------------------------------------------------------

export interface ApiClientConfig {
    baseUrl: string;
    getToken(): string | null;
}

export class ApiError extends Error {
    constructor(
        message: string,
        public readonly status: number,
    ) {
        super(message);
        this.name = "ApiError";
    }
}

export interface ILogger {
    info(msg: string): void;
    error(msg: string, err?: unknown): void;
}

// ---------------------------------------------------------------------------
// ApiClient — all smells resolved
// ---------------------------------------------------------------------------

export class ApiClient {
    // FIX 1: token is a private instance field, not a global variable
    private token: string | null = null;

    // FIX 6: ILogger injected via constructor — no tight coupling
    constructor(
        // FIX 2: base URL lives in the injected config, not hard-coded everywhere
        private readonly config: ApiClientConfig,
        private readonly logger: ILogger,
    ) {}

    // -----------------------------------------------------------------------
    // FIX 3: single private helper used by every public method
    // FIX 4: async/await + try/catch — no silent promise failures
    // FIX 5: generic <T> eliminates all `any`
    // -----------------------------------------------------------------------
    private async request<T>(
        method: string,
        path: string,
        body?: unknown,
    ): Promise<T> {
        const url = this.config.baseUrl + path;
        this.logger.info(`${method} ${url}`);

        // FIX 4: wrapped in try/catch — rejections are always surfaced
        try {
            const response = await fetch(url, {
                method,
                headers: {
                    "Content-Type": "application/json",
                    // FIX 2: token comes from config, not a global
                    ...(this.token !== null
                        ? { Authorization: `Bearer ${this.token}` }
                        : {}),
                },
                body: body !== undefined ? JSON.stringify(body) : undefined,
            });

            // FIX 3: single error-check in one place
            if (!response.ok) {
                throw new ApiError(
                    `HTTP ${method} ${url} failed with status ${response.status}`,
                    response.status,
                );
            }

            // FIX 5: typed via generic T, not cast to any
            return (await response.json()) as T;
        } catch (err) {
            this.logger.error(`${method} ${url} threw`, err);
            throw err;
        }
    }

    // -----------------------------------------------------------------------
    // Public API — all delegating to request<T>()
    // -----------------------------------------------------------------------

    // FIX 5: generic return type instead of Promise<any>
    async get<T>(path: string): Promise<T> {
        return this.request<T>("GET", path);
    }

    async post<T>(path: string, body: unknown): Promise<T> {
        return this.request<T>("POST", path, body);
    }

    async put<T>(path: string, body: unknown): Promise<T> {
        return this.request<T>("PUT", path, body);
    }

    async delete<T>(path: string): Promise<T> {
        return this.request<T>("DELETE", path);
    }

    // -----------------------------------------------------------------------
    // login — updates the private instance field, not a global variable
    // -----------------------------------------------------------------------
    async login(username: string, password: string): Promise<void> {
        // FIX 1: result stored in this.token, not a module-level let
        const data = await this.request<{ token: string }>(
            "POST",
            "/auth/login",
            { username, password },
        );
        this.token = data.token;
        this.logger.info("Logged in, token acquired");
    }
}

// ---------------------------------------------------------------------------
// Smell-to-fix trace
// ---------------------------------------------------------------------------
//
//  SMELL 1 — Global mutable state
//    Before:  let authToken: string | null = null  (module top-level)
//    After:   private token: string | null = null  (ApiClient instance field)
//
//  SMELL 2 — Hardcoded base URL
//    Before:  "https://api.internal/v2" copy-pasted in every fetch() call
//    After:   this.config.baseUrl + path  — one place, injected at construction
//
//  SMELL 3 — DRY violation (headers + error check repeated 4×)
//    Before:  headers block and `if (!response.ok)` duplicated in get/post/put/deleteReq
//    After:   private request<T>() — single implementation, zero duplication
//
//  SMELL 4 — Unhandled rejection
//    Before:  response.json().then(res => res)  with no .catch()
//    After:   await response.json() inside try/catch; errors logged and re-thrown
//
//  SMELL 5 — any return type
//    Before:  Promise<any>, path: any, (res: any)
//    After:   Promise<T> with generics throughout; path: string
//
//  SMELL 6 — Tight coupling
//    Before:  const logger = new Logger() created inside every function
//    After:   ILogger injected in ApiClient constructor; callers supply implementation
