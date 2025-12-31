import {
  AssignmentHistoryEntry,
  AssignRuleResponse,
  GameStatusResponse,
  LeaderboardEntry,
  RuleWithAssignment,
  StartGameResponse,
  EndGameResponse,
  UpdateRuleRequest,
  UpdateRuleResponse,
  CreateRuleRequest,
  CreateRuleResponse,
} from "@/types/dto";
import { dataStore } from "./dataStore";
import { ConflictError, NotFoundError } from "@/lib/http/errors";
import { GameStatus } from "@/types/entities";

// Mock API handlers that simulate backend responses

export const handlers = {
  // ❌ RIMOSSO: Auth handlers tradizionali (login con username/accessCode)
  // Ora l'autenticazione funziona solo tramite Google OAuth

  // Game handlers
  // ⚠️ DEPRECATO: startGame non è più usato con il nuovo sistema
  // I giochi vengono creati tramite createGame con inviti via email
  startGame: async (): Promise<StartGameResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Questo metodo è deprecato ma mantenuto per compatibilità
    throw new Error(
      "StartGame is deprecated. Use CreateGame endpoint with email invites instead."
    );
  },

  getLeaderboard: async (gameId: number): Promise<LeaderboardEntry[]> => {
    await new Promise((resolve) => setTimeout(resolve, 200));

    const players = dataStore.getPlayersByGameId(gameId);
    if (players.length === 0) {
      throw new NotFoundError("Game not found");
    }

    return players
      .map((p) => ({
        id: p.Id,
        email: p.Email || `player${p.Id}@example.com`, // ✅ CAMBIATO: username -> email
        currentScore: p.CurrentScore,
        isCreator: p.IsCreator,
      }))
      .sort((a, b) => b.currentScore - a.currentScore);
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
          assignedToEmail:
            assignedPlayer?.Email ||
            `player${assignment.AssignedToPlayerId}@example.com`, // ✅ CAMBIATO
          assignedAt: assignment.AssignedAt,
        };
      }

      return {
        rule: {
          id: rule.Id,
          name: rule.Name,
          ruleType: rule.RuleType,
          scoreDelta: rule.ScoreDelta,
        },
        assignment: assignmentData,
      };
    });
  },

  createRule: async (
    gameId: number,
    request: CreateRuleRequest
  ): Promise<CreateRuleResponse> => {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const game = dataStore.getGame(gameId);
    if (!game) {
      throw new NotFoundError("Game not found");
    }

    const rule = dataStore.createRule({
      GameId: gameId,
      Name: request.name,
      RuleType: request.ruleType,
      ScoreDelta: request.scoreDelta,
    });

    return {
      rule: {
        id: rule.Id,
        name: rule.Name,
        ruleType: rule.RuleType,
        scoreDelta: rule.ScoreDelta,
      },
    };
  },

  deleteRule: async (gameId: number, ruleId: number): Promise<void> => {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const rule = dataStore.getRule(ruleId);
    if (!rule) {
      throw new NotFoundError("Rule not found");
    }

    if (rule.GameId !== gameId) {
      throw new Error("Rule does not belong to this game");
    }

    // Check if assigned
    const assignment = dataStore.getAssignmentByRuleId(ruleId);
    if (assignment) {
      throw new ConflictError(
        "Cannot delete a rule that has already been assigned"
      );
    }

    dataStore.deleteRule(ruleId);
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
        id: updatedPlayer.Id,
        currentScore: updatedPlayer.CurrentScore,
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
          id: winnerPlayer.Id,
          email: winnerPlayer.Email || `player${winnerPlayer.Id}@example.com`, // ✅ CAMBIATO
          currentScore: winnerPlayer.CurrentScore,
        };
      }
    }

    return {
      game: {
        id: game.Id,
        status: game.Status,
        winnerPlayerId: game.WinnerPlayerId ?? undefined,
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
        assignedToEmail:
          player?.Email || `player${a.AssignedToPlayerId}@example.com`, // ✅ CAMBIATO
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
        id: game.Id,
        status: GameStatus.Ended,
        winnerPlayerId: winner.Id,
      },
      winner: {
        id: winner.Id,
        email: winner.Email || `player${winner.Id}@example.com`, // ✅ CAMBIATO
        currentScore: winner.CurrentScore,
      },
      leaderboard: sortedPlayers.map((p) => ({
        id: p.Id,
        email: p.Email || `player${p.Id}@example.com`, // ✅ CAMBIATO
        currentScore: p.CurrentScore,
        isCreator: p.IsCreator,
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
        id: updatedRule.Id,
        name: updatedRule.Name,
        ruleType: updatedRule.RuleType,
        scoreDelta: updatedRule.ScoreDelta,
      },
    };
  },
};
