import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
	providedIn: 'root',
})
export class AuthService {
	private readonly apiUrl = environment.apiUrl;
	private readonly tokenKey = 'lb_token';

	constructor(private http: HttpClient) {}

	register(userName: string, password: string): Observable<boolean> {
		const body = { UserName: userName, Password: password };
		return this.http.post<any>(`${this.apiUrl}/register`, body).pipe(
			map(() => true),
			catchError((err) => of(false))
		);
	}

	login(userName: string, password: string): Observable<boolean> {
		const body = { UserName: userName, Password: password };
		return this.http.post<{ token: string, Message: string }>(`${this.apiUrl}/login`, body).pipe(
			map(res => {
				if (res && res.token) {
					this.setToken(res.token);
					return true;
				}
				return false;
			}),
			catchError(() => of(false))
		);
	}

	logout(): void {
		localStorage.removeItem(this.tokenKey);
	}

	getToken(): string | null {
		return localStorage.getItem(this.tokenKey);
	}

	isAuthenticated(): boolean {
		const token = this.getToken();
		if (!token) return false;
		const payload = this.decodeTokenPayload(token);
		if (!payload) return false;
		if (payload.exp) {
			const exp = Number(payload.exp);
			// exp is in seconds since epoch
			return Date.now() < exp * 1000;
		}
		return true;
	}

	getAuthHeaders(): { headers: HttpHeaders } | {} {
		const token = this.getToken();
		if (!token) return {};
		return { headers: new HttpHeaders({ Authorization: `Bearer ${token}` }) };
	}

	getUserIdFromToken(): string | null {
		const token = this.getToken();
		const payload = token && this.decodeTokenPayload(token);
		if (!payload) return null;
		// In backend the Name claim holds user.Id as string
		return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['name'] || payload['unique_name'] || null;
	}

	hasRole(role: string): boolean {
		const token = this.getToken();
		const payload = token && this.decodeTokenPayload(token);
		if (!payload) return false;
		const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || [];
		return roles.includes(role);
	}

	hasAnyRole(rolesToCheck: string[]): boolean {
		const token = this.getToken();
		const payload = token && this.decodeTokenPayload(token);
		if (!payload) return false;
		const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || [];
		return rolesToCheck.some(role => roles.includes(role));
	}

	private setToken(token: string) {
		localStorage.setItem(this.tokenKey, token);
	}

	private decodeTokenPayload(token: string): any | null {
		try {
			const parts = token.split('.');
			if (parts.length < 2) return null;
			const payload = parts[1];
			// Add padding if necessary
			const padded = payload.replace(/-/g, '+').replace(/_/g, '/');
			const pad = padded.length % 4;
			const base64 = pad === 0 ? padded : padded + '='.repeat(4 - pad);
			const json = atob(base64);
			return JSON.parse(json);
		} catch {
			return null;
		}
	}
}
