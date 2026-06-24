# Refactoring Assessment Prep

Practice workspace for 60-minute refactoring technical assessments.
Focus: Clean Architecture, SOLID principles, and common refactoring patterns in C# and TypeScript.

---

## Files

| File | Language | Description |
|---|---|---|
| `first-test.md` | — | Coach prompt: drills, checklist, time strategy, patterns reference |
| `mock-exam-01.ts` | TypeScript | `ReportService` — 5 smells to find and fix |
| `mock-exam-02.cs` | C# .NET | `InvoiceManager` — 6 smells to find and fix |
| `mock-exam-02-answer.cs` | C# .NET | Full refactored answer for exam-02 with explanation comments |

---

## How to use

1. Open the exam file (`mock-exam-01.ts` or `mock-exam-02.cs`)
2. Fill in the **SMELLS TO FIND** list at the top — do this before touching code
3. Write your refactored version below the dashed line in the same file
4. Check against the answer file or paste your attempt in the chat for review

---

## Smells covered across exams

| Smell | Exam 01 | Exam 02 |
|---|---|---|
| SQL injection (string interpolation) | ✓ | ✓ |
| Duplicated code block (DRY) | ✓ | ✓ |
| Tight coupling (`new` inside business logic) | ✓ | ✓ |
| SRP violation (god method) | ✓ | ✓ |
| Magic literals (numbers / strings) | ✓ | ✓ |
| Hardcoded credentials | ✓ | ✓ |
| Untestable design (no interfaces) | — | ✓ |

---

## 60-minute time-box strategy

| Time | Activity |
|---|---|
| 0–5 min | Read instructions, identify language and framework |
| 5–15 min | Scan codebase, write your smell list — do NOT code yet |
| 15–20 min | Rank smells by impact, pick 2–3 to fully fix |
| 20–50 min | Refactor, commit after each logical change, run tests |
| 50–57 min | Write or update one test proving a key fix holds |
| 57–60 min | Final scan: naming, leftover TODOs, regressions |

---

## Quick smell checklist (5-minute codebase scan)

- [ ] Entry point — how is DI/composition wired?
- [ ] Layer violations — does Domain reference HTTP, DB, or UI?
- [ ] God classes — any class >200 lines or >10 public methods?
- [ ] Magic literals — raw numbers or strings in logic?
- [ ] Long methods — any method >20 lines?
- [ ] `new` inside business logic — hard-coded infrastructure?
- [ ] Repeated blocks — same code in 2+ places?
- [ ] Switch/if-else on a type string — polymorphism opportunity?
- [ ] No interfaces on I/O — untestable without real DB or SMTP?
