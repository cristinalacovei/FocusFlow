import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

// Definim tipurile de date pentru cererile noastre
// Acestea se potrivesc cu DTO-urile din .NET

export interface StartSessionPayload {
  activityId: number;
  durationMinutes: number;
  mood: string;
}

export interface SessionFeedbackPayload {
  sessionId: number;
  productivityRating: number;
  musicFeedback: string; // 'Ajutat', 'Neutru', 'Distras'
}

@Injectable({
  providedIn: 'root',
})
export class FocusSessionService {
  private apiUrl = 'https://localhost:7251/api/session';

  constructor(private http: HttpClient) {}

  // POST: /api/session/start
  startSession(payload: StartSessionPayload): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/start`, payload);
  }

  // POST: /api/session/feedback
  submitFeedback(payload: SessionFeedbackPayload): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/feedback`, payload);
  }
}
