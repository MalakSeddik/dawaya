// Typed client for the Dawaya API. `schema.ts` is generated from the server's OpenAPI document
// (`pnpm --filter api-client generate`); never edit it by hand (CLAUDE.md rule 4).
import createClient, { type ClientOptions } from 'openapi-fetch';

import type { components, paths } from './schema';

export type { components, paths };
export type Schemas = components['schemas'];

export function createDawayaClient(options: ClientOptions = {}) {
  return createClient<paths>(options);
}

export type DawayaClient = ReturnType<typeof createDawayaClient>;
