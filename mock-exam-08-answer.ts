// =============================================================================
// MOCK EXAM #08 — ANSWER KEY
// Smells fixed:
//   1. Callback pyramid     → async/await with fs.promises.readFile
//   2. Implicit any types   → CsvRow, ImportResult, ImportError interfaces
//   3. Exception swallowing → errors collected in ImportResult.errors; thrown on fatal issues
//   4. Magic column indices → parseCsv() uses header row to map named fields
//   5. No validation        → CsvRowValidator.validate() checks each row before insert
//   6. SRP violation        → readFile / parseCsv / validate / import / summarise separated
// =============================================================================

import * as fs from 'fs';

// ── Domain types ──────────────────────────────────────────────────────────────

interface CsvRow {
  name: string;
  price: number;
  category: string;
  stock: number;
}

interface ImportError {
  row: number;
  reason: string;
}

interface ImportResult {
  imported: number;
  skipped: number;
  errors: ImportError[];
}

// ── Repository abstraction ────────────────────────────────────────────────────

interface IProductRepository {
  insertProduct(row: CsvRow): Promise<void>;
}

// ── CSV parsing (fix: header row maps column names → no magic indices) ────────

function parseCsv(content: string): Array<Partial<CsvRow>> {
  const lines = content.split('\n').filter(l => l.trim() !== '');
  if (lines.length < 2) return [];

  const headers = lines[0].split(',').map(h => h.trim().toLowerCase());
  const rows: Array<Partial<CsvRow>> = [];

  for (let i = 1; i < lines.length; i++) {
    const cols = lines[i].split(',');
    const entry: Record<string, string> = {};
    headers.forEach((h, idx) => { entry[h] = (cols[idx] ?? '').trim(); });

    rows.push({
      name:     entry['name'],
      price:    entry['price']    !== undefined ? parseFloat(entry['price'])    : undefined,
      category: entry['category'],
      stock:    entry['stock']    !== undefined ? parseInt(entry['stock'], 10)  : undefined,
    });
  }

  return rows;
}

// ── Validation (fix: explicit checks before insert) ──────────────────────────

class CsvRowValidator {
  static validate(row: Partial<CsvRow>, rowIndex: number): ImportError | null {
    if (!row.name || row.name.trim() === '') {
      return { row: rowIndex, reason: 'name is empty' };
    }
    if (row.price === undefined || isNaN(row.price) || row.price < 0) {
      return { row: rowIndex, reason: `invalid price: ${row.price}` };
    }
    if (!row.category || row.category.trim() === '') {
      return { row: rowIndex, reason: 'category is empty' };
    }
    if (row.stock === undefined || isNaN(row.stock) || row.stock < 0) {
      return { row: rowIndex, reason: `invalid stock: ${row.stock}` };
    }
    return null;
  }
}

// ── Import service (fix: single responsibility per method) ───────────────────

class CsvImportService {
  constructor(private readonly repo: IProductRepository) {}

  async import(filePath: string): Promise<ImportResult> {
    // fix smell 1: async/await instead of callback pyramid
    // fix smell 3: fatal read error is thrown, not swallowed
    const content = await fs.promises.readFile(filePath, 'utf8');

    const rawRows = parseCsv(content);
    const result: ImportResult = { imported: 0, skipped: 0, errors: [] };

    for (let i = 0; i < rawRows.length; i++) {
      const rowNumber = i + 2; // account for header row

      // fix smell 5: validate before inserting
      const error = CsvRowValidator.validate(rawRows[i], rowNumber);
      if (error) {
        result.errors.push(error);
        result.skipped++;
        continue;
      }

      // fix smell 3: per-row errors collected, not silently swallowed
      try {
        await this.repo.insertProduct(rawRows[i] as CsvRow);
        result.imported++;
      } catch (e) {
        result.errors.push({ row: rowNumber, reason: (e as Error).message });
        result.skipped++;
      }
    }

    return result;
  }
}

// ── Infrastructure stub ───────────────────────────────────────────────────────

class DbProductRepository implements IProductRepository {
  private db = { query: (sql: string, params: unknown[], cb: (err: Error | null) => void) => cb(null) };

  insertProduct(row: CsvRow): Promise<void> {
    return new Promise((resolve, reject) => {
      this.db.query(
        'INSERT INTO products (name, price, category, stock) VALUES (?, ?, ?, ?)',
        [row.name, row.price, row.category, row.stock],
        (err) => (err ? reject(err) : resolve())
      );
    });
  }
}

// ── Entry point ───────────────────────────────────────────────────────────────

async function main(): Promise<void> {
  const repo = new DbProductRepository();
  const service = new CsvImportService(repo);
  const result = await service.import('./products.csv');
  console.log(`done — imported: ${result.imported}, skipped: ${result.skipped}, errors: ${result.errors.length}`);
  if (result.errors.length > 0) {
    result.errors.forEach(e => console.warn(`  row ${e.row}: ${e.reason}`));
  }
}

main().catch(err => console.error('fatal:', err));
