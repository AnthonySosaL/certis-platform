import { Component } from '@angular/core';

import { BRAND_NAME } from '../../core/brand';

@Component({
  imports: [],
  selector: 'app-footer',
  styleUrl: './footer.scss',
  templateUrl: './footer.html',
})
export class Footer {
  protected readonly year = new Date().getFullYear();
  protected readonly brandName = BRAND_NAME;
}
