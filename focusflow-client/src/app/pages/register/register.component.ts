import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
// --- 1. Importă uneltele pentru formulare ---
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  // --- 2. Adaugă ReactiveFormsModule la 'imports' ---
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  // --- 3. Definește formularul ---
  registerForm: FormGroup;
  registerError: string = ''; // Pentru erori
  registerSuccess: string = ''; // Pentru succes

  // --- 4. Injectează serviciile ---
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // --- 5. Inițializează formularul ---
    this.registerForm = this.fb.group({
      // Numele trebuie să se potrivească cu DTO-ul .NET (RegisterRequestDto)
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  // --- 6. Creează metoda onSubmit ---
  onSubmit() {
    if (this.registerForm.valid) {
      this.registerError = '';
      this.registerSuccess = '';

      this.authService.register(this.registerForm.value).subscribe({
        next: (response) => {
          // A mers!
          console.log('Înregistrare reușită!', response);
          this.registerSuccess = 'Cont creat cu succes! Te poți loga acum.';

          // Opțional: redirecționează la login după 2 secunde
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        },
        error: (err) => {
          // Nu a mers.
          console.error('Eroare la înregistrare:', err);
          // Încercăm să afișăm o eroare mai specifică de la .NET
          if (err.error && err.error[0] && err.error[0].description) {
            this.registerError = err.error[0].description;
          } else {
            this.registerError =
              'A apărut o eroare la înregistrare. Încearcă din nou.';
          }
        },
      });
    }
  }
}
