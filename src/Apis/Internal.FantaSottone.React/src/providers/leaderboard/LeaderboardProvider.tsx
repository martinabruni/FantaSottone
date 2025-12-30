import React, { createContext, useContext, useMemo } from "react";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

// DTOs
export interface LeaderboardPlayerDto {
  id: number;
  username: string;
  currentScore: number;
  isCreator: boolean;
}

interface LeaderboardContextValue {
  getLeaderboard: (gameId: number) => Promise<LeaderboardPlayerDto[]>;
}

const LeaderboardContext = createContext<LeaderboardContextValue | undefined>(
  undefined
);

export function LeaderboardProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const getLeaderboard = async (
    gameId: number
  ): Promise<LeaderboardPlayerDto[]> => {
    return transport.get<LeaderboardPlayerDto[]>(
      `/api/Games/${gameId}/leaderboard`
    );
  };

  const value: LeaderboardContextValue = {
    getLeaderboard,
  };

  return (
    <LeaderboardContext.Provider value={value}>
      {children}
    </LeaderboardContext.Provider>
  );
}

export function useLeaderboard() {
  const context = useContext(LeaderboardContext);
  if (!context) {
    throw new Error("useLeaderboard must be used within LeaderboardProvider");
  }
  return context;
}
