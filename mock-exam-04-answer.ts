// =============================================================================
// MOCK EXAM #04 — ANSWER KEY
// Smells fixed:
//   1. Hardcoded secret    → JWT_SECRET read from process.env.JWT_SECRET
//   2. Callback pyramid    → async/await throughout; db wrapped in promise helpers
//   3. Tight coupling      → IUserRepository interface injected via constructor
//   4. Implicit any types  → User, LoginResult, RegisterResult interfaces defined
//   5. DRY violation       → isValidEmail() extracted once and reused
//   6. SRP violation       → AuthService / UserRepository / TokenService separated
// =============================================================================

const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');

// --- Interfaces -----------------------------------------------------------

interface User {
  id: number;
  username: string;
  email: string;
  passwordHash: string;
  lastLogin: Date | null;
}

interface LoginResult {
  token: string;
  userId: number;
}

interface RegisterResult {
  token: string;
  userId: number;
}

interface IUserRepository {
  findByUsername(username: string): Promise<User | null>;
  save(user: Omit<User, 'id'>): Promise<number>;
  updateLastLogin(username: string): Promise<void>;
}

interface ITokenService {
  sign(payload: object): string;
  verify(token: string): object;
}

// --- Helpers --------------------------------------------------------------

function isValidEmail(email: string): boolean {
  return /^[^@]+@[^@]+$/.test(email);
}

// --- TokenService ---------------------------------------------------------

class JwtTokenService implements ITokenService {
  private readonly secret: string;

  constructor() {
    const secret = process.env.JWT_SECRET;
    if (!secret) {
      throw new Error("JWT_SECRET environment variable is not set");
    }
    this.secret = secret;
  }

  sign(payload: object): string {
    return jwt.sign(payload, this.secret, { expiresIn: "1h" });
  }

  verify(token: string): object {
    return jwt.verify(token, this.secret) as object;
  }
}

// --- UserRepository -------------------------------------------------------

class DbUserRepository implements IUserRepository {
  private db: any;

  constructor(db: any) {
    this.db = db;
  }

  async findByUsername(username: string): Promise<User | null> {
    const rows = await this.db.queryAsync(
      "SELECT * FROM users WHERE username = $1",
      [username]
    );
    if (!rows || rows.length === 0) return null;
    const r = rows[0];
    return { id: r.id, username: r.username, email: r.email, passwordHash: r.password_hash, lastLogin: r.last_login };
  }

  async save(user: Omit<User, 'id'>): Promise<number> {
    const result = await this.db.queryAsync(
      "INSERT INTO users (username, email, password_hash) VALUES ($1, $2, $3) RETURNING id",
      [user.username, user.email, user.passwordHash]
    );
    return result[0].id;
  }

  async updateLastLogin(username: string): Promise<void> {
    await this.db.queryAsync(
      "UPDATE users SET last_login = NOW() WHERE username = $1",
      [username]
    );
  }
}

// --- AuthService ----------------------------------------------------------

class AuthService {
  constructor(
    private readonly repo: IUserRepository,
    private readonly tokens: ITokenService
  ) {}

  async login(username: string, password: string): Promise<LoginResult> {
    if (!isValidEmail(username)) {
      throw new Error("Invalid email format");
    }

    const user = await this.repo.findByUsername(username);
    if (!user) {
      throw new Error("User not found");
    }

    const match: boolean = await bcrypt.compare(password, user.passwordHash);
    if (!match) {
      throw new Error("Invalid credentials");
    }

    await this.repo.updateLastLogin(username);

    const token = this.tokens.sign({ id: user.id, username: user.username });
    return { token, userId: user.id };
  }

  async register(username: string, password: string, email: string): Promise<RegisterResult> {
    if (!isValidEmail(email)) {
      throw new Error("Invalid email format");
    }

    const existing = await this.repo.findByUsername(username);
    if (existing) {
      throw new Error("Username already taken");
    }

    const passwordHash: string = await bcrypt.hash(password, 10);
    const newId = await this.repo.save({ username, email, passwordHash, lastLogin: null });

    const token = this.tokens.sign({ id: newId, username });
    return { token, userId: newId };
  }
}

module.exports = { AuthService, DbUserRepository, JwtTokenService, isValidEmail };
