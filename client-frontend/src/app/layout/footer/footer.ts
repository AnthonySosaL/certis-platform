import { Component } from '@angular/core';

import { BRAND_NAME } from '../../core/brand';

// Cross-links to the author's other projects, tagged with UTM so traffic
// sent from Certis shows up in their analytics.
const UTM = '?utm_source=certis&utm_medium=footer&utm_campaign=otros_proyectos';
const OTHER_PROJECTS = [
  { label: 'ATLAS Lab - quant trading research', href: `https://atlas-lab-one.vercel.app/${UTM}` },
  { label: "Anthony Sosa's portfolio", href: `https://curricula-fawn.vercel.app/${UTM}` },
];

@Component({
  imports: [],
  selector: 'app-footer',
  styleUrl: './footer.scss',
  templateUrl: './footer.html',
})
export class Footer {
  protected readonly year = new Date().getFullYear();
  protected readonly brandName = BRAND_NAME;
  protected readonly otherProjects = OTHER_PROJECTS;
}
