// =============================================================================
// MOCK REFACTORING EXAM #01 — TypeScript
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, add types, do NOT add new libraries.
// =============================================================================
//
// SMELLS TO FIND (don't peek until you've done your own scan):
//   1. _______________________________________________
//   2. _______________________________________________
//   3. _______________________________________________
//   4. _______________________________________________
//   5. _______________________________________________
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

export class ReportService {
    gen(uid: number, t: string, sd: string, ed: string) {
        let data: any[] = [];

        if (t === "sales") {
            const mysql = require("mysql2");
            const c = mysql.createConnection({ host: "127.0.0.1", user: "root", password: "P@ssw0rd!", database: "app_db" });
            c.connect();
            const rows: any[] = [];
            c.query(`SELECT * FROM orders WHERE user_id = ${uid} AND created_at BETWEEN '${sd}' AND '${ed}'`, (e: any, r: any) => { rows.push(...r); });
            c.end();
            data = rows;
        } else if (t === "inventory") {
            const mysql = require("mysql2");
            const c = mysql.createConnection({ host: "127.0.0.1", user: "root", password: "P@ssw0rd!", database: "app_db" });
            c.connect();
            const rows: any[] = [];
            c.query(`SELECT * FROM products WHERE stock < 10`, (e: any, r: any) => { rows.push(...r); });
            c.end();
            data = rows;
        } else if (t === "users") {
            const mysql = require("mysql2");
            const c = mysql.createConnection({ host: "127.0.0.1", user: "root", password: "P@ssw0rd!", database: "app_db" });
            c.connect();
            const rows: any[] = [];
            c.query(`SELECT * FROM users WHERE created_at BETWEEN '${sd}' AND '${ed}' AND role = 'customer'`, (e: any, r: any) => { rows.push(...r); });
            c.end();
            data = rows;
        }

        let out = "";
        if (t === "sales") {
            let total = 0;
            for (const r of data) { total += r.amount; }
            if (total > 100000) {
                out = `SALES REPORT\nTotal: ${total}\nStatus: GOLD`;
            } else if (total > 50000) {
                out = `SALES REPORT\nTotal: ${total}\nStatus: SILVER`;
            } else {
                out = `SALES REPORT\nTotal: ${total}\nStatus: BRONZE`;
            }
        } else if (t === "inventory") {
            out = `INVENTORY REPORT\nLow stock items: ${data.length}`;
        } else if (t === "users") {
            out = `USER REPORT\nNew customers: ${data.length}`;
        }

        const nodemailer = require("nodemailer");
        const tr = nodemailer.createTransport({ host: "smtp.gmail.com", port: 587, auth: { user: "reports@company.com", pass: "smtp_secret_99" } });
        tr.sendMail({ from: "reports@company.com", to: "manager@company.com", subject: `Report - ${t}`, text: out });

        console.log("report sent");
        return out;
    }
}

// =============================================================================
// YOUR REFACTORED VERSION BELOW
// =============================================================================
