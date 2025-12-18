import { Component } from '@angular/core';
import { AuthService } from '../core/auth-service';
import { Router } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { octSearch, octFeedPerson} from '@ng-icons/octicons';

@Component({
  selector: 'app-header',
  imports: [ NgIcon],
  providers: [provideIcons({ octSearch, octFeedPerson })],
  templateUrl: './header.html',
  styleUrls: ['./header.scss'],
})
export class Header {
	constructor(public authService: AuthService, private router: Router) {}

	onSearch(query: string) {
		//TODO: wire actual search
		console.log('Header search:', query);
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
}
