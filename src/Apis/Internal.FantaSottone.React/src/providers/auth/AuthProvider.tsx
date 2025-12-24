import React, { createContext, useContext, useEffect, useState } from "react";
import { LoginRequest, LoginResponse } from "@/types/dto";
import { IAuthStrategy, SessionData } from "@/lib/auth/AuthStrategy";
import { MockAuthStrategy } from "@/lib/auth/MockAuthStrategy";
import { JwtAuthStrategy } from "@/lib/auth/JwtAuthStrategy";
import { HttpClient } from "@/lib/http/HttpClient";
import { MockTransport } from "@/mocks/MockTransport";
import { Role, getPermissions, UserPermissions } from "@/lib/auth/roles";

interface AuthContextValue {
  session: SessionData | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: Error | null;
  permissions: UserPermissions;
  login: (credentials: LoginRequest) => Promise<LoginResponse | undefined>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function createAuthStrategy(): IAuthStrategy {
  const authStrategy = import.meta.env.VITE_AUTH_STRATEGY || "mock";
  const useMocks = import.meta.env.VITE_USE_MOCKS === "true";
  const baseUrl = import.meta.env.VITE_API_BASE_URL || "http://localhost:5001";

  if (authStrategy === "mock") {
    return new MockAuthStrategy();
  }

  // JWT strategy
  const transport = useMocks
    ? new MockTransport()
    : new HttpClient({ baseUrl });
  return new JwtAuthStrategy(transport);
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [strategy] = useState<IAuthStrategy>(() => createAuthStrategy());
  const [session, setSession] = useState<SessionData | null>(() =>
    strategy.getSession()
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const permissions = session
    ? getPermissions(session.role)
    : getPermissions("player" as Role);

  useEffect(() => {
    // Initialize session on mount
    setSession(strategy.getSession());
  }, [strategy]);

  const login = async (
    credentials: LoginRequest
  ): Promise<LoginResponse | undefined> => {
    setLoading(true);
    setError(null);

    try {
      const response = await strategy.login(credentials);
      strategy.saveSession(response);
      setSession(strategy.getSession());
      return response;
    } catch (err) {
      const error = err as Error;
      setError(error);
      return undefined;
    } finally {
      setLoading(false);
    }
  };

  const logout = async (): Promise<void> => {
    await strategy.logout();
    setSession(null);
    setError(null);
  };

  const value: AuthContextValue = {
    session,
    isAuthenticated: session !== null,
    loading,
    error,
    permissions,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}
