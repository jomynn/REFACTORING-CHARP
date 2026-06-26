# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose

This directory is a refactoring practice workspace used to prepare for technical assessments. It contains notes, code drills, and mock scenarios focused on Clean Architecture, SOLID principles, and refactoring patterns in C# and TypeScript.

## Workspace files

| File | Role |
|---|---|
| `first-test.md` | Coach prompt: drills, checklist, time strategy, patterns reference |
| `mock-exam-01.ts` | TypeScript exam — `ReportService`, 5 smells to find and fix |
| `mock-exam-02.cs` | C# exam — `InvoiceManager`, 6 smells to find and fix |
| `mock-exam-02-answer.cs` | Full refactored answer for exam-02 with smell-to-fix trace |
| `mock-exam-03.cs` | C# exam — `OrderProcessor`, 6 smells to find and fix |
| `mock-exam-03-answer.cs` | Full refactored answer for exam-03 with smell-to-fix trace |
| `mock-exam-04.ts` | TypeScript exam — `AuthService`, 6 smells to find and fix |
| `mock-exam-04-answer.ts` | Full refactored answer for exam-04 with smell-to-fix trace |
| `mock-exam-05.cs` | C# exam — `ProductCatalog`, 6 smells to find and fix |
| `mock-exam-05-answer.cs` | Full refactored answer for exam-05 with smell-to-fix trace |
| `mock-exam-06.ts` | TypeScript exam — `CartService`, 6 smells to find and fix |
| `mock-exam-06-answer.ts` | Full refactored answer for exam-06 with smell-to-fix trace |
| `mock-exam-07.cs` | C# exam — `NotificationDispatcher`, 6 smells to find and fix |
| `mock-exam-07-answer.cs` | Full refactored answer for exam-07 with smell-to-fix trace |
| `mock-exam-08.ts` | TypeScript exam — `CsvImporter`, 6 smells to find and fix |
| `mock-exam-08-answer.ts` | Full refactored answer for exam-08 with smell-to-fix trace |
| `mock-exam-09.cs` | C# exam — `InventoryService`, 6 smells to find and fix |
| `mock-exam-09-answer.cs` | Full refactored answer for exam-09 with smell-to-fix trace |
| `mock-exam-10.ts` | TypeScript exam — `ApiClient`, 6 smells to find and fix |
| `mock-exam-10-answer.ts` | Full refactored answer for exam-10 with smell-to-fix trace |
| `mock-exam-11.cs` | C# exam — `PayrollProcessor`, 6 smells to find and fix |
| `mock-exam-11-answer.cs` | Full refactored answer for exam-11 with smell-to-fix trace |
| `README.md` | Smells matrix, time-box strategy, 5-minute scan checklist |

## Smells covered

SQL injection, DRY violations, tight coupling (`new` inside business logic), SRP god methods, magic literals, hardcoded credentials, untestable design (no interfaces), swallowed exception (silent catch-all), replace conditional with polymorphism, callback pyramid, global mutable state, race condition.

## Exam workflow

1. Open the exam file, fill in the **SMELLS TO FIND** list at the top before touching code.
2. Write the refactored version below the dashed line in the same file.
3. Check against the answer file or paste into chat for review.

## Adding a new mock exam

- Name sequentially: `mock-exam-03.cs`, `mock-exam-04.ts`, etc.
- Start with the header block from an existing exam (smells list, dashed line, answer space).
- Update the files table above and the smells matrix in `README.md`.

