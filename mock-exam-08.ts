// =============================================================================
// MOCK REFACTORING EXAM #08 — TypeScript
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

const fs = require('fs');

const db = { query: (sql: any, params: any, cb: any) => cb(null) };

function importFile(filePath: any) {
  fs.readFile(filePath, 'utf8', function(err: any, data: any) {
    if (err) {
      console.log('could not read file');
      return;
    }

    try {
      var lines = data.split('\n');
      var rows: any[] = [];

      for (var i = 0; i < lines.length; i++) {
        var line = lines[i];
        if (line.trim() === '') continue;
        var cols = line.split(',');
        rows.push(cols);
      }

      var imported = 0;

      for (var j = 1; j < rows.length; j++) {
        var row: any = rows[j];

        var name     = row[0];
        var price    = parseFloat(row[1]);
        var category = row[2];
        var stock    = parseInt(row[3], 10);

        db.query(
          'INSERT INTO products (name, price, category, stock) VALUES (?, ?, ?, ?)',
          [name, price, category, stock],
          function(err2: any) {
            if (err2) {
              // ignore
            } else {
              imported++;
            }
          }
        );
      }

      console.log('done, imported ' + imported);
    } catch(e) {}
  });
}

importFile('./products.csv');

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
