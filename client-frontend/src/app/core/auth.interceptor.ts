import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { API_BASE_URL } from './api-config';
import { Auth } from './auth';

// Attaches the JWT to every request going to our own API - not to
// third-party requests, so a stray external call never leaks the token.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const token = auth.token();

  if (token && req.url.startsWith(API_BASE_URL)) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};
