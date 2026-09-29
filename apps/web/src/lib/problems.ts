// Server errors carry a stable "code" (RFC 7807 extension); the UI maps it to a translated message.
export const KNOWN_ERROR_CODES = [
  'invalid_credentials',
  'account_locked',
  'email_taken',
  'password_too_weak',
  'password_same_as_email',
  'invalid_email',
  'invalid_reset_link',
  'invalid_refresh_token',
  'rate_limited',
  'profile_limit_reached',
  'self_profile_cannot_be_deleted',
  'profile_not_found',
  'allergy_already_added',
  'ingredient_not_found',
  'wrong_password',
] as const;

export type KnownErrorCode = (typeof KNOWN_ERROR_CODES)[number];

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly code: KnownErrorCode | 'generic',
  ) {
    super(`API error ${status} (${code})`);
  }
}

export function toApiError(status: number, body: unknown): ApiError {
  const code =
    typeof body === 'object' && body !== null && 'code' in body && typeof body.code === 'string' ? body.code : null;
  const known = KNOWN_ERROR_CODES.find((c) => c === code);
  return new ApiError(status, known ?? 'generic');
}

export function errorCodeOf(error: unknown): KnownErrorCode | 'generic' {
  return error instanceof ApiError ? error.code : 'generic';
}
