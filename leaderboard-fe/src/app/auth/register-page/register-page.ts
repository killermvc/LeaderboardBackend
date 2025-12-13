import { Component, inject } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/auth-service';

@Component({
  selector: 'app-register-page',
  imports: [RouterModule],
  templateUrl: './register-page.html',
  styleUrls: ['./register-page.scss'],
})
export class RegisterPage {
	authService = inject(AuthService);

	constructor(private router: Router) { }

	handleSubmit(): void {

		const userName = (document.getElementById('username') as HTMLInputElement).value;
		const password = (document.getElementById('password') as HTMLInputElement).value;

		this.authService.register(userName, password).subscribe(success => {
			if (success) {
				this.router.navigate(['/auth/login']);
			} else {
				alert('Registration failed. Please try again.');
			}
		});
	}
}
