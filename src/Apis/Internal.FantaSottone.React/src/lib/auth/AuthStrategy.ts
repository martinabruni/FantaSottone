import { LoginRequest, LoginResponse } from "@/types/dto";
import { Role } from "./roles";

export interface SessionData {
  token: string;
  playerId: number;
  gameId: number;
  username: string;
  role: Role;
}

export interface IAuthStrategy {
  login(credentials: LoginRequest): Promise<LoginResponse>;
  logout(): Promise<void>;
  getSession(): SessionData | null;
  saveSession(response: LoginResponse): void;
  clearSession(): void;
  isAuthenticated(): boolean;
}
