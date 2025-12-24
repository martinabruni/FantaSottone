// Role-based authorization

export type Role = "creator" | "player";

export interface UserPermissions {
  canStartGame: boolean;
  canAssignRules: boolean;
  canViewLeaderboard: boolean;
  canViewRules: boolean;
}

export function getRoleFromIsCreator(isCreator: boolean): Role {
  return isCreator ? "creator" : "player";
}

export function getPermissions(role: Role): UserPermissions {
  return {
    canStartGame: role === "creator",
    canAssignRules: true, // both can assign
    canViewLeaderboard: true, // both can view
    canViewRules: true, // both can view
  };
}
