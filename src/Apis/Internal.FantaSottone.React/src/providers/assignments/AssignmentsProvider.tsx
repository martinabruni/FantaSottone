import React, { createContext, useContext, useMemo } from "react";
import { AssignmentHistoryEntry, GameStatusResponse } from "@/types/dto";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

interface AssignmentsContextValue {
  getAssignments: (gameId: number) => Promise<AssignmentHistoryEntry[]>;
  getGameStatus: (gameId: number) => Promise<GameStatusResponse>;
}

const AssignmentsContext = createContext<AssignmentsContextValue | undefined>(
  undefined
);

export function AssignmentsProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const getAssignments = async (
    gameId: number
  ): Promise<AssignmentHistoryEntry[]> => {
    return transport.get<AssignmentHistoryEntry[]>(
      `/api/games/${gameId}/assignments`
    );
  };

  const getGameStatus = async (gameId: number): Promise<GameStatusResponse> => {
    return transport.get<GameStatusResponse>(`/api/games/${gameId}/status`);
  };

  const value: AssignmentsContextValue = {
    getAssignments,
    getGameStatus,
  };

  return (
    <AssignmentsContext.Provider value={value}>
      {children}
    </AssignmentsContext.Provider>
  );
}

export function useAssignments() {
  const context = useContext(AssignmentsContext);
  if (!context) {
    throw new Error("useAssignments must be used within AssignmentsProvider");
  }
  return context;
}
