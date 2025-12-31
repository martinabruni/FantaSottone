import React, { createContext, useContext, useMemo } from "react";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";
import {
  GetUserGamesResponse,
  GetUserProfileResponse,
} from "@/types/user-types";

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
