import React, { createContext, useContext, useEffect, useState } from "react";
import { LoginRequest, LoginResponse } from "@/types/dto";
import { IAuthStrategy, SessionData } from "@/lib/auth/AuthStrategy";
import { MockAuthStrategy } from "@/lib/auth/MockAuthStrategy";
import { JwtAuthStrategy } from "@/lib/auth/JwtAuthStrategy";
import { GoogleAuthStrategy, GoogleAuthResponse } from "@/lib/auth/GoogleAuthStrategy";
import { createTransport } from "@/lib/http/transportFactory";
import { Role, getPermissions, UserPermissions } from "@/lib/auth/roles";

interface AuthContextValue {
  session: SessionData | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: Error | null;
  permissions: UserPermissions;
  login: (credentials: LoginRequest) => Promise<LoginResponse | undefined>;
  loginWithGoogle: (idToken: string) => Promise<GoogleAuthResponse>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function createAuthStrategy(getToken?: () => string | null): IAuthStrategy {
  const authStrategy = import.meta.env.VITE_AUTH_STRATEGY || "mock";

  if (authStrategy === "mock") {
    return new MockAuthStrategy();
  }

  // JWT strategy - uses transportFactory
  const transport = createTransport(getToken);
  return new JwtAuthStrategy(transport);
}

function createGoogleAuthStrategy(getToken?: () => string | null): GoogleAuthStrategy {
  // Google auth always uses a transport (not mock strategy)
  const transport = createTransport(getToken);
  return new GoogleAuthStrategy(transport);
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<SessionData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  // Create getToken function that returns current token
  const getToken = () => session?.token ?? null;

  const [strategy] = useState<IAuthStrategy>(() => createAuthStrategy(getToken));
  const [googleStrategy] = useState<GoogleAuthStrategy>(() =>
    createGoogleAuthStrategy(getToken)
  );

  const permissions = session
    ? getPermissions(session.role)
    : getPermissions("player" as Role);

  useEffect(() => {
    // Initialize session on mount - check both strategies
    const jwtSession = strategy.getSession();
    const googleSession = googleStrategy.getSession();
    setSession(jwtSession || googleSession);
  }, [strategy, googleStrategy]);

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

  const loginWithGoogle = async (
    idToken: string
  ): Promise<GoogleAuthResponse> => {
    setLoading(true);
    setError(null);

    try {
      const response = await googleStrategy.loginWithGoogle(idToken);
      googleStrategy.saveGoogleSession(response, response.email);
      setSession(googleStrategy.getSession());
      return response;
    } catch (err) {
      const error = err as Error;
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const logout = async (): Promise<void> => {
    await strategy.logout();
    await googleStrategy.logout();
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
    loginWithGoogle,
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
