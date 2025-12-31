import React, { createContext, useContext, useMemo } from "react";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

// DTOs
export interface AssignmentAuditDto {
  id: number;
  ruleId: number;
  ruleName: string;
  assignedToPlayerId: number;
  assignedToUsername: string;
  scoreDeltaApplied: number;
  assignedAt: string;
}

interface AssignmentsContextValue {
  getAssignments: (gameId: number) => Promise<AssignmentAuditDto[]>;
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
  ): Promise<AssignmentAuditDto[]> => {
    return transport.get<AssignmentAuditDto[]>(
      `/api/Games/${gameId}/assignments`
    );
  };

  const value: AssignmentsContextValue = {
    getAssignments,
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
