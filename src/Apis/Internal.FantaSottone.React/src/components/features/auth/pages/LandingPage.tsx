import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/providers/auth/AuthProvider";
import { LoginForm } from "../components/LoginForm";

export function LandingPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  // Se l'utente è già autenticato, reindirizza alla lista partite
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/games");
    }
  }, [isAuthenticated, navigate]);

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="w-full max-w-md space-y-4">
        <div className="text-center mb-6">
          <h1 className="text-4xl font-bold mb-2">FantaSottone</h1>
          <p className="text-muted-foreground">
            Benvenuto! Accedi o registrati per iniziare a giocare
          </p>
        </div>
        <LoginForm />
      </div>
    </div>
  );
}
