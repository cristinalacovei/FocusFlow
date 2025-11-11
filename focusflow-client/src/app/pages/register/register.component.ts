import {
  Component,
  ElementRef,
  Renderer2,
  ViewChild,
  OnInit,
  AfterViewInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  size: number;
  alpha: number;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('particlesCanvas') particlesCanvas!: ElementRef<HTMLCanvasElement>;

  registerForm!: FormGroup;
  registerError = '';
  registerSuccess = '';

  private particles: Particle[] = [];
  private animationFrameId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private renderer: Renderer2,
    private host: ElementRef
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngAfterViewInit(): void {
    const page = this.host.nativeElement.querySelector('.register-page');
    if (page) {
      this.renderer.addClass(page, 'animate-entry');
    }

    if (this.particlesCanvas) {
      this.initParticles();
      this.animateParticles();
      window.addEventListener('resize', this.resizeCanvas);
    }
  }

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
    const count = 65;

    for (let i = 0; i < count; i++) {
      this.particles.push({
        x: Math.random() * canvas.width,
        y: Math.random() * canvas.height,
        vx: (Math.random() - 0.5) * 0.25,
        vy: (Math.random() - 0.5) * 0.25,
        size: Math.random() * 1.6 + 0.6,
        alpha: Math.random() * 0.4 + 0.2,
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

  ngOnDestroy(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
    window.removeEventListener('resize', this.resizeCanvas);
  }

  onSubmit(): void {
    if (!this.registerForm.valid) {
      this.registerError = 'Please fill in all fields correctly.';
      this.registerSuccess = '';
      return;
    }

    this.registerError = '';
    this.registerSuccess = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.registerSuccess = 'Account created successfully! Redirecting...';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        if (err?.error?.[0]?.description) {
          this.registerError = err.error[0].description;
        } else {
          this.registerError =
            'There was an error while creating your account. Please try again.';
        }
      },
    });
  }
}
