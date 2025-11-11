import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SpotifyService {
  private apiUrl = 'https://localhost:7251/api/spotify';

  constructor(private http: HttpClient) {}

  /**
   * Pasul 1: Cere backend-ului nostru URL-ul de login Spotify.
   */
  getSpotifyLoginUrl(): Observable<{ redirectUrl: string }> {
    // Interceptor-ul va adăuga token-ul nostru JWT
    return this.http.get<{ redirectUrl: string }>(`${this.apiUrl}/login`);
  }
}
