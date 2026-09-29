import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';

import i18n from '@/i18n';

import { Disclaimer } from './Disclaimer';

// BR-23: fixed disclaimer text.
describe('Disclaimer', () => {
  it('shows the fixed Arabic text as a labelled note', () => {
    render(<Disclaimer />);
    const note = screen.getByRole('note');
    expect(note).toHaveTextContent('المعلومات دي عامة وللتوضيح بس، ومش بديل عن كلام الدكتور أو الصيدلي.');
    expect(note).toHaveAccessibleName(/تنبيه مهم/);
  });

  it('shows the fixed English text', async () => {
    await i18n.changeLanguage('en');
    render(<Disclaimer />);
    expect(screen.getByRole('note')).toHaveTextContent(
      'This is general information only. It does not replace advice from your doctor or pharmacist.',
    );
  });
});
