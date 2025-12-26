import { LoginRequest, LoginResponse } from "@/types/dto";
import { IAuthStrategy, SessionData } from "./AuthStrategy";
import { getRoleFromIsCreator } from "./roles";

const SESSION_KEY = "fantaSottone_session";

export class MockAuthStrategy implements IAuthStrategy {
  private mockStore: Map<
    string,
    { username: string; accessCode: string; response: LoginResponse }
  > = new Map();

  constructor() {
    // Seed with test data
    this.mockStore.set("test1", {
      username: "test1",
      accessCode: "code1",
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
          username: "test1",
          isCreator: true,
          currentScore: 100,
        },
      },
    });

    this.mockStore.set("test2", {
      username: "test2",
      accessCode: "code2",
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
          username: "test2",
          isCreator: false,
          currentScore: 100,
        },
      },
    });
  }

  async login(credentials: LoginRequest): Promise<LoginResponse> {
    // Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    const user = this.mockStore.get(credentials.username);
    if (!user || user.accessCode !== credentials.accessCode) {
      throw new Error("Invalid credentials");
    }

    return user.response;
  }

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
      username: response.player.username,
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
