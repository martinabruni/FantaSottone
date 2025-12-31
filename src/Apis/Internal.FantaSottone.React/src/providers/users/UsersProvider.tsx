import React, { createContext, useContext, useMemo } from "react";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

// DTOs
export interface UserProfileDto {
  userId: number;
  email: string;
  profileImageUrl: string | null;
  createdAt: string;
}

export interface GetUserProfileResponse {
  profile: UserProfileDto;
}

export interface GameInvitationDto {
  gameId: number;
  gameName: string;
  initialScore: number;
  status: number;
  playerCount: number;
  createdAt: string;
}

export interface GetUserGamesResponse {
  games: GameInvitationDto[];
}

interface UsersContextValue {
  getUserProfile: () => Promise<GetUserProfileResponse>;
  getUserGames: () => Promise<GetUserGamesResponse>;
}

const UsersContext = createContext<UsersContextValue | undefined>(undefined);

export function UsersProvider({ children }: { children: React.ReactNode }) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const getUserProfile = async (): Promise<GetUserProfileResponse> => {
    return transport.get<GetUserProfileResponse>("/api/Users/profile");
  };

  const getUserGames = async (): Promise<GetUserGamesResponse> => {
    return transport.get<GetUserGamesResponse>("/api/Users/games");
  };

  const value: UsersContextValue = {
    getUserProfile,
    getUserGames,
  };

  return (
    <UsersContext.Provider value={value}>{children}</UsersContext.Provider>
  );
}

export function useUsers() {
  const context = useContext(UsersContext);
  if (!context) {
    throw new Error("useUsers must be used within UsersProvider");
  }
  return context;
}
