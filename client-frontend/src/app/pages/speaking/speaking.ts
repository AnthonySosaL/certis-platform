import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { SpeakingApi, SpeakingScenario, SpeakingTurn } from '../../core/speaking-api';

type Stage = 'loading' | 'picking' | 'chatting';

// AI-only roleplay practice (2026-08-28) - the first buildable slice of
// "speaking practice, Cambridge-exam style". Matching with another real
// user is a separate, materially bigger feature deliberately not
// attempted here - see docs/PENDING_IDEAS.md.
@Component({
  selector: 'app-speaking',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  templateUrl: './speaking.html',
  styleUrl: './speaking.scss',
})
export class Speaking implements OnInit {
  private readonly speakingApi = inject(SpeakingApi);
  private readonly fb = inject(FormBuilder);

  protected readonly stage = signal<Stage>('loading');
  protected readonly scenarios = signal<SpeakingScenario[]>([]);
  protected readonly activeScenario = signal<SpeakingScenario | null>(null);
  protected readonly messages = signal<SpeakingTurn[]>([]);
  protected readonly sending = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly messageForm = this.fb.nonNullable.group({
    message: ['', [Validators.required]],
  });

  async ngOnInit(): Promise<void> {
    try {
      this.scenarios.set(await this.speakingApi.getScenarios());
      this.stage.set('picking');
    } catch {
      this.errorMessage.set('Could not load speaking scenarios. Please try again.');
    }
  }

  async pick(scenario: SpeakingScenario): Promise<void> {
    this.activeScenario.set(scenario);
    this.messages.set([]);
    this.errorMessage.set(null);
    this.stage.set('chatting');
    this.sending.set(true);

    // Kicks the examiner off with an opening line - sent to the backend
    // as the "user" turn Groq replies to, but not shown as a message the
    // student typed themselves.
    try {
      const reply = await this.speakingApi.reply(scenario.id, [], "Hi, I'm ready to start.");
      this.messages.set([{ role: 'assistant', content: reply }]);
    } catch {
      this.errorMessage.set("The AI partner isn't available right now. Please try again.");
    } finally {
      this.sending.set(false);
    }
  }

  backToScenarios(): void {
    this.activeScenario.set(null);
    this.messages.set([]);
    this.stage.set('picking');
  }

  async onSubmit(): Promise<void> {
    if (this.messageForm.invalid || this.sending()) return;
    const message = this.messageForm.getRawValue().message.trim();
    if (!message) return;
    this.messageForm.reset({ message: '' });
    await this.send(message);
  }

  private async send(message: string): Promise<void> {
    const scenario = this.activeScenario();
    if (!scenario) return;

    const history = this.messages();
    this.messages.set([...history, { role: 'user', content: message }]);
    this.sending.set(true);
    this.errorMessage.set(null);

    try {
      const reply = await this.speakingApi.reply(scenario.id, history, message);
      this.messages.update((current) => [...current, { role: 'assistant', content: reply }]);
    } catch {
      this.errorMessage.set("The AI partner isn't available right now. Please try again.");
    } finally {
      this.sending.set(false);
    }
  }
}
