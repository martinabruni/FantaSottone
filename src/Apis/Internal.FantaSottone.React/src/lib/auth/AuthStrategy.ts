import { LoginResponse } from "@/types/dto";
import { Role } from "./roles";

export interface SessionData {
  token: string;
  playerId: number;
  gameId: number;
  email: string; // ✅ CAMBIATO: username -> email
  role: Role;
}

export interface IAuthStrategy {
  // ❌ RIMOSSO: login con LoginRequest (non esiste più)
  logout(): Promise<void>;
  getSession(): SessionData | null;
  saveSession(response: LoginResponse): void;
  clearSession(): void;
  isAuthenticated(): boolean;
}
