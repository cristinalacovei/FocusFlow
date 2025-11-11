import { Routes } from '@angular/router';

// Importă componentele
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';

// Importă paznicul
import { authGuard } from './services/auth.guard';
import { ProfileComponent } from './pages/profile/profile.component';

export const routes: Routes = [
  // Rutele de autentificare (publice)
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // Ruta protejată
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard], // Aici atașăm paznicul!
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [authGuard],
  },

  // Ruta de fallback
  // Acum, dacă suntem pe ruta principală, ne trimite la dashboard
  // Paznicul va decide dacă ne lasă sau ne trimite la login
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
];
