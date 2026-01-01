import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { GameService, Game } from '../core/game-service';
import { toSignal } from '@angular/core/rxjs-interop';
import { GameCard } from '../game-card/game-card';

@Component({
  selector: 'app-games',
  standalone: true,
  imports: [CommonModule, GameCard],
  templateUrl: './games.html',
  styleUrl: './games.scss',
})
export class Games implements OnInit {
  private gameService = inject(GameService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  games = signal<Game[]>([]);
  searchQuery = signal<string>('');

  filteredGames = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const allGames = this.games();
    if (!query) return allGames;
    return allGames.filter(g => g.name.toLowerCase().includes(query));
  });

  ngOnInit() {
    this.loadGames();

    this.route.queryParams.subscribe(params => {
      const q = params['q'];
      if (q) {
        this.searchQuery.set(q);
      } else {
        this.searchQuery.set('');
      }
    });
  }

  loadGames() {
    // Fetching a reasonable number of games. Pagination could be added later.
    this.gameService.getAllGames(100, 0).subscribe({
      next: (data) => this.games.set(data),
      error: (err) => console.error('Failed to load games', err)
    });
  }

  viewGame(gameId: number) {
    this.router.navigate(['/games', gameId]);
  }
}
