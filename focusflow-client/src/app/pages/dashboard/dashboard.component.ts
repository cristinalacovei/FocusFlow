import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { ActivityService } from '../../services/activity.service';
import {
  FocusSessionService,
  StartSessionPayload,
} from '../../services/focus-session.service';
import { SafeUrlPipe } from '../../safe-url.pipe'; // <- calea pentru src/app/safe-url.pipe.ts

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SafeUrlPipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  activities: any[] = [];
  activityForm: FormGroup;
  apiError = '';

  sessionForm: FormGroup;
  isInSession = false;
  currentSessionId: number | null = null;
  currentPlaylist: string | null = null;
  currentPlaylistEmbedUrl: string | null = null;

  constructor(
    private activityService: ActivityService,
    private focusSessionService: FocusSessionService,
    private fb: FormBuilder
  ) {
    this.activityForm = this.fb.group({
      name: ['', Validators.required],
    });

    this.sessionForm = this.fb.group({
      activityId: [null, Validators.required],
      mood: ['Motivat', Validators.required],
      durationMinutes: [25, Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadActivities();
  }

  loadActivities(): void {
    this.apiError = '';
    this.activityService.getActivities().subscribe({
      next: (data) => (this.activities = data),
      error: () => (this.apiError = 'Nu am putut încărca activitățile.'),
    });
  }

  onAddActivity(): void {
    if (!this.activityForm.valid) return;

    this.apiError = '';
    this.activityService.createActivity(this.activityForm.value).subscribe({
      next: () => {
        this.loadActivities();
        this.activityForm.reset();
      },
      error: () => (this.apiError = 'Eroare la salvarea activității.'),
    });
  }

  onDeleteActivity(id: number): void {
    this.apiError = '';
    this.activityService.deleteActivity(id).subscribe({
      next: () => this.loadActivities(),
      error: () => (this.apiError = 'Eroare la ștergerea activității.'),
    });
  }

  onStartSession(): void {
    if (this.sessionForm.invalid) {
      this.apiError = 'Te rog selectează o activitate.';
      return;
    }

    this.apiError = '';
    const payload: StartSessionPayload = this.sessionForm.value;

    this.focusSessionService.startSession(payload).subscribe({
      next: (response) => {
        console.log('Sesiune începută!', response);

        this.isInSession = true;
        this.currentSessionId = response.sessionId;
        this.currentPlaylist = response.spotifyPlaylistUri || null;

        if (this.currentPlaylist?.startsWith('spotify:playlist:')) {
          const id = this.currentPlaylist.split(':')[2];
          this.currentPlaylistEmbedUrl = `https://open.spotify.com/embed/playlist/${id}`;
        } else {
          this.currentPlaylistEmbedUrl = null;
        }
      },
      error: (err) => {
        console.error('Eroare la pornirea sesiunii:', err);
        if (err.error && err.error.error) {
          this.apiError = err.error.error;
        } else {
          this.apiError = 'Eroare necunoscută la pornirea sesiunii.';
        }
      },
    });
  }

  onEndSession(): void {
    this.isInSession = false;
    this.currentSessionId = null;
    this.currentPlaylist = null;
    this.currentPlaylistEmbedUrl = null;
  }
}
