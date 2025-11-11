import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  // Injectăm serviciile necesare
  const authService = inject(AuthService);
  const router = inject(Router);

  // Verificăm dacă avem un token
  if (authService.getToken()) {
    // Există token, utilizatorul este logat
    return true;
  } else {
    // Nu există token, utilizatorul nu e logat
    // Îl redirecționăm la /login
    router.navigate(['/login']);
    return false;
  }
};
