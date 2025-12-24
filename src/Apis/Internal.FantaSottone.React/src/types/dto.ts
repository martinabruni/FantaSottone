import { GameStatus, RuleType } from "./entities";

// Auth DTOs

export interface LoginRequest {
  username: string;
  accessCode: string;
}

export interface LoginResponse {
  token: string;
  game: {
    Id: number;
    Name: string;
    InitialScore: number;
    Status: GameStatus;
    CreatorPlayerId?: number | null;
    WinnerPlayerId?: number | null;
  };
  player: {
    Id: number;
    GameId: number;
    Username: string;
    IsCreator: boolean;
    CurrentScore: number;
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
  Id: number;
  Username: string;
  CurrentScore: number;
  IsCreator: boolean;
}

export interface RuleWithAssignment {
  rule: {
    Id: number;
    Name: string;
    RuleType: RuleType;
    ScoreDelta: number;
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
    Id: number;
    CurrentScore: number;
  };
  gameStatus: {
    status: GameStatus;
    winnerPlayerId?: number | null;
  };
}

export interface GameStatusResponse {
  game: {
    Id: number;
    Status: GameStatus;
    WinnerPlayerId?: number | null;
  };
  winner: {
    Id: number;
    Username: string;
    CurrentScore: number;
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
