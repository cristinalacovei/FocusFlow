import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  // Adresa la care rulează API-ul .NET
  // Verifică portul în Visual Studio (ex: 7251)
  private apiUrl = 'https://localhost:7251/api/auth';

  constructor(private http: HttpClient) {}

  // Metodă pentru login
  // 'any' este folosit temporar; vom defini interfețe DTO mai târziu
  login(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, data);
  }

  // Metodă pentru register
  register(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, data);
  }

  // Vom adăuga metode pentru a salva/șterge token-ul aici
  saveToken(token: string): void {
    localStorage.setItem('focusflow-token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('focusflow-token');
  }

  logout(): void {
    localStorage.removeItem('focusflow-token');
    // Vom adăuga și navigare la login aici
  }
}
