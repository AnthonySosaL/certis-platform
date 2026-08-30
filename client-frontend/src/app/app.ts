import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { Navbar } from './layout/navbar/navbar';
import { Footer } from './layout/footer/footer';
import { TipsTicker } from './layout/tips-ticker/tips-ticker';

@Component({
  imports: [RouterOutlet, Navbar, TipsTicker, Footer],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {}
