import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service'; // 1. Importă AuthService

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // 2. Injectează AuthService
  const authService = inject(AuthService);

  // 3. Ia token-ul
  const token = authService.getToken();

  // 4. Verifică dacă există un token
  if (token) {
    // Dacă avem token, clonăm cererea și adăugăm noul header
    const clonedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
    // Trimitem mai departe cererea MODIFICATĂ
    return next(clonedReq);
  } else {
    // Dacă nu avem token (ex: cererea de login),
    // trimitem cererea ORIGINALĂ, nemodificată
    return next(req);
  }
};
