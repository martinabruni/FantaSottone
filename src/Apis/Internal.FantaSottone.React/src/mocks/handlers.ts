import {
  AssignmentHistoryEntry,
  AssignRuleResponse,
  GameStatusResponse,
  LeaderboardEntry,
  LoginRequest,
  LoginResponse,
  RuleWithAssignment,
  StartGameRequest,
  StartGameResponse,
  EndGameResponse,
  UpdateRuleRequest,
  UpdateRuleResponse,
} from "@/types/dto";
import { dataStore } from "./dataStore";
import {
  ConflictError,
  NotFoundError,
  UnauthorizedError,
} from "@/lib/http/errors";
import { GameStatus } from "@/types/entities";

// Mock API handlers that simulate backend responses

export const handlers = {
  // Auth handlers
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const player = dataStore.getPlayerByUsername(credentials.username);
    if (!player || player.AccessCode !== credentials.accessCode) {
      throw new UnauthorizedError("Invalid credentials");
    }

    const game = dataStore.getGame(player.GameId);
    if (!game) {
      throw new NotFoundError("Game not found");
    }

    return {
      token: `mock-token-${player.Id}-${Date.now()}`,
      game: {
        Id: game.Id,
        Name: game.Name,
        InitialScore: game.InitialScore,
        Status: game.Status,
        CreatorPlayerId: game.CreatorPlayerId,
        WinnerPlayerId: game.WinnerPlayerId,
      },
      player: {
        Id: player.Id,
        GameId: player.GameId,
        Username: player.Username,
        IsCreator: player.IsCreator,
        CurrentScore: player.CurrentScore,
      },
    };
  },

  // Game handlers
  startGame: async (request: StartGameRequest): Promise<StartGameResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Create game
    const game = dataStore.createGame({
      Name: request.name,
      InitialScore: request.initialScore,
      Status: GameStatus.Started,
      CreatorPlayerId: undefined,
      WinnerPlayerId: null,
    });

    // Create players
    const credentials: StartGameResponse["credentials"] = [];
    request.players.forEach((p, index) => {
      const player = dataStore.createPlayer({
        GameId: game.Id,
        Username: p.username,
        AccessCode: p.accessCode,
        IsCreator: p.isCreator ?? index === 0,
        CurrentScore: request.initialScore,
      });

      if (player.IsCreator && !game.CreatorPlayerId) {
        dataStore.updateGame(game.Id, { CreatorPlayerId: player.Id });
      }

      credentials.push({
        username: player.Username,
        accessCode: player.AccessCode,
        isCreator: player.IsCreator,
      });
    });

    // Create rules
    request.rules.forEach((r) => {
      dataStore.createRule({
        GameId: game.Id,
        Name: r.name,
        RuleType: r.ruleType,
        ScoreDelta: r.scoreDelta,
      });
    });

    return {
      gameId: game.Id,
      credentials,
    };
  },

  getLeaderboard: async (gameId: number): Promise<LeaderboardEntry[]> => {
    await new Promise((resolve) => setTimeout(resolve, 200));

    const players = dataStore.getPlayersByGameId(gameId);
    if (players.length === 0) {
      throw new NotFoundError("Game not found");
    }

    return players
      .map((p) => ({
        Id: p.Id,
        Username: p.Username,
        CurrentScore: p.CurrentScore,
        IsCreator: p.IsCreator,
      }))
      .sort((a, b) => b.CurrentScore - a.CurrentScore);
  },

  getRules: async (gameId: number): Promise<RuleWithAssignment[]> => {
    await new Promise((resolve) => setTimeout(resolve, 200));

    const rules = dataStore.getRulesByGameId(gameId);
    if (rules.length === 0) {
      const game = dataStore.getGame(gameId);
      if (!game) {
        throw new NotFoundError("Game not found");
      }
      return [];
    }

    return rules.map((rule) => {
      const assignment = dataStore.getAssignmentByRuleId(rule.Id);
      let assignmentData = null;

      if (assignment) {
        const assignedPlayer = dataStore.getPlayer(
          assignment.AssignedToPlayerId
        );
        assignmentData = {
          ruleAssignmentId: assignment.Id,
          assignedToPlayerId: assignment.AssignedToPlayerId,
          assignedToUsername: assignedPlayer?.Username ?? "Unknown",
          assignedAt: assignment.AssignedAt,
        };
      }

      return {
        rule: {
          Id: rule.Id,
          Name: rule.Name,
          RuleType: rule.RuleType,
          ScoreDelta: rule.ScoreDelta,
        },
        assignment: assignmentData,
      };
    });
  },

  assignRule: async (
    gameId: number,
    ruleId: number,
    playerId: number
  ): Promise<AssignRuleResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const rule = dataStore.getRule(ruleId);
    if (!rule || rule.GameId !== gameId) {
      throw new NotFoundError("Rule not found");
    }

    // Check if already assigned ("La prima che")
    const existingAssignment = dataStore.getAssignmentByRuleId(ruleId);
    if (existingAssignment) {
      throw new ConflictError("Rule already assigned");
    }

    const player = dataStore.getPlayer(playerId);
    if (!player || player.GameId !== gameId) {
      throw new NotFoundError("Player not found");
    }

    // Create assignment
    const assignment = dataStore.createAssignment({
      RuleId: ruleId,
      GameId: gameId,
      AssignedToPlayerId: playerId,
      ScoreDeltaApplied: rule.ScoreDelta,
    });

    // Update player score
    const newScore = player.CurrentScore + rule.ScoreDelta;
    const updatedPlayer = dataStore.updatePlayer(playerId, {
      CurrentScore: newScore,
    });

    if (!updatedPlayer) {
      throw new Error("Failed to update player");
    }

    // Check if all rules are assigned (game end condition)
    const allRules = dataStore.getRulesByGameId(gameId);
    const allAssignments = dataStore.getAssignmentsByGameId(gameId);
    const gameStatus =
      allRules.length > 0 && allRules.length === allAssignments.length
        ? GameStatus.Ended
        : GameStatus.Started;

    let winnerId: number | null = null;
    if (gameStatus === GameStatus.Ended) {
      const leaderboard = dataStore.getPlayersByGameId(gameId);
      const winner = leaderboard.sort(
        (a, b) => b.CurrentScore - a.CurrentScore
      )[0];
      winnerId = winner?.Id ?? null;
      dataStore.updateGame(gameId, {
        Status: gameStatus,
        WinnerPlayerId: winnerId,
      });
    }

    return {
      assignment: {
        id: assignment.Id,
        ruleId: assignment.RuleId,
        assignedToPlayerId: assignment.AssignedToPlayerId,
        assignedAt: assignment.AssignedAt,
        scoreDeltaApplied: assignment.ScoreDeltaApplied,
      },
      updatedPlayer: {
        Id: updatedPlayer.Id,
        CurrentScore: updatedPlayer.CurrentScore,
      },
      gameStatus: {
        status: gameStatus,
        winnerPlayerId: winnerId,
      },
    };
  },

  getGameStatus: async (gameId: number): Promise<GameStatusResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 200));

    const game = dataStore.getGame(gameId);
    if (!game) {
      throw new NotFoundError("Game not found");
    }

    let winner = null;
    if (game.WinnerPlayerId) {
      const winnerPlayer = dataStore.getPlayer(game.WinnerPlayerId);
      if (winnerPlayer) {
        winner = {
          Id: winnerPlayer.Id,
          Username: winnerPlayer.Username,
          CurrentScore: winnerPlayer.CurrentScore,
        };
      }
    }

    return {
      game: {
        Id: game.Id,
        Status: game.Status,
        WinnerPlayerId: game.WinnerPlayerId ?? undefined,
      },
      winner,
    };
  },

  getAssignments: async (gameId: number): Promise<AssignmentHistoryEntry[]> => {
    await new Promise((resolve) => setTimeout(resolve, 200));

    const assignments = dataStore.getAssignmentsByGameId(gameId);

    return assignments.map((a) => {
      const rule = dataStore.getRule(a.RuleId);
      const player = dataStore.getPlayer(a.AssignedToPlayerId);

      return {
        id: a.Id,
        ruleId: a.RuleId,
        ruleName: rule?.Name ?? "Unknown",
        assignedToPlayerId: a.AssignedToPlayerId,
        assignedToUsername: player?.Username ?? "Unknown",
        scoreDeltaApplied: a.ScoreDeltaApplied,
        assignedAt: a.AssignedAt,
      };
    });
  },

  endGame: async (gameId: number): Promise<EndGameResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const game = dataStore.getGame(gameId);
    if (!game) {
      throw new NotFoundError("Game not found");
    }

    // Get all players and determine winner
    const players = dataStore.getPlayersByGameId(gameId);
    const sortedPlayers = players.sort((a, b) => {
      if (b.CurrentScore === a.CurrentScore) {
        // Tie-breaker: lowest ID wins
        return a.Id - b.Id;
      }
      return b.CurrentScore - a.CurrentScore;
    });

    const winner = sortedPlayers[0];
    if (!winner) {
      throw new Error("No players found in game");
    }

    // Update game status
    dataStore.updateGame(gameId, {
      Status: GameStatus.Ended,
      WinnerPlayerId: winner.Id,
    });

    return {
      game: {
        Id: game.Id,
        Status: GameStatus.Ended,
        WinnerPlayerId: winner.Id,
      },
      winner: {
        Id: winner.Id,
        Username: winner.Username,
        CurrentScore: winner.CurrentScore,
      },
      leaderboard: sortedPlayers.map((p) => ({
        Id: p.Id,
        Username: p.Username,
        CurrentScore: p.CurrentScore,
        IsCreator: p.IsCreator,
      })),
    };
  },

  updateRule: async (
    gameId: number,
    ruleId: number,
    request: UpdateRuleRequest
  ): Promise<UpdateRuleResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const rule = dataStore.getRule(ruleId);
    if (!rule || rule.GameId !== gameId) {
      throw new NotFoundError("Rule not found");
    }

    // Check if rule is already assigned
    const assignment = dataStore.getAssignmentByRuleId(ruleId);
    if (assignment) {
      throw new ConflictError(
        "Cannot modify rule that has already been assigned"
      );
    }

    // Update the rule
    const updatedRule = dataStore.updateRule(ruleId, {
      Name: request.name,
      RuleType: request.ruleType,
      ScoreDelta: request.scoreDelta,
    });

    if (!updatedRule) {
      throw new Error("Failed to update rule");
    }

    return {
      rule: {
        Id: updatedRule.Id,
        Name: updatedRule.Name,
        RuleType: updatedRule.RuleType,
        ScoreDelta: updatedRule.ScoreDelta,
      },
    };
  },
};
