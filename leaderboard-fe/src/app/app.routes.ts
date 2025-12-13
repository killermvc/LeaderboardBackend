import { Routes } from '@angular/router';

import { RegisterPage } from './auth/register-page/register-page';
import { LoginPage } from './auth/login-page/login-page';
import { HomePage } from './home-page/home-page';

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
		path: '',
		component: HomePage
	  }
];
