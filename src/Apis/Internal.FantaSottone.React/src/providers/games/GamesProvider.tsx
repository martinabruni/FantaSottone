import React, { createContext, useContext, useMemo } from "react";
import {
  StartGameRequest,
  StartGameResponse,
  EndGameResponse,
} from "@/types/dto";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

interface GamesContextValue {
  startGame: (request: StartGameRequest) => Promise<StartGameResponse>;
  endGame: (gameId: number) => Promise<EndGameResponse>;
}

const GamesContext = createContext<GamesContextValue | undefined>(undefined);

export function GamesProvider({ children }: { children: React.ReactNode }) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const startGame = async (
    request: StartGameRequest
  ): Promise<StartGameResponse> => {
    return transport.post<StartGameRequest, StartGameResponse>(
      "/api/games/start",
      request
    );
  };

  const endGame = async (gameId: number): Promise<EndGameResponse> => {
    return transport.post<{}, EndGameResponse>(`/api/games/${gameId}/end`, {});
  };

  const value: GamesContextValue = {
    startGame,
    endGame,
  };

  return (
    <GamesContext.Provider value={value}>{children}</GamesContext.Provider>
  );
}

export function useGames() {
  const context = useContext(GamesContext);
  if (!context) {
    throw new Error("useGames must be used within GamesProvider");
  }
  return context;
}
