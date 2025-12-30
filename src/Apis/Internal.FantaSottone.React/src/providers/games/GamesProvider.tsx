import React, { createContext, useContext, useMemo } from "react";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

// DTOs
export interface CreateGameRequest {
  name: string;
  initialScore: number;
  invitedEmails: string[];
}

export interface GameDto {
  id: number;
  name: string;
  initialScore: number;
  status: number;
  creatorPlayerId: number;
}

export interface PlayerDto {
  id: number;
  email: string; // ✅ CAMBIATO: username -> email
  isCreator: boolean;
}

export interface CreateGameResponse {
  game: GameDto;
  creatorPlayer: PlayerDto;
  invitedEmails: string[];
  invalidEmails: string[];
}

export interface InvitePlayerRequest {
  email: string;
}

export interface GameStatusResponse {
  status: number;
}

export interface EndGameInfoDto {
  id: number;
  status: number;
  winnerPlayerId: number;
}

export interface WinnerDto {
  id: number;
  email: string; // ✅ CAMBIATO: username -> email
  currentScore: number;
}

export interface LeaderboardPlayerDto {
  id: number;
  email: string; // ✅ CAMBIATO: username -> email
  currentScore: number;
  isCreator: boolean;
}

export interface EndGameResponse {
  game: EndGameInfoDto;
  winner: WinnerDto;
  leaderboard: LeaderboardPlayerDto[];
}

interface GamesContextValue {
  createGame: (request: CreateGameRequest) => Promise<CreateGameResponse>;
  invitePlayer: (gameId: number, request: InvitePlayerRequest) => Promise<{ message: string; playerId: number }>;
  getGameStatus: (gameId: number) => Promise<GameStatusResponse>;
  endGame: (gameId: number) => Promise<EndGameResponse>;
}

const GamesContext = createContext<GamesContextValue | undefined>(undefined);

export function GamesProvider({ children }: { children: React.ReactNode }) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const createGame = async (
    request: CreateGameRequest
  ): Promise<CreateGameResponse> => {
    return transport.post<CreateGameRequest, CreateGameResponse>(
      "/api/Games/create",
      request
    );
  };

  const invitePlayer = async (
    gameId: number,
    request: InvitePlayerRequest
  ): Promise<{ message: string; playerId: number }> => {
    return transport.post<InvitePlayerRequest, { message: string; playerId: number }>(
      `/api/Games/${gameId}/invite`,
      request
    );
  };

  const getGameStatus = async (gameId: number): Promise<GameStatusResponse> => {
    return transport.get<GameStatusResponse>(`/api/Games/${gameId}/status`);
  };

  const endGame = async (gameId: number): Promise<EndGameResponse> => {
    return transport.post<{}, EndGameResponse>(`/api/Games/${gameId}/end`, {});
  };

  const value: GamesContextValue = {
    createGame,
    invitePlayer,
    getGameStatus,
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
