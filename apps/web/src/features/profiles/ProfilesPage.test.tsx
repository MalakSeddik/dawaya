import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { authResponse, json, mockApi, profile, signedInStorage } from '@/test/mockApi';
import { renderApp } from '@/test/renderApp';

import { ACTIVE_PROFILE_STORAGE_KEY } from './useActiveProfile';

const family = [
  profile(),
  profile({
    id: 2,
    displayName: 'Mom',
    isSelf: false,
    relationship: 'Mother',
    ageGroup: 'Elderly',
    allergies: [{ id: 7, ingredientId: 3, ingredientNameEn: 'Penicillin', ingredientNameAr: 'بنسلين', allergyText: null }],
  }),
];

describe('family profiles', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('asks guests to sign in (BR-04, BR-05)', async () => {
    mockApi({});
    renderApp('/profiles');

    expect(await screen.findByText('سجل دخول عشان تضيف أفراد عيلتك وتتابع أدويتهم.')).toBeInTheDocument();
    expect(screen.getAllByRole('link', { name: 'تسجيل الدخول' }).length).toBeGreaterThan(0);
  });

  it('lists the family with allergies and switches the active profile', async () => {
    const user = userEvent.setup();
    mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'GET /api/v1/profiles': () => json(family),
    });
    const storage = signedInStorage();
    renderApp('/profiles', storage);

    const mom = await screen.findByRole('article', { name: 'Mom' });
    expect(within(mom).getByText(/Mother · كبير في السن/)).toBeInTheDocument();
    expect(within(mom).getByText(/بنسلين/)).toBeInTheDocument();
    expect(screen.getByRole('article', { name: 'Malak (أنا)' })).toHaveAttribute('aria-current', 'true');

    await user.click(within(mom).getByRole('button', { name: 'تابع أدوية الشخص ده' }));

    expect(mom).toHaveAttribute('aria-current', 'true');
    expect(storage.getItem(ACTIVE_PROFILE_STORAGE_KEY)).toBe('2');
  });

  it('creates a profile and continues to the page for adding allergies', async () => {
    const user = userEvent.setup();
    const calls = mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'GET /api/v1/profiles': () => json(family),
      'POST /api/v1/profiles': () => json(profile({ id: 9, displayName: 'Dad', isSelf: false, ageGroup: 'Elderly' })),
      'GET /api/v1/profiles/9': () => json(profile({ id: 9, displayName: 'Dad', isSelf: false, ageGroup: 'Elderly' })),
    });
    const { router } = renderApp('/profiles/new', signedInStorage());

    await user.type(await screen.findByLabelText('الاسم'), 'Dad');
    await user.selectOptions(screen.getByLabelText('السن'), 'Elderly');
    await user.click(screen.getByRole('button', { name: 'حفظ، وبعدها ضيف الحساسية' }));

    expect(await screen.findByRole('heading', { name: /الحساسية/ })).toBeInTheDocument();
    expect(router.state.location.pathname).toBe('/profiles/9');
    expect(calls.find((c) => c.method === 'POST' && c.path === '/api/v1/profiles')?.body).toEqual({
      displayName: 'Dad',
      relationship: null,
      ageGroup: 'Elderly',
      isPregnant: false,
      isBreastfeeding: false,
    });
  });

  it('links an allergy picked from the suggestions', async () => {
    const user = userEvent.setup();
    const dad = profile({ id: 9, displayName: 'Dad', isSelf: false });
    const calls = mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'GET /api/v1/profiles/9': () => json(dad),
      'GET /api/v1/ingredients': () => json([{ id: 3, nameEn: 'Penicillin', nameAr: 'بنسلين' }]),
      'POST /api/v1/profiles/9/allergies': () =>
        json({ ...dad, allergies: [{ id: 1, ingredientId: 3, ingredientNameEn: 'Penicillin', ingredientNameAr: 'بنسلين', allergyText: null }] }),
    });
    renderApp('/profiles/9', signedInStorage());

    await user.type(await screen.findByLabelText('إضافة حساسية'), 'بنس');
    await user.click(await screen.findByRole('button', { name: /بنسلين \(Penicillin\)/ }));

    expect(await screen.findByRole('button', { name: 'امسح حساسية بنسلين' })).toBeInTheDocument();
    expect(calls.find((c) => c.path === '/api/v1/profiles/9/allergies')?.body).toEqual({ ingredientId: 3, allergyText: null });
  });

  it('never offers to delete the Me profile', async () => {
    mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'GET /api/v1/profiles/1': () => json(profile()),
    });
    renderApp('/profiles/1', signedInStorage());

    expect(await screen.findByRole('heading', { name: /الحساسية/ })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /امسح Malak/ })).not.toBeInTheDocument();
  });
});
