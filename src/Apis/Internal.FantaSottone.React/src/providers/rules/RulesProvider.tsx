import React, { createContext, useContext, useMemo } from "react";
import { ITransport } from "@/lib/http/Transport";
import { createTransport } from "@/lib/http/transportFactory";
import { useAuth } from "../auth/AuthProvider";

// DTOs
export interface RuleDto {
  id: number;
  name: string;
  ruleType: number;
  scoreDelta: number;
}

export interface RuleAssignmentInfoDto {
  assignedToPlayerId: number;
  assignedToUsername: string;
  assignedAt: string;
}

export interface RuleWithAssignmentDto {
  rule: RuleDto;
  assignment: RuleAssignmentInfoDto | null;
}

export interface AssignmentDto {
  id: number;
  ruleId: number;
  assignedToPlayerId: number;
  scoreDeltaApplied: number;
  assignedAt: string;
}

export interface AssignRuleResponse {
  assignment: AssignmentDto;
}

export interface CreateRuleRequest {
  name: string;
  ruleType: number;
  scoreDelta: number;
}

export interface CreateRuleResponse {
  rule: RuleDto;
}

export interface UpdateRuleRequest {
  name: string;
  ruleType: number;
  scoreDelta: number;
}

export interface UpdateRuleResponse {
  rule: RuleDto;
}

interface RulesContextValue {
  getRules: (gameId: number) => Promise<RuleWithAssignmentDto[]>;
  assignRule: (gameId: number, ruleId: number) => Promise<AssignRuleResponse>;
  createRule: (gameId: number, request: CreateRuleRequest) => Promise<CreateRuleResponse>;
  updateRule: (gameId: number, ruleId: number, request: UpdateRuleRequest) => Promise<UpdateRuleResponse>;
  deleteRule: (gameId: number, ruleId: number) => Promise<void>;
}

const RulesContext = createContext<RulesContextValue | undefined>(undefined);

export function RulesProvider({ children }: { children: React.ReactNode }) {
  const { session } = useAuth();
  const transport: ITransport = useMemo(
    () => createTransport(() => session?.token ?? null),
    [session]
  );

  const getRules = async (gameId: number): Promise<RuleWithAssignmentDto[]> => {
    return transport.get<RuleWithAssignmentDto[]>(`/api/Games/${gameId}/rules`);
  };

  const assignRule = async (
    gameId: number,
    ruleId: number
  ): Promise<AssignRuleResponse> => {
    // Non serve inviare playerId nel body - viene estratto dal token JWT
    return transport.post<{}, AssignRuleResponse>(
      `/api/Games/${gameId}/rules/${ruleId}/assign`,
      {}
    );
  };

  const createRule = async (
    gameId: number,
    request: CreateRuleRequest
  ): Promise<CreateRuleResponse> => {
    return transport.post<CreateRuleRequest, CreateRuleResponse>(
      `/api/Games/${gameId}/rules`,
      request
    );
  };

  const updateRule = async (
    gameId: number,
    ruleId: number,
    request: UpdateRuleRequest
  ): Promise<UpdateRuleResponse> => {
    return transport.put<UpdateRuleRequest, UpdateRuleResponse>(
      `/api/Games/${gameId}/rules/${ruleId}`,
      request
    );
  };

  const deleteRule = async (gameId: number, ruleId: number): Promise<void> => {
    await transport.delete<void>(`/api/Games/${gameId}/rules/${ruleId}`);
  };

  const value: RulesContextValue = {
    getRules,
    assignRule,
    createRule,
    updateRule,
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
