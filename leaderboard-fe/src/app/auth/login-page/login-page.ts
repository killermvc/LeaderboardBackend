import { Component, inject } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/auth-service';

@Component({
  selector: 'app-login-page',
  imports: [RouterModule],
  templateUrl: './login-page.html',
  styleUrls: ['./login-page.scss'],
})
export class LoginPage {
	authService = inject(AuthService);

	constructor(private router: Router) { }

	handleSubmit() {
		const userName = (document.getElementById('username') as HTMLInputElement).value;
		const password = (document.getElementById('password') as HTMLInputElement).value;

		this.authService.login(userName, password).subscribe(success => {
			if (success) {
				this.router.navigate(['/']);
			} else {
				alert('Login failed. Please check your credentials and try again.');
			}
		});
	}

}
