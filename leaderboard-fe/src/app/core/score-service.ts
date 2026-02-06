import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth-service';

export interface LeaderboardEntry {
  userId: number;
  userName?: string | null;
  score: number;
}

export interface RankResponse {
  gameId: number;
  userId: number;
  rank: number | null;
}

export interface TopPlayersResponse {
  gameId: number;
  limit: number;
  leaderboard: LeaderboardEntry[];
}

export interface ScoreUser {
  id: number;
  username?: string | null;
}

export interface ScoreGame {
  id: number;
  name: string;
}

export enum ScoreStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2
}

export interface ScoreRecord {
  id: number;
  user: ScoreUser;
  game: ScoreGame;
  value: number;
  dateAchieved: string;
  status: ScoreStatus;
  reviewedBy?: ScoreUser | null;
  reviewedAt?: string | null;
  rejectionReason?: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class ScoreService {
  // Fallback guards the rare case where scoreApiUrl is omitted in environment files.
  private readonly baseUrl = environment.scoreApiUrl ?? environment.apiUrl.replace(/\/auth$/, '/score');

  constructor(private http: HttpClient, private authService: AuthService) {}

  submitScore(gameId: number, score: number): Observable<string> {
    const payload = { gameId, score };
    return this.http.post<string>(`${this.baseUrl}/submit`, payload, {
      ...this.getAuthOptions(),
      responseType: 'text' as 'json',
    });
  }

  getLeaderboard(gameId: number, limit = 10): Observable<LeaderboardEntry[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<LeaderboardEntry[]>(`${this.baseUrl}/leaderboard/${gameId}`, { params });
  }

  getRank(gameId: number, userId: number): Observable<RankResponse> {
    return this.http.get<RankResponse>(`${this.baseUrl}/leaderboard/${gameId}/rank/${userId}`);
  }

  getTopPlayers(gameId: number, limit: number): Observable<TopPlayersResponse> {
    return this.http.get<TopPlayersResponse>(`${this.baseUrl}/leaderboard/${gameId}/top/${limit}`);
  }

  getScoresByUser(userId: number, limit = 10, offset = 0): Observable<ScoreRecord[]> {
    const params = this.buildPaginationParams(limit, offset);
    return this.http.get<ScoreRecord[]>(`${this.baseUrl}/scores/user/${userId}`, { params });
  }

  getRecentScores(limit = 10, offset = 0): Observable<ScoreRecord[]> {
    const params = this.buildPaginationParams(limit, offset);
    return this.http.get<ScoreRecord[]>(`${this.baseUrl}/scores/recent`, { params });
  }

  /**
   * Get all scores submitted by the current user, including pending ones
   */
  getMySubmissions(limit = 20, offset = 0): Observable<ScoreRecord[]> {
    const params = this.buildPaginationParams(limit, offset);
    return this.http.get<ScoreRecord[]>(`${this.baseUrl}/scores/my-submissions`, {
      params,
      ...this.getAuthOptions()
    });
  }

  private buildPaginationParams(limit: number, offset: number): HttpParams {
    return new HttpParams()
      .set('limit', limit.toString())
      .set('offset', offset.toString());
  }

  private getAuthOptions(): { headers?: HttpHeaders } {
    return this.authService.getAuthHeaders() as { headers?: HttpHeaders };
  }
}
