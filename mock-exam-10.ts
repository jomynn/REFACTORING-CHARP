// =============================================================================
// MOCK REFACTORING EXAM #10 — TypeScript
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, TypeScript strict mode, no new packages.
// =============================================================================
//
// SMELLS TO FIND (fill in before you start coding):
//   1. _______________________________________________
//   2. _______________________________________________
//   3. _______________________________________________
//   4. _______________________________________________
//   5. _______________________________________________
//   6. _______________________________________________
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

class Logger {
    log(msg: string): void {
        console.log("[LOG]", msg);
    }
}

// SMELL 1: Global mutable state — module-level variable mutated by login()
let authToken: string | null = null;

// SMELL 6: Tight coupling — Logger instantiated at module level
const logger = new Logger();

// SMELL 5: any return type — Promise<any>, path typed as any
export async function get(path: any): Promise<any> {
    const logger = new Logger(); // SMELL 6: new Logger() again inside function
    logger.log("GET " + path);

    // SMELL 2: Hardcoded base URL copy-pasted
    // SMELL 3: DRY violation — headers block repeated
    const response = await fetch("https://api.internal/v2" + path, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + authToken,
        },
    });

    // SMELL 3: DRY violation — error check repeated
    if (!response.ok) {
        throw new Error("Request failed with status " + response.status);
    }

    // SMELL 4: Unhandled rejection — no .catch()
    return response.json().then((res: any) => res);
}

// SMELL 5: any return type
export async function post(path: any, body: any): Promise<any> {
    const logger = new Logger(); // SMELL 6: tight coupling repeated
    logger.log("POST " + path);

    // SMELL 2: Hardcoded base URL again
    // SMELL 3: DRY violation — same headers block
    const response = await fetch("https://api.internal/v2" + path, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + authToken,
        },
        body: JSON.stringify(body),
    });

    // SMELL 3: DRY violation — same error check
    if (!response.ok) {
        throw new Error("Request failed with status " + response.status);
    }

    return response.json(); // SMELL 4: no .catch() — silent failure
}

// SMELL 5: any return type
export async function put(path: any, body: any): Promise<any> {
    const logger = new Logger(); // SMELL 6
    logger.log("PUT " + path);

    // SMELL 2: Hardcoded base URL again
    // SMELL 3: DRY violation — same headers block
    const response = await fetch("https://api.internal/v2" + path, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + authToken,
        },
        body: JSON.stringify(body),
    });

    // SMELL 3: DRY violation — same error check
    if (!response.ok) {
        throw new Error("Request failed with status " + response.status);
    }

    return response.json(); // SMELL 4: no .catch()
}

// SMELL 5: any return type
export async function deleteReq(path: any): Promise<any> {
    const logger = new Logger(); // SMELL 6
    logger.log("DELETE " + path);

    // SMELL 2: Hardcoded base URL again
    // SMELL 3: DRY violation — headers block repeated a 4th time
    const response = await fetch("https://api.internal/v2" + path, {
        method: "DELETE",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + authToken,
        },
    });

    // SMELL 3: DRY violation — same error check a 4th time
    if (!response.ok) {
        throw new Error("Request failed with status " + response.status);
    }

    return response.json(); // SMELL 4: no .catch()
}

export async function login(user: string, pass: string): Promise<void> {
    // SMELL 2: Hardcoded base URL yet again
    const response = await fetch("https://api.internal/v2/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: user, password: pass }),
    });

    if (!response.ok) {
        throw new Error("Login failed with status " + response.status);
    }

    // SMELL 1: Mutating global state
    const data = await response.json();
    authToken = data.token;

    logger.log("Logged in, token acquired");
}

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
