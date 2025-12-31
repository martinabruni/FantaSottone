import React, { createContext, useContext, useEffect, useState } from "react";
import { IAuthStrategy, SessionData } from "@/lib/auth/AuthStrategy";
import { MockAuthStrategy } from "@/lib/auth/MockAuthStrategy";
import { JwtAuthStrategy } from "@/lib/auth/JwtAuthStrategy";
import {
  GoogleAuthStrategy,
  GoogleAuthResponse,
} from "@/lib/auth/GoogleAuthStrategy";
import {
  EmailAuthStrategy,
  EmailAuthResponse,
} from "@/lib/auth/EmailAuthStrategy";
import { createTransport } from "@/lib/http/transportFactory";
import { Role, getPermissions, UserPermissions } from "@/lib/auth/roles";

interface AuthContextValue {
  session: SessionData | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: Error | null;
  permissions: UserPermissions;
  loginWithGoogle: (idToken: string) => Promise<GoogleAuthResponse>;
  loginWithEmail: (
    email: string,
    password: string
  ) => Promise<EmailAuthResponse>;
  registerWithEmail: (
    email: string,
    password: string
  ) => Promise<EmailAuthResponse>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function createAuthStrategy(): IAuthStrategy {
  const authStrategy = import.meta.env.VITE_AUTH_STRATEGY || "mock";

  if (authStrategy === "mock") {
    return new MockAuthStrategy();
  }

  // JWT strategy - uses transportFactory
  return new JwtAuthStrategy();
}

function createGoogleAuthStrategy(
  getToken?: () => string | null
): GoogleAuthStrategy {
  // Google auth always uses a transport (not mock strategy)
  const transport = createTransport(getToken);
  return new GoogleAuthStrategy(transport);
}

function createEmailAuthStrategy(
  getToken?: () => string | null
): EmailAuthStrategy {
  // Email auth always uses a transport
  const transport = createTransport(getToken);
  return new EmailAuthStrategy(transport);
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<SessionData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  // Create getToken function that returns current token
  const getToken = () => session?.token ?? null;

  const [strategy] = useState<IAuthStrategy>(() => createAuthStrategy());
  const [googleStrategy] = useState<GoogleAuthStrategy>(() =>
    createGoogleAuthStrategy(getToken)
  );
  const [emailStrategy] = useState<EmailAuthStrategy>(() =>
    createEmailAuthStrategy(getToken)
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

  // ❌ RIMOSSO: metodo login tradizionale

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
    await emailStrategy.logout();
    setSession(null);
    setError(null);
  };

  const loginWithEmail = async (
    email: string,
    password: string
  ): Promise<EmailAuthResponse> => {
    setLoading(true);
    setError(null);

    try {
      const response = await emailStrategy.login(email, password);
      // Save session
      const session: SessionData = {
        token: response.token,
        playerId: 0,
        gameId: 0,
        email: response.email,
        role: "player",
      };
      localStorage.setItem("google_auth_session", JSON.stringify(session));
      setSession(session);
      return response;
    } catch (err) {
      const error = err as Error;
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const registerWithEmail = async (
    email: string,
    password: string
  ): Promise<EmailAuthResponse> => {
    setLoading(true);
    setError(null);

    try {
      const response = await emailStrategy.register(email, password);
      // Save session
      const session: SessionData = {
        token: response.token,
        playerId: 0,
        gameId: 0,
        email: response.email,
        role: "player",
      };
      localStorage.setItem("google_auth_session", JSON.stringify(session));
      setSession(session);
      return response;
    } catch (err) {
      const error = err as Error;
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const value: AuthContextValue = {
    session,
    isAuthenticated: session !== null,
    loading,
    error,
    permissions,
    loginWithGoogle,
    loginWithEmail,
    registerWithEmail,
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
