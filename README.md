# C# .NET Refactoring Practice — Clean Architecture & SOLID Interview Prep

> Hands-on mock exams and answer keys for 60-minute refactoring technical assessments.
> Covers C# .NET 8, TypeScript, Clean Architecture, SOLID principles, and common code smells.

---

## What is this?

A self-contained practice workspace to prepare for refactoring-focused coding interviews and technical assessments. Each mock exam gives you a realistic messy codebase to identify code smells and refactor — the same format used in senior software engineering interviews.

**Languages:** C# (.NET 8) · TypeScript  
**Topics:** Clean Architecture · SOLID · Dependency Injection · Design Patterns · Code Smells · TDD-friendly design

---

## Mock Exams

| File | Language | Scenario | Smells |
|---|---|---|---|
| `mock-exam-01.ts` | TypeScript | `ReportService` — multi-type report generator | 5 |
| `mock-exam-02.cs` | C# .NET | `InvoiceManager` — invoice processing pipeline | 6 |
| `mock-exam-02-answer.cs` | C# .NET | Full refactored answer for exam-02 | — |
| `mock-exam-03.cs` | C# .NET | `OrderProcessor` — e-commerce order pipeline | 6 |
| `mock-exam-03-answer.cs` | C# .NET | Full refactored answer for exam-03 | — |
| `mock-exam-04.ts` | TypeScript | `AuthService` — JWT authentication pipeline | 6 |
| `mock-exam-04-answer.ts` | TypeScript | Full refactored answer for exam-04 | — |
| `mock-exam-05.cs` | C# .NET | `ProductCatalog` — product lookup with cache | 6 |
| `mock-exam-05-answer.cs` | C# .NET | Full refactored answer for exam-05 | — |
| `mock-exam-06.ts` | TypeScript | `CartService` — shopping cart checkout | 6 |
| `mock-exam-06-answer.ts` | TypeScript | Full refactored answer for exam-06 | — |
| `mock-exam-07.cs` | C# .NET | `NotificationDispatcher` — multi-channel notify | 6 |
| `mock-exam-07-answer.cs` | C# .NET | Full refactored answer for exam-07 | — |
| `mock-exam-08.ts` | TypeScript | `CsvImporter` — CSV file import pipeline | 6 |
| `mock-exam-08-answer.ts` | TypeScript | Full refactored answer for exam-08 | — |
| `mock-exam-09.cs` | C# .NET | `InventoryService` — stock deduction with alerts | 6 |
| `mock-exam-09-answer.cs` | C# .NET | Full refactored answer for exam-09 | — |
| `mock-exam-10.ts` | TypeScript | `ApiClient` — HTTP REST client wrapper | 6 |
| `mock-exam-10-answer.ts` | TypeScript | Full refactored answer for exam-10 | — |
| `mock-exam-11.cs` | C# .NET | `PayrollProcessor` — employee payroll | 6 |
| `mock-exam-11-answer.cs` | C# .NET | Full refactored answer for exam-11 | — |

---

## Code Smells Covered

| Smell | Exam 01 | Exam 02 | Exam 03 | Exam 04 | Exam 05 | Exam 06 | Exam 07 | Exam 08 | Exam 09 | Exam 10 | Exam 11 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| SQL injection (string interpolation) | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — |
| Duplicated code block (DRY violation) | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — |
| Tight coupling (`new` inside business logic) | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — |
| SRP violation (god method / god class) | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — |
| Magic literals (numbers / strings) | ✓ | ✓ | — | — | — | — | — | — | — | — | — |
| Hardcoded credentials in source code | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — |
| Untestable design (missing interfaces) | — | ✓ | ✓ | — | — | — | — | — | — | — | — |
| Swallowed exception (silent catch-all) | — | — | ✓ | — | — | — | — | — | — | — | — |
| Replace Conditional with Polymorphism | — | — | — | — | — | — | ✓ | — | — | — | — |
| Callback pyramid / missing async-await | — | — | — | ✓ | — | — | — | ✓ | — | — | — |
| Global mutable state (module-level) | — | — | — | — | — | — | — | — | — | ✓ | — |
| Race condition (non-atomic check-then-act) | — | — | — | — | — | — | — | — | ✓ | — | — |

---

## How to Use Each Exam

1. Open the exam file (`mock-exam-01.ts` or `mock-exam-02.cs`)
2. Fill in the **SMELLS TO FIND** list at the top — scan first, code second
3. Write your refactored version below the dashed line
4. Compare against the answer file or paste your attempt in Claude Code for a review

---

## 60-Minute Interview Time-Box Strategy

| Time | Activity |
|---|---|
| 0–5 min | Read instructions, identify language and framework |
| 5–15 min | Scan codebase, write your smell list — do **not** code yet |
| 15–20 min | Rank smells by impact, pick 2–3 to fully fix |
| 20–50 min | Refactor; commit after each logical change; run tests |
| 50–57 min | Write or update one test proving a key fix holds |
| 57–60 min | Final scan: naming, leftover TODOs, regressions |

> **Tip:** Fully finish 2 clean refactors over half-finishing 5. Leave a comment on what you'd do next if time runs out.

---

## 5-Minute Codebase Scan Checklist

Apply this when you first open an unfamiliar repo:

- [ ] **Entry point** — how is DI / composition root wired?
- [ ] **Layer violations** — does Domain reference HTTP, DB, or UI namespaces?
- [ ] **God classes** — any class >200 lines or >10 public methods?
- [ ] **Magic literals** — raw numbers or strings in business logic?
- [ ] **Long methods** — any method >20 lines is an Extract Method candidate
- [ ] **`new` inside business logic** — hard-coded infrastructure (DB, SMTP, HTTP)?
- [ ] **Repeated blocks** — same code in 2+ places?
- [ ] **Switch / if-else on a type string** — Replace Conditional with Polymorphism?
- [ ] **No interfaces on I/O** — untestable without a real database or mail server?

---

## Key Refactoring Patterns Reference

| Pattern | When to apply |
|---|---|
| **Extract Method** | Method >15 lines or a comment describing a block |
| **Replace Magic Number with Constant** | Any raw literal with business meaning |
| **Replace Conditional with Polymorphism** | Recurring `if/switch` on a type discriminator |
| **Introduce Parameter Object** | Method takes 4+ related parameters |
| **Dependency Injection** | `new ConcreteService()` inside a class |

---

## Related Resources

- `first-test.md` — full coach prompt with pattern drills, before/after examples, and a mock scenario
- `CLAUDE.md` — guidance for Claude Code sessions in this workspace
