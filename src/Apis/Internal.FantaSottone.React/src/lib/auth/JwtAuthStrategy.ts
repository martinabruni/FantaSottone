import { LoginRequest, LoginResponse } from "@/types/dto";
import { IAuthStrategy, SessionData } from "./AuthStrategy";
import { getRoleFromIsCreator } from "./roles";
import { ITransport } from "../http/Transport";

const SESSION_KEY = "fantaSottone_session";

export class JwtAuthStrategy implements IAuthStrategy {
  constructor(private transport: ITransport) {}

  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await this.transport.post<LoginRequest, LoginResponse>(
      "/api/auth/login",
      credentials
    );
    return response;
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
      playerId: response.player.Id,
      gameId: response.player.GameId,
      username: response.player.Username,
      role: getRoleFromIsCreator(response.player.IsCreator),
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
