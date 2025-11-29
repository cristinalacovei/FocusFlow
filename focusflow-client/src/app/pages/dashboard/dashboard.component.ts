import {
  Component,
  OnInit,
  AfterViewInit,
  OnDestroy,
  ElementRef,
  Renderer2,
  ViewChild,
} from '@angular/core';
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
import { SafeUrlPipe } from '../../safe-url.pipe';

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  size: number;
  alpha: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SafeUrlPipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('particlesCanvas') particlesCanvas!: ElementRef<HTMLCanvasElement>;

  activities: any[] = [];
  activityForm: FormGroup;
  sessionForm: FormGroup;
  apiError = '';

  isInSession = false;
  currentSessionId: number | null = null;
  currentPlaylist: string | null = null;
  currentPlaylistEmbedUrl: string | null = null;

  // --- TIMER PROPERTIES --- // <--- MODIFICARE
  timeLeft: number = 0; // Timpul rămas în secunde
  displayTime: string = '00:00'; // Timpul formatat pentru afișare (MM:SS)
  private timerInterval: any;
  // Folosim un sunet standard de alarmă (poți înlocui URL-ul cu un fișier local din assets)
  private alarmSound = new Audio(
    'https://actions.google.com/sounds/v1/alarms/beep_short.ogg'
  );

  private particles: Particle[] = [];
  private animationFrameId: number | null = null;

  constructor(
    private activityService: ActivityService,
    private focusSessionService: FocusSessionService,
    private fb: FormBuilder,
    private host: ElementRef,
    private renderer: Renderer2
  ) {
    this.activityForm = this.fb.group({
      name: ['', Validators.required],
    });

    this.sessionForm = this.fb.group({
      activityId: [null, Validators.required],
      mood: ['Motivated', Validators.required],
      durationMinutes: [25, Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadActivities();
  }

  ngAfterViewInit(): void {
    const page = this.host.nativeElement.querySelector('.dashboard-page');
    if (page) {
      this.renderer.addClass(page, 'animate-entry');
    }

    if (this.particlesCanvas) {
      this.initParticles();
      this.animateParticles();
      window.addEventListener('resize', this.resizeCanvas);
    }
  }

  ngOnDestroy(): void {
    this.stopTimer(); // <--- MODIFICARE: Oprim timer-ul dacă ieșim din pagină
    if (this.animationFrameId) cancelAnimationFrame(this.animationFrameId);
    window.removeEventListener('resize', this.resizeCanvas);
  }

  /* ========== PARTICLES ========== */
  // ... (Codul pentru particule rămâne neschimbat)
  private resizeCanvas = () => {
    const canvas = this.particlesCanvas.nativeElement;
    const rect = canvas.parentElement?.getBoundingClientRect();
    if (!rect) return;
    canvas.width = rect.width;
    canvas.height = rect.height;
  };

  private initParticles(): void {
    const canvas = this.particlesCanvas.nativeElement;
    const rect = canvas.parentElement?.getBoundingClientRect();
    if (!rect) return;

    canvas.width = rect.width;
    canvas.height = rect.height;

    this.particles = [];
    const count = 80;

    for (let i = 0; i < count; i++) {
      this.particles.push({
        x: Math.random() * canvas.width,
        y: Math.random() * canvas.height,
        vx: (Math.random() - 0.5) * 0.25,
        vy: (Math.random() - 0.5) * 0.25,
        size: Math.random() * 1.8 + 0.5,
        alpha: Math.random() * 0.4 + 0.15,
      });
    }
  }

  private animateParticles(): void {
    const canvas = this.particlesCanvas.nativeElement;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const render = () => {
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      this.particles.forEach((p) => {
        p.x += p.vx;
        p.y += p.vy;

        if (p.x < 0) p.x = canvas.width;
        if (p.x > canvas.width) p.x = 0;
        if (p.y < 0) p.y = canvas.height;
        if (p.y > canvas.height) p.y = 0;

        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(34, 197, 94, ${p.alpha})`;
        ctx.fill();
      });

      this.animationFrameId = requestAnimationFrame(render);
    };

    render();
  }

  /* ========== TIMER LOGIC ========== */ // <--- SECȚIUNE NOUĂ

  startTimer(durationMinutes: number): void {
    this.stopTimer(); // Curățăm orice timer anterior
    this.timeLeft = durationMinutes * 60; // Convertim în secunde
    this.updateDisplayTime(); // Afișăm timpul inițial imediat

    this.timerInterval = setInterval(() => {
      if (this.timeLeft > 0) {
        this.timeLeft--;
        this.updateDisplayTime();
      } else {
        // Timpul a expirat
        this.stopTimer();
        this.playAlarm();
        // Opțional: Aici poți declanșa automat "onEndSession" sau afișa un mesaj
        // alert("Focus session complete!");
      }
    }, 1000);
  }

  stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  private updateDisplayTime(): void {
    const minutes = Math.floor(this.timeLeft / 60);
    const seconds = this.timeLeft % 60;
    // Formatăm cu zero în față (ex: 05:09)
    this.displayTime = `${this.padZero(minutes)}:${this.padZero(seconds)}`;
  }

  private padZero(num: number): string {
    return num < 10 ? `0${num}` : `${num}`;
  }

  private playAlarm(): void {
    this.alarmSound
      .play()
      .catch((err) => console.error('Error playing audio:', err));
  }

  /* ========== API LOGIC ========== */

  loadActivities(): void {
    this.apiError = '';
    this.activityService.getActivities().subscribe({
      next: (data) => (this.activities = data),
      error: () =>
        (this.apiError = 'Could not load activities. Please try again.'),
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
      error: () =>
        (this.apiError = 'Error while saving the activity. Please try again.'),
    });
  }

  onDeleteActivity(id: number): void {
    this.apiError = '';
    this.activityService.deleteActivity(id).subscribe({
      next: () => this.loadActivities(),
      error: () =>
        (this.apiError =
          'Error while deleting the activity. Please try again.'),
    });
  }

  onStartSession(): void {
    if (this.sessionForm.invalid) {
      this.apiError = 'Please select an activity to start your focus session.';
      return;
    }

    this.apiError = '';
    const payload: StartSessionPayload = this.sessionForm.value;

    this.focusSessionService.startSession(payload).subscribe({
      next: (response) => {
        this.isInSession = true;
        this.currentSessionId = response.sessionId;
        this.currentPlaylist = response.spotifyPlaylistUri || null;

        if (this.currentPlaylist?.startsWith('spotify:playlist:')) {
          const id = this.currentPlaylist.split(':')[2];
          this.currentPlaylistEmbedUrl = `https://open.spotify.com/embed/playlist/${id}`;
        } else {
          this.currentPlaylistEmbedUrl = null;
        }

        // --- PORNIRE TIMER --- // <--- MODIFICARE
        // Folosim durata selectată din formular
        this.startTimer(payload.durationMinutes);
      },
      error: (err) => {
        if (err.error && err.error.error) {
          this.apiError = err.error.error;
        } else {
          this.apiError =
            'Unknown error while starting the session. Please try again.';
        }
      },
    });
  }

  onEndSession(): void {
    this.isInSession = false;
    this.currentSessionId = null;
    this.currentPlaylist = null;
    this.currentPlaylistEmbedUrl = null;

    this.stopTimer(); // <--- MODIFICARE: Oprim timer-ul manual când utilizatorul încheie sesiunea
  }
}
