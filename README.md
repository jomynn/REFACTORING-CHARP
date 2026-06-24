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

---

## Code Smells Covered

| Smell | Exam 01 | Exam 02 |
|---|---|---|
| SQL injection (string interpolation) | ✓ | ✓ |
| Duplicated code block (DRY violation) | ✓ | ✓ |
| Tight coupling (`new` inside business logic) | ✓ | ✓ |
| SRP violation (god method / god class) | ✓ | ✓ |
| Magic literals (numbers / strings) | ✓ | ✓ |
| Hardcoded credentials in source code | ✓ | ✓ |
| Untestable design (missing interfaces) | — | ✓ |

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
