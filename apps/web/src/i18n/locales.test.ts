import { describe, expect, it } from 'vitest';

import ar from './locales/ar/common.json';
import en from './locales/en/common.json';

function keysOf(value: unknown, prefix = ''): string[] {
  if (typeof value !== 'object' || value === null) return [prefix];
  return Object.entries(value).flatMap(([key, child]) => keysOf(child, prefix ? `${prefix}.${key}` : key));
}

describe('translations', () => {
  it('Arabic and English have exactly the same keys', () => {
    expect(keysOf(en).sort()).toEqual(keysOf(ar).sort());
  });

  it('has no empty strings', () => {
    const values = [...keysOf(ar), ...keysOf(en)];
    expect(values.length).toBeGreaterThan(0);
    for (const locale of [ar, en]) {
      expect(JSON.stringify(locale)).not.toMatch(/:\s*""/);
    }
  });
});
