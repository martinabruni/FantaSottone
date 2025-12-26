import { GameStatus, RuleType } from "./entities";

// Auth DTOs

export interface LoginRequest {
  username: string;
  accessCode: string;
}

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
    username: string;
    isCreator: boolean;
    currentScore: number;
  };
}

// Game DTOs

export interface StartGameRequest {
  name: string;
  initialScore: number;
  players: Array<{
    username: string;
    accessCode: string;
    isCreator?: boolean;
  }>;
  rules: Array<{
    name: string;
    ruleType: RuleType;
    scoreDelta: number;
  }>;
}

export interface StartGameResponse {
  gameId: number;
  credentials: Array<{
    username: string;
    accessCode: string;
    isCreator: boolean;
  }>;
}

export interface LeaderboardEntry {
  id: number;
  username: string;
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
    assignedToUsername: string;
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
    username: string;
    currentScore: number;
  } | null;
}

export interface AssignmentHistoryEntry {
  id: number;
  ruleId: number;
  ruleName: string;
  assignedToPlayerId: number;
  assignedToUsername: string;
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
    username: string;
    currentScore: number;
  };
  leaderboard: Array<{
    id: number;
    username: string;
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
