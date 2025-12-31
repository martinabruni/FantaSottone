import {
  GameEntity,
  PlayerEntity,
  RuleEntity,
  RuleAssignmentEntity,
  GameStatus,
  RuleType,
} from "@/types/entities";

// In-memory data store for mock server

export class DataStore {
  private games: Map<number, GameEntity> = new Map();
  private players: Map<number, PlayerEntity> = new Map();
  private rules: Map<number, RuleEntity> = new Map();
  private assignments: Map<number, RuleAssignmentEntity> = new Map();

  private nextGameId = 1;
  private nextPlayerId = 1;
  private nextRuleId = 1;
  private nextAssignmentId = 1;

  constructor() {
    this.seedData();
  }

  private seedData() {
    // Create a test game
    const game: GameEntity = {
      Id: this.nextGameId++,
      Name: "Test Game",
      InitialScore: 100,
      Status: GameStatus.Started,
      CreatorPlayerId: 1,
      WinnerPlayerId: null,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.games.set(game.Id, game);

    // Create test players con email
    const player1: PlayerEntity = {
      Id: this.nextPlayerId++,
      GameId: game.Id,
      Email: "test1@example.com", // ✅ CAMBIATO: Username -> Email
      IsCreator: true,
      CurrentScore: 100,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.players.set(player1.Id, player1);

    const player2: PlayerEntity = {
      Id: this.nextPlayerId++,
      GameId: game.Id,
      Email: "test2@example.com", // ✅ CAMBIATO: Username -> Email
      IsCreator: false,
      CurrentScore: 100,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.players.set(player2.Id, player2);

    const player3: PlayerEntity = {
      Id: this.nextPlayerId++,
      GameId: game.Id,
      Email: "test3@example.com", // ✅ CAMBIATO: Username -> Email
      IsCreator: false,
      CurrentScore: 100,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.players.set(player3.Id, player3);

    // Create test rules
    const rule1: RuleEntity = {
      Id: this.nextRuleId++,
      GameId: game.Id,
      Name: "Prima birra",
      RuleType: RuleType.Malus,
      ScoreDelta: -10,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.rules.set(rule1.Id, rule1);

    const rule2: RuleEntity = {
      Id: this.nextRuleId++,
      GameId: game.Id,
      Name: "Primo goal",
      RuleType: RuleType.Bonus,
      ScoreDelta: 20,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.rules.set(rule2.Id, rule2);

    const rule3: RuleEntity = {
      Id: this.nextRuleId++,
      GameId: game.Id,
      Name: "Primo cartellino",
      RuleType: RuleType.Malus,
      ScoreDelta: -15,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.rules.set(rule3.Id, rule3);
  }

  // Game methods
  getGame(id: number): GameEntity | undefined {
    return this.games.get(id);
  }

  createGame(
    data: Omit<GameEntity, "Id" | "CreatedAt" | "UpdatedAt">
  ): GameEntity {
    const game: GameEntity = {
      ...data,
      Id: this.nextGameId++,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.games.set(game.Id, game);
    return game;
  }

  updateGame(id: number, data: Partial<GameEntity>): GameEntity | undefined {
    const game = this.games.get(id);
    if (!game) return undefined;

    const updated = { ...game, ...data, UpdatedAt: new Date().toISOString() };
    this.games.set(id, updated);
    return updated;
  }

  // Player methods
  getPlayer(id: number): PlayerEntity | undefined {
    return this.players.get(id);
  }

  getPlayerByEmail(email: string): PlayerEntity | undefined {
    return Array.from(this.players.values()).find(
      (p) => p.Email === email
    );
  }

  getPlayersByGameId(gameId: number): PlayerEntity[] {
    return Array.from(this.players.values()).filter((p) => p.GameId === gameId);
  }

  createPlayer(
    data: Omit<PlayerEntity, "Id" | "CreatedAt" | "UpdatedAt">
  ): PlayerEntity {
    const player: PlayerEntity = {
      ...data,
      Id: this.nextPlayerId++,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.players.set(player.Id, player);
    return player;
  }

  updatePlayer(
    id: number,
    data: Partial<PlayerEntity>
  ): PlayerEntity | undefined {
    const player = this.players.get(id);
    if (!player) return undefined;

    const updated = { ...player, ...data, UpdatedAt: new Date().toISOString() };
    this.players.set(id, updated);
    return updated;
  }

  // Rule methods
  getRule(id: number): RuleEntity | undefined {
    return this.rules.get(id);
  }

  getRulesByGameId(gameId: number): RuleEntity[] {
    return Array.from(this.rules.values()).filter((r) => r.GameId === gameId);
  }

  createRule(
    data: Omit<RuleEntity, "Id" | "CreatedAt" | "UpdatedAt">
  ): RuleEntity {
    const rule: RuleEntity = {
      ...data,
      Id: this.nextRuleId++,
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.rules.set(rule.Id, rule);
    return rule;
  }

  updateRule(id: number, data: Partial<RuleEntity>): RuleEntity | undefined {
    const rule = this.rules.get(id);
    if (!rule) return undefined;

    const updated = { ...rule, ...data, UpdatedAt: new Date().toISOString() };
    this.rules.set(id, updated);
    return updated;
  }

  deleteRule(id: number): boolean {
    return this.rules.delete(id);
  }

  // Assignment methods
  getAssignment(id: number): RuleAssignmentEntity | undefined {
    return this.assignments.get(id);
  }

  getAssignmentByRuleId(ruleId: number): RuleAssignmentEntity | undefined {
    return Array.from(this.assignments.values()).find(
      (a) => a.RuleId === ruleId
    );
  }

  getAssignmentsByGameId(gameId: number): RuleAssignmentEntity[] {
    return Array.from(this.assignments.values()).filter(
      (a) => a.GameId === gameId
    );
  }

  createAssignment(
    data: Omit<
      RuleAssignmentEntity,
      "Id" | "AssignedAt" | "CreatedAt" | "UpdatedAt"
    >
  ): RuleAssignmentEntity {
    const assignment: RuleAssignmentEntity = {
      ...data,
      Id: this.nextAssignmentId++,
      AssignedAt: new Date().toISOString(),
      CreatedAt: new Date().toISOString(),
      UpdatedAt: new Date().toISOString(),
    };
    this.assignments.set(assignment.Id, assignment);
    return assignment;
  }

  reset() {
    this.games.clear();
    this.players.clear();
    this.rules.clear();
    this.assignments.clear();
    this.nextGameId = 1;
    this.nextPlayerId = 1;
    this.nextRuleId = 1;
    this.nextAssignmentId = 1;
    this.seedData();
  }
}

export const dataStore = new DataStore();
