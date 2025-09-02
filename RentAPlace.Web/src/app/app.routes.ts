
import { Routes } from '@angular/router';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { Dashboard } from './dashboard/dashboard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'dashboard', component: Dashboard },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  {path:'home', loadComponent:()=>import('./auth/home/home').then(m=>m.Home)}
];
