import { Component } from '@angular/core';
import { AuthService } from '../core/auth-service';
import { Router } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { octSearch, octFeedPerson, octPlus} from '@ng-icons/octicons';

@Component({
  selector: 'app-header',
  imports: [ NgIcon],
  providers: [provideIcons({ octSearch, octFeedPerson, octPlus })],
  templateUrl: './header.html',
  styleUrls: ['./header.scss'],
})
export class Header {
	constructor(public authService: AuthService, private router: Router) {}

	onSearch(query: string) {
		if (query.trim()) {
			this.router.navigate(['/games'], { queryParams: { q: query } });
		} else {
			this.router.navigate(['/games']);
		}
	}

	onLogout() {
		this.authService.logout();
		this.router.navigate(['']);
	}
	routeToLogin() {
		this.router.navigate(['/auth/login']);
	}
	routeToRegister() {
		this.router.navigate(['/auth/register']);
	}
	routeToProfile() {
		this.router.navigate(['/auth/profile'])
	}

	routeToHome() {
		this.router.navigate([''])
	}
	routeToGames() {
		this.router.navigate(['/games'])
	}
	routeToMyGames() {
		this.router.navigate(['/mygames'])
	}
	routeToNewGame() {
		throw new Error('Method not implemented.');
	}
}
