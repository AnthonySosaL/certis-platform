import { Service, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'app-theme';

// The brand defaults to light mode. Dark mode is fully wired (the .dark
// class below plus Material's color-scheme) but is not the default look
// yet - flip the initial signal value when that changes.
@Service()
export class Theme {
  readonly mode = signal<ThemeMode>(this.readInitial());

  constructor() {
    this.apply(this.mode());
  }

  toggle(): void {
    const next: ThemeMode = this.mode() === 'light' ? 'dark' : 'light';
    this.mode.set(next);
    this.apply(next);
  }

  private readInitial(): ThemeMode {
    if (typeof window === 'undefined') return 'light';
    return (localStorage.getItem(STORAGE_KEY) as ThemeMode | null) ?? 'light';
  }

  private apply(mode: ThemeMode): void {
    if (typeof document === 'undefined') return;
    document.documentElement.classList.toggle('dark', mode === 'dark');
    localStorage.setItem(STORAGE_KEY, mode);
  }
}
