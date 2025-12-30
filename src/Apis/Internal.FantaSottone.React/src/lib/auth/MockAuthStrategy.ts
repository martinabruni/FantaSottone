import { LoginResponse } from "@/types/dto";
import { IAuthStrategy, SessionData } from "./AuthStrategy";
import { getRoleFromIsCreator } from "./roles";

const SESSION_KEY = "fantaSottone_session";

// ⚠️ DEPRECATO: Questa strategia non è più usata con Google OAuth
// Mantenuta solo per retrocompatibilità durante lo sviluppo
export class MockAuthStrategy implements IAuthStrategy {
  private mockStore: Map<
    string,
    { email: string; response: LoginResponse }
  > = new Map();

  constructor() {
    // Seed with test data usando email
    this.mockStore.set("test1@example.com", {
      email: "test1@example.com",
      response: {
        token: "mock-token-test1",
        game: {
          id: 1,
          name: "Test Game",
          initialScore: 100,
          status: 2, // Started
          creatorPlayerId: 1,
          winnerPlayerId: null,
        },
        player: {
          id: 1,
          gameId: 1,
          email: "test1@example.com", // ✅ CAMBIATO: username -> email
          isCreator: true,
          currentScore: 100,
        },
      },
    });

    this.mockStore.set("test2@example.com", {
      email: "test2@example.com",
      response: {
        token: "mock-token-test2",
        game: {
          id: 1,
          name: "Test Game",
          initialScore: 100,
          status: 2,
          creatorPlayerId: 1,
          winnerPlayerId: null,
        },
        player: {
          id: 2,
          gameId: 1,
          email: "test2@example.com", // ✅ CAMBIATO: username -> email
          isCreator: false,
          currentScore: 100,
        },
      },
    });
  }

  // ❌ RIMOSSO: metodo login tradizionale

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
