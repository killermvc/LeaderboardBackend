import { Routes } from '@angular/router';

import { RegisterPage } from './auth/register-page/register-page';
import { LoginPage } from './auth/login-page/login-page';
import { HomePage } from './home-page/home-page';
import { Games } from './games/games';
import { MyGames } from './my-games/my-games';
import { GameDetail } from './game-detail/game-detail';

export const routes: Routes = [
	  {
		path: 'auth/register',
		component: RegisterPage
	  },
	  {
		path: 'auth/login',
		component: LoginPage
	  },
	  {
		path: 'games',
		component: Games
	  },
	  {
		path: 'games/:id',
		component: GameDetail
	  },
	  {
		path: 'mygames',
		component: MyGames
	  },
	  {
		path: '',
		component: HomePage
	  }
];
