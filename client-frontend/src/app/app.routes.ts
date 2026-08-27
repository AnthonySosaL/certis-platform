import { Routes } from '@angular/router';

import { Home } from './pages/home/home';
import { About } from './pages/about/about';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { PlacementTest } from './pages/test/placement-test/placement-test';
import { TestResultPage } from './pages/test/test-result/test-result';
import { Reinforcement } from './pages/test/reinforcement/reinforcement';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'about', component: About },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'test', component: PlacementTest, canActivate: [authGuard] },
  { path: 'test/results', component: TestResultPage, canActivate: [authGuard] },
  { path: 'test/reinforce/:level/:skill', component: Reinforcement, canActivate: [authGuard] },
];
