import { LoginResponse } from "@/types/dto";
import { IAuthStrategy, SessionData } from "./AuthStrategy";
import { getRoleFromIsCreator } from "./roles";

const SESSION_KEY = "fantaSottone_session";

export class JwtAuthStrategy implements IAuthStrategy {
  constructor() {}

  // ❌ RIMOSSO: metodo login (ora si usa solo Google OAuth)

  async logout(): Promise<void> {
    this.clearSession();
  }

  getSession(): SessionData | null {
    const stored = localStorage.getItem(SESSION_KEY);
    if (!stored) return null;

    try {
      return JSON.parse(stored) as SessionData;
    } catch {
      return null;
    }
  }

  saveSession(response: LoginResponse): void {
    const session: SessionData = {
      token: response.token,
      playerId: response.player.id,
      gameId: response.player.gameId,
      email: response.player.email, // ✅ CAMBIATO: username -> email
      role: getRoleFromIsCreator(response.player.isCreator),
    };

    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  }

  clearSession(): void {
    localStorage.removeItem(SESSION_KEY);
  }

  isAuthenticated(): boolean {
    return this.getSession() !== null;
  }
}
