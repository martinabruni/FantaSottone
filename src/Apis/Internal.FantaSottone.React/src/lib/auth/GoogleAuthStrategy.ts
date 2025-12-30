import { IAuthStrategy, SessionData } from "./AuthStrategy";
import { LoginResponse } from "@/types/dto";
import { ITransport } from "@/lib/http/Transport";

export interface GoogleAuthResponse {
  token: string;
  email: string;
  userId: number;
  isFirstLogin: boolean;
}

export class GoogleAuthStrategy implements IAuthStrategy {
  private readonly SESSION_KEY = "google_auth_session";
  private transport: ITransport;

  constructor(transport: ITransport) {
    this.transport = transport;
  }

  async loginWithGoogle(idToken: string): Promise<GoogleAuthResponse> {
    const response = await this.transport.post<
      { idToken: string },
      GoogleAuthResponse
    >("/api/GoogleAuth/login", { idToken });

    return response;
  }

  async logout(): Promise<void> {
    this.clearSession();
  }

  getSession(): SessionData | null {
    const sessionJson = localStorage.getItem(this.SESSION_KEY);
    if (!sessionJson) return null;

    try {
      return JSON.parse(sessionJson) as SessionData;
    } catch {
      return null;
    }
  }

  saveSession(response: LoginResponse): void {
    const session: SessionData = {
      token: response.token,
      playerId: response.player.id,
      gameId: response.game.id,
      email: response.player.email, // ✅ CAMBIATO: username -> email
      role: response.player.isCreator ? "creator" : "player",
    };
    localStorage.setItem(this.SESSION_KEY, JSON.stringify(session));
  }

  saveGoogleSession(response: GoogleAuthResponse, email: string): void {
    const session: SessionData = {
      token: response.token,
      playerId: 0, // Google users don't have a player ID initially
      gameId: 0, // Google users don't have a game ID initially
      email: email, // ✅ email da Google
      role: "player",
    };
    localStorage.setItem(this.SESSION_KEY, JSON.stringify(session));
  }

  clearSession(): void {
    localStorage.removeItem(this.SESSION_KEY);
  }

  isAuthenticated(): boolean {
    return this.getSession() !== null;
  }
}
