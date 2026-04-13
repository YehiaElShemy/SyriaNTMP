import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

const ALLOWED_PATHS = [
  /^\/auth\/login-page(\/.*)?$/, // matches /auth/login-page/:id
  /^\/syria-stats(\/.*)?$/,        // matches /syria-stats and any sub-paths
];

export const syriaRouteGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);
  const currentUrl = state.url;

  const isAllowed = ALLOWED_PATHS.some(pattern => pattern.test(currentUrl));

  if (isAllowed) {
    return true;
  }

  router.navigate(['/notfound']);
  return false;
};
