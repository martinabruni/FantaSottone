import { GameStatus, RuleType } from "./entities";

// Auth DTOs - RIMOSSI username e accessCode, ora si usa solo Google OAuth

export interface LoginResponse {
  token: string;
  game: {
    id: number;
    name: string;
    initialScore: number;
    status: GameStatus;
    creatorPlayerId?: number | null;
    winnerPlayerId?: number | null;
  };
  player: {
    id: number;
    gameId: number;
    email: string; // ✅ CAMBIATO: ora si usa email
    isCreator: boolean;
    currentScore: number;
  };
}

// Game DTOs

export interface StartGameRequest {
  name: string;
  initialScore: number;
  // ✅ RIMOSSO: players non ha più username/accessCode
  // Ora i giocatori vengono invitati via email separatamente
  rules: Array<{
    name: string;
    ruleType: RuleType;
    scoreDelta: number;
  }>;
}

export interface StartGameResponse {
  gameId: number;
  // ✅ RIMOSSO: credentials non esiste più
}

export interface LeaderboardEntry {
  id: number;
  email: string; // ✅ CAMBIATO: username -> email
  currentScore: number;
  isCreator: boolean;
}

export interface RuleWithAssignment {
  rule: {
    id: number;
    name: string;
    ruleType: RuleType;
    scoreDelta: number;
  };
  assignment: {
    ruleAssignmentId: number;
    assignedToPlayerId: number;
    assignedToEmail: string; // ✅ CAMBIATO: assignedToUsername -> assignedToEmail
    assignedAt: string;
  } | null;
}

export interface AssignRuleResponse {
  assignment: {
    id: number;
    ruleId: number;
    assignedToPlayerId: number;
    assignedAt: string;
    scoreDeltaApplied: number;
  };
  updatedPlayer: {
    id: number;
    currentScore: number;
  };
  gameStatus: {
    status: GameStatus;
    winnerPlayerId?: number | null;
  };
}

export interface GameStatusResponse {
  game: {
    id: number;
    status: GameStatus;
    winnerPlayerId?: number | null;
  };
  winner: {
    id: number;
    email: string; // ✅ CAMBIATO: username -> email
    currentScore: number;
  } | null;
}

export interface AssignmentHistoryEntry {
  id: number;
  ruleId: number;
  ruleName: string;
  assignedToPlayerId: number;
  assignedToEmail: string; // ✅ CAMBIATO: assignedToUsername -> assignedToEmail
  scoreDeltaApplied: number;
  assignedAt: string;
}

// End Game DTOs
export interface EndGameRequest {}

export interface EndGameResponse {
  game: {
    id: number;
    status: GameStatus;
    winnerPlayerId: number | null;
  };
  winner: {
    id: number;
    email: string; // ✅ CAMBIATO: username -> email
    currentScore: number;
  };
  leaderboard: Array<{
    id: number;
    email: string; // ✅ CAMBIATO: username -> email
    currentScore: number;
    isCreator: boolean;
  }>;
}

// Rule Management DTOs
export interface CreateRuleRequest {
  name: string;
  ruleType: RuleType;
  scoreDelta: number;
}

export interface CreateRuleResponse {
  rule: {
    id: number;
    name: string;
    ruleType: RuleType;
    scoreDelta: number;
  };
}

export interface UpdateRuleRequest {
  name: string;
  ruleType: RuleType;
  scoreDelta: number;
}

export interface UpdateRuleResponse {
  rule: {
    id: number;
    name: string;
    ruleType: RuleType;
    scoreDelta: number;
  };
}
