import { RenderMode, ServerRoute } from '@angular/ssr';

// Public pages are prerendered to real HTML at build time so search
// engines can index them; everything behind login stays client-rendered
// (served from index.csr.html - see public/web.config).
export const serverRoutes: ServerRoute[] = [
  { path: '', renderMode: RenderMode.Prerender },
  { path: 'about', renderMode: RenderMode.Prerender },
  { path: '**', renderMode: RenderMode.Client },
];
