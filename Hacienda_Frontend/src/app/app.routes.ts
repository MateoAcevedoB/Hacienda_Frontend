import { Routes } from '@angular/router';

import { adminGuard, authGuard, sinSesionGuard } from './core/guards/auth.guard';
import { ShellComponent } from './layout/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [sinSesionGuard],
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'potreros',
        loadComponent: () =>
          import('./pages/potreros/potreros.component').then((m) => m.PotrerosComponent),
      },
      {
        path: 'potreros/:id',
        loadComponent: () =>
          import('./pages/potrero-detalle/potrero-detalle.component').then(
            (m) => m.PotreroDetalleComponent,
          ),
      },
      {
        path: 'ventas',
        loadComponent: () => import('./pages/ventas/ventas.component').then((m) => m.VentasComponent),
      },
      {
        path: 'vacunas',
        loadComponent: () =>
          import('./pages/vacunas/vacunas.component').then((m) => m.VacunasComponent),
      },
      {
        path: 'usuarios',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./pages/usuarios/usuarios.component').then((m) => m.UsuariosComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
