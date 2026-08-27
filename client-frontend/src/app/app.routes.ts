import { Routes } from '@angular/router';

import { Home } from './pages/home/home';
import { About } from './pages/about/about';
import { ForgotPassword } from './pages/forgot-password/forgot-password';
import { PlacementTest } from './pages/test/placement-test/placement-test';
import { TestResultPage } from './pages/test/test-result/test-result';
import { Reinforcement } from './pages/test/reinforcement/reinforcement';
import { Dashboard } from './pages/dashboard/dashboard';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'about', component: About },
  { path: 'forgot-password', component: ForgotPassword },
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'test', component: PlacementTest, canActivate: [authGuard] },
  { path: 'test/results', component: TestResultPage, canActivate: [authGuard] },
  { path: 'test/reinforce/:level/:skill', component: Reinforcement, canActivate: [authGuard] },
];
