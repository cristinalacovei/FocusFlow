import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
// 1. Importă serviciul și uneltele de rutare
import { SpotifyService } from '../../services/spotify.service';
import { ActivatedRoute, RouterLink } from '@angular/router'; // RouterLink pentru link-uri

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink], // 2. Adaugă RouterLink
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  // 3. Implementează OnInit

  connectionMessage: string = '';

  // 4. Injectează serviciile
  constructor(
    private spotifyService: SpotifyService,
    private route: ActivatedRoute // Pentru a citi query params (ex: ?spotify=success)
  ) {}

  // 5. Verifică dacă am fost redirecționați de la Spotify
  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (params['spotify'] === 'success') {
        this.connectionMessage = 'Succes! Contul tău Spotify a fost conectat.';
      }
      if (params['spotify'] === 'error') {
        this.connectionMessage = 'A apărut o eroare la conectarea Spotify.';
      }
    });
  }

  // 6. Metoda pentru butonul de conectare
  connectSpotify(): void {
    this.spotifyService.getSpotifyLoginUrl().subscribe({
      next: (response) => {
        // Aici este magia:
        // Backend-ul ne dă URL-ul...
        // ...iar noi redirecționăm browser-ul utilizatorului la acel URL.
        window.location.href = response.redirectUrl;
      },
      error: (err) => {
        console.error('Eroare la obținerea URL-ului Spotify', err);
        this.connectionMessage = 'Nu am putut iniția conexiunea Spotify.';
      },
    });
  }
}
