import React, { createContext, useContext, useMemo } from "react";
import {
  AssignRuleResponse,
  RuleWithAssignment,
  UpdateRuleRequest,
  UpdateRuleResponse,
  CreateRuleRequest,
  CreateRuleResponse,
} from "@/types/dto";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

interface RulesContextValue {
  getRules: (gameId: number) => Promise<RuleWithAssignment[]>;
  assignRule: (
    gameId: number,
    ruleId: number,
    playerId: number
  ) => Promise<AssignRuleResponse>;
  updateRule: (
    gameId: number,
    ruleId: number,
    request: UpdateRuleRequest
  ) => Promise<UpdateRuleResponse>;
  createRule: (
    gameId: number,
    request: CreateRuleRequest
  ) => Promise<CreateRuleResponse>;
  deleteRule: (gameId: number, ruleId: number) => Promise<void>;
}

const RulesContext = createContext<RulesContextValue | undefined>(undefined);

export function RulesProvider({ children }: { children: React.ReactNode }) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const getRules = async (gameId: number): Promise<RuleWithAssignment[]> => {
    return transport.get<RuleWithAssignment[]>(`/api/games/${gameId}/rules`);
  };

  const assignRule = async (
    gameId: number,
    ruleId: number,
    playerId: number
  ): Promise<AssignRuleResponse> => {
    return transport.post<{ playerId: number }, AssignRuleResponse>(
      `/api/games/${gameId}/rules/${ruleId}/assign`,
      { playerId }
    );
  };

  const updateRule = async (
    gameId: number,
    ruleId: number,
    request: UpdateRuleRequest
  ): Promise<UpdateRuleResponse> => {
    return transport.put<UpdateRuleRequest, UpdateRuleResponse>(
      `/api/games/${gameId}/rules/${ruleId}`,
      request
    );
  };

  const createRule = async (
    gameId: number,
    request: CreateRuleRequest
  ): Promise<CreateRuleResponse> => {
    return transport.post<CreateRuleRequest, CreateRuleResponse>(
      `/api/games/${gameId}/rules`,
      request
    );
  };

  const deleteRule = async (
    gameId: number,
    ruleId: number
  ): Promise<void> => {
    await transport.delete<void>(`/api/games/${gameId}/rules/${ruleId}`);
  };

  const value: RulesContextValue = {
    getRules,
    assignRule,
    updateRule,
    createRule,
    deleteRule,
  };

  return (
    <RulesContext.Provider value={value}>{children}</RulesContext.Provider>
  );
}

export function useRules() {
  const context = useContext(RulesContext);
  if (!context) {
    throw new Error("useRules must be used within RulesProvider");
  }
  return context;
}
