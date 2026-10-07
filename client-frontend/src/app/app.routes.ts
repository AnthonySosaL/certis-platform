import { Routes } from '@angular/router';

import { Home } from './pages/home/home';
import { About } from './pages/about/about';
import { ForgotPassword } from './pages/forgot-password/forgot-password';
import { PlacementTest } from './pages/test/placement-test/placement-test';
import { TestResultPage } from './pages/test/test-result/test-result';
import { Reinforcement } from './pages/test/reinforcement/reinforcement';
import { Speaking } from './pages/speaking/speaking';
import { Courses } from './pages/courses/courses';
import { Course } from './pages/course/course';
import { Dashboard } from './pages/dashboard/dashboard';
import { AdminDashboard } from './pages/admin/admin';
import { authGuard } from './core/auth.guard';
import { adminGuard } from './core/admin.guard';

export const routes: Routes = [
  { path: '', component: Home, title: 'Certis — Free English Placement Test (A2 to C1) & Practice' },
  { path: 'about', component: About, title: 'How the Certis placement test works — Certis' },
  { path: 'forgot-password', component: ForgotPassword },
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'admin', component: AdminDashboard, canActivate: [adminGuard] },
  { path: 'test', component: PlacementTest, canActivate: [authGuard] },
  { path: 'test/results', component: TestResultPage, canActivate: [authGuard] },
  { path: 'test/reinforce/:level/:skill', component: Reinforcement, canActivate: [authGuard] },
  { path: 'speaking', component: Speaking, canActivate: [authGuard] },
  { path: 'courses', component: Courses, canActivate: [authGuard] },
  { path: 'courses/:level/:skill', component: Course, canActivate: [authGuard] },
];
