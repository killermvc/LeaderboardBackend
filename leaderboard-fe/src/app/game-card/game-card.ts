import { Component, input } from '@angular/core';
import { Game } from '../core/game-service';

@Component({
  selector: 'app-game-card',
  standalone: true,
  templateUrl: './game-card.html',
  styleUrl: './game-card.scss'
})
export class GameCard {
  game = input.required<Game>();
}
