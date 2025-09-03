import { Routes } from '@angular/router';
import { Home } from './auth/home/home';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { Dashboard } from './dashboard/dashboard';
import { AuthGuard } from './auth/auth.guard';
import { GuestGuard } from './auth/guest.guard';
import { UsersList } from './dashboard/users-list/users-list';


export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'login', component: Login, canActivate: [GuestGuard] },
  { path: 'register', component: Register, canActivate: [GuestGuard] },
  { path: 'dashboard', component: Dashboard, canActivate: [AuthGuard] },
  {path:'owner/users',component:UsersList,canActivate:[AuthGuard]},
  { path: '**', redirectTo: '/home' }
];
