import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ActivityService {
  // Adresa API-ului nostru (fără /auth)
  private apiUrl = 'https://localhost:7251/api/activities';

  constructor(private http: HttpClient) {}

  // GET: /api/activities
  // Ia toate activitățile utilizatorului logat
  getActivities(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // POST: /api/activities
  // Creează o activitate nouă
  createActivity(data: { name: string }): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  // DELETE: /api/activities/{id}
  // Șterge o activitate
  deleteActivity(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
