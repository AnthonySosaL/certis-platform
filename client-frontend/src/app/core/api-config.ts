// No Angular environment.ts setup yet (the Angular 22 CLI doesn't
// scaffold one by default) - detect prod vs local dev at runtime from
// the hostname instead. Revisit with a real environment.ts if a staging
// environment is ever needed too.
export const API_BASE_URL =
  typeof location !== 'undefined' && location.hostname !== 'localhost'
    ? 'https://english-c1-api.runasp.net'
    : 'http://localhost:5223';
