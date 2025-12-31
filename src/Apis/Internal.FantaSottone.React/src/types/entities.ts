// Backend entities matching the database schema

export interface GameEntity {
  Id: number;
  Name: string;
  InitialScore: number;
  Status: GameStatus;
  CreatorPlayerId?: number | null;
  WinnerPlayerId?: number | null;
  CreatedAt: string; // ISO date
  UpdatedAt: string; // ISO date
}

export interface PlayerEntity {
  Id: number;
  GameId: number;
  Email: string; // ✅ CAMBIATO: Username -> Email
  // ❌ RIMOSSO: AccessCode (non più necessario)
  IsCreator: boolean;
  CurrentScore: number;
  CreatedAt: string; // ISO date
  UpdatedAt: string; // ISO date
}

export interface RuleEntity {
  Id: number;
  GameId: number;
  Name: string;
  RuleType: RuleType;
  ScoreDelta: number; // positive for bonus, negative for malus
  CreatedAt: string; // ISO date
  UpdatedAt: string; // ISO date
}

export interface RuleAssignmentEntity {
  Id: number;
  RuleId: number;
  GameId: number;
  AssignedToPlayerId: number;
  ScoreDeltaApplied: number;
  AssignedAt: string; // ISO date
  CreatedAt: string; // ISO date
  UpdatedAt: string; // ISO date
}

// Enums

export enum GameStatus {
  Draft = 1,
  Started = 2,
  Ended = 3,
}

export enum RuleType {
  Bonus = 1,
  Malus = 2,
}
