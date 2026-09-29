// Fails the lint step if physical (left/right) CSS is used anywhere in src.
// RTL is a first-class layout (CLAUDE.md UI rules): use ms-/me-/ps-/pe-/start-/end- instead.
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const root = new URL('../src', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1');

const rules = [
  { pattern: /(?<![\w-])-?(ml|mr|pl|pr)-[\w[\]./-]+/g, hint: 'use ms-/me-/ps-/pe-' },
  { pattern: /(?<![\w-])-?(left|right)-[\w[\]./-]+/g, hint: 'use start-/end-' },
  { pattern: /(?<![\w-])text-(left|right)(?![\w-])/g, hint: 'use text-start/text-end' },
  { pattern: /(?<![\w-])(rounded|border)-(l|r|tl|tr|bl|br)(?![\w])/g, hint: 'use rounded-s/-e or border-s/-e' },
  { pattern: /(?<![\w-])float-(left|right)(?![\w-])/g, hint: 'use float-start/float-end' },
  { pattern: /(?<![\w-])(margin|padding|border)-(left|right)\s*:/g, hint: 'use *-inline-start/end' },
  { pattern: /(?<![\w-])(left|right)\s*:\s*[^;{]+;/g, hint: 'use inset-inline-start/end' },
];

function* walk(dir) {
  for (const name of readdirSync(dir)) {
    const path = join(dir, name);
    if (statSync(path).isDirectory()) yield* walk(path);
    else if (/\.(tsx?|css)$/.test(name) && !/\.test\.tsx?$/.test(name)) yield path;
  }
}

const problems = [];
for (const file of walk(root)) {
  const lines = readFileSync(file, 'utf8').split('\n');
  lines.forEach((line, i) => {
    if (line.includes('logical-css-ignore')) return;
    for (const { pattern, hint } of rules) {
      for (const match of line.matchAll(pattern)) {
        problems.push(`${relative(process.cwd(), file)}:${i + 1}  "${match[0].trim()}"  -> ${hint}`);
      }
    }
  });
}

if (problems.length > 0) {
  console.error(`Physical left/right CSS found (${problems.length}):\n${problems.join('\n')}`);
  process.exit(1);
}
console.log('Logical CSS check passed.');
