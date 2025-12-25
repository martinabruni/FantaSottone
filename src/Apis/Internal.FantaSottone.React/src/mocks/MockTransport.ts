import { ITransport } from "@/lib/http/Transport";
import { handlers } from "./handlers";

// MockTransport implements the same interface as HttpClient but uses in-memory handlers

export class MockTransport implements ITransport {
  async get<T>(url: string): Promise<T> {
    // Parse URL and route to appropriate handler
    const parts = url.split("/").filter(Boolean);

    if (parts[0] === "api") {
      if (parts[1] === "games" && parts[2]) {
        const gameId = parseInt(parts[2]);

        if (parts[3] === "leaderboard") {
          return handlers.getLeaderboard(gameId) as Promise<T>;
        }

        if (parts[3] === "rules") {
          return handlers.getRules(gameId) as Promise<T>;
        }

        if (parts[3] === "status") {
          return handlers.getGameStatus(gameId) as Promise<T>;
        }

        if (parts[3] === "assignments") {
          return handlers.getAssignments(gameId) as Promise<T>;
        }
      }
    }

    throw new Error(`Mock GET not implemented for: ${url}`);
  }

  async post<TReq, TRes>(url: string, data: TReq): Promise<TRes> {
    const parts = url.split("/").filter(Boolean);

    if (parts[0] === "api") {
      if (parts[1] === "auth" && parts[2] === "login") {
        return handlers.login(data as never) as Promise<TRes>;
      }

      if (parts[1] === "games") {
        if (parts[2] === "start") {
          return handlers.startGame(data as never) as Promise<TRes>;
        }

        // POST /api/games/{gameId}/end
        if (parts[2] && parts[3] === "end") {
          const gameId = parseInt(parts[2]);
          return handlers.endGame(gameId) as Promise<TRes>;
        }

        if (
          parts[2] &&
          parts[3] === "rules" &&
          parts[4] &&
          parts[5] === "assign"
        ) {
          const gameId = parseInt(parts[2]);
          const ruleId = parseInt(parts[4]);
          const { playerId } = data as { playerId: number };
          return handlers.assignRule(gameId, ruleId, playerId) as Promise<TRes>;
        }
      }
    }

    throw new Error(`Mock POST not implemented for: ${url}`);
  }

  async put<TReq, TRes>(url: string, data: TReq): Promise<TRes> {
    const parts = url.split("/").filter(Boolean);

    if (parts[0] === "api") {
      if (
        parts[1] === "games" &&
        parts[2] &&
        parts[3] === "rules" &&
        parts[4]
      ) {
        const gameId = parseInt(parts[2]);
        const ruleId = parseInt(parts[4]);
        return handlers.updateRule(
          gameId,
          ruleId,
          data as never
        ) as Promise<TRes>;
      }
    }

    throw new Error(`Mock PUT not implemented for: ${url}`);
  }

  async delete<T>(url: string): Promise<T> {
    throw new Error(`Mock DELETE not implemented for: ${url}`);
  }
}
