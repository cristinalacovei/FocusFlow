import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
// --- 1. Importă uneltele pentru formulare ---
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  // --- 2. Adaugă ReactiveFormsModule la 'imports' ---
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  // --- 3. Definește formularul ---
  loginForm: FormGroup;
  loginError: string = ''; // Pentru a afișa erori

  // --- 4. Injectează FormBuilder, AuthService și Router ---
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // --- 5. Inițializează formularul ---
    this.loginForm = this.fb.group({
      // Numele de aici (username, password) trebuie să se potrivească
      // cu DTO-ul din .NET (LoginRequestDto)
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  // --- 6. Creează metoda onSubmit ---
  onSubmit() {
    if (this.loginForm.valid) {
      // Resetează eroarea
      this.loginError = '';

      // Apelăm serviciul
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          // A mers! Salvăm token-ul
          this.authService.saveToken(response.token);

          // Logăm în consolă (pentru testare)
          console.log('Login reușit!', response.token);

          // Redirecționăm la dashboard (vom crea ruta /dashboard mai târziu)
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          // Nu a mers. Afișăm o eroare
          console.error('Eroare la login:', err);
          this.loginError = 'Numele de utilizator sau parola sunt incorecte.';
        },
      });
    }
  }
}
