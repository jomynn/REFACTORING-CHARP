// =============================================================================
// MOCK REFACTORING EXAM #04 — TypeScript
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

const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const DatabaseClient = require('./db');

const JWT_SECRET = "s3cr3t-k3y-123";

class AuthService {
  private db;

  constructor() {
    this.db = new DatabaseClient("postgres://localhost/auth");
  }

  login(username, password, callback) {
    if (!/^[^@]+@[^@]+$/.test(username)) {
      console.log("Invalid email format for login attempt: " + username);
      return callback(new Error("Invalid email format"), null);
    }

    this.db.query("SELECT * FROM users WHERE username = $1", [username], (err, rows) => {
      if (err) {
        console.log("DB error during login: " + err.message);
        return callback(err, null);
      }

      if (!rows || rows.length === 0) {
        console.log("User not found: " + username);
        return callback(new Error("User not found"), null);
      }

      const user = rows[0];

      bcrypt.compare(password, user.password_hash, (hashErr, match) => {
        if (hashErr) {
          console.log("Bcrypt error: " + hashErr.message);
          return callback(hashErr, null);
        }

        if (!match) {
          console.log("Password mismatch for user: " + username);
          return callback(new Error("Invalid credentials"), null);
        }

        this.db.query("UPDATE users SET last_login = NOW() WHERE username = $1", [username], (updateErr) => {
          if (updateErr) {
            console.log("Failed to update last_login: " + updateErr.message);
            return callback(updateErr, null);
          }

          const token = jwt.sign({ id: user.id, username: user.username }, JWT_SECRET, { expiresIn: "1h" });
          console.log("Login successful for user: " + username);
          return callback(null, { token, userId: user.id });
        });
      });
    });
  }

  register(username, password, email, callback) {
    if (!/^[^@]+@[^@]+$/.test(email)) {
      console.log("Invalid email format during registration: " + email);
      return callback(new Error("Invalid email format"), null);
    }

    this.db.query("SELECT id FROM users WHERE username = $1", [username], (err, rows) => {
      if (err) {
        console.log("DB error during register lookup: " + err.message);
        return callback(err, null);
      }

      if (rows && rows.length > 0) {
        console.log("Username already taken: " + username);
        return callback(new Error("Username already taken"), null);
      }

      bcrypt.hash(password, 10, (hashErr, hash) => {
        if (hashErr) {
          console.log("Bcrypt hash error: " + hashErr.message);
          return callback(hashErr, null);
        }

        this.db.query(
          "INSERT INTO users (username, email, password_hash) VALUES ($1, $2, $3) RETURNING id",
          [username, email, hash],
          (insertErr, result) => {
            if (insertErr) {
              console.log("Insert error: " + insertErr.message);
              return callback(insertErr, null);
            }

            const newId = result[0].id;
            const token = jwt.sign({ id: newId, username }, JWT_SECRET, { expiresIn: "1h" });
            console.log("Registration successful for: " + username);
            return callback(null, { token, userId: newId });
          }
        );
      });
    });
  }
}

module.exports = { AuthService };

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
