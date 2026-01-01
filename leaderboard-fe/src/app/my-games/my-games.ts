import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GameService, Game } from '../core/game-service';
import { AuthService } from '../core/auth-service';
import { GameCard } from '../game-card/game-card';

@Component({
  selector: 'app-my-games',
  standalone: true,
  imports: [CommonModule, GameCard],
  templateUrl: './my-games.html',
  styleUrl: './my-games.scss',
})
export class MyGames implements OnInit {
  private gameService = inject(GameService);
  private authService = inject(AuthService);
  private router = inject(Router);

  games = signal<Game[]>([]);
  loading = signal<boolean>(true);
  isAuthenticated = signal<boolean>(false);

  ngOnInit() {
    if (!this.authService.isAuthenticated()) {
      this.loading.set(false);
      return;
    }
    this.isAuthenticated.set(true);
    this.loadMyGames();
  }

  loadMyGames() {
    const userIdStr = this.authService.getUserIdFromToken();
    if (!userIdStr) {
      console.error('User ID not found in token');
      this.loading.set(false);
      return;
    }

    const userId = parseInt(userIdStr, 10);
    if (isNaN(userId)) {
      console.error('Invalid User ID format');
      this.loading.set(false);
      return;
    }

    this.gameService.getGamesByPlayer(userId).subscribe({
      next: (data) => {
        this.games.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load my games', err);
        this.loading.set(false);
      }
    });
  }

  viewGame(gameId: number) {
    this.router.navigate(['/games', gameId]);
  }

  goToLogin() {
    this.router.navigate(['/auth/login']);
  }
}
