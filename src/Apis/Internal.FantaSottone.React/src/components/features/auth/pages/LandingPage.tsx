import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { GoogleLoginButton } from "../components/GoogleLoginButton";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useToast } from "@/hooks/useToast";

export function LandingPage() {
  const navigate = useNavigate();
  const { loginWithGoogle, isAuthenticated } = useAuth();
  const { toast } = useToast();

  // Se l'utente è già autenticato, reindirizza alla lista partite
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/games");
    }
  }, [isAuthenticated, navigate]);

  const handleGoogleLogin = async (idToken: string) => {
    try {
      const response = await loginWithGoogle(idToken);

      toast({
        variant: "success",
        title: response.isFirstLogin ? "Account creato!" : "Accesso riuscito",
        description: response.isFirstLogin
          ? `Benvenuto! Account creato per ${response.email}`
          : `Bentornato, ${response.email}!`,
      });

      // Reindirizza alla lista delle partite
      navigate("/games");
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore durante il login",
        description:
          error instanceof Error
            ? error.message
            : "Si è verificato un errore imprevisto",
      });
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-3xl font-bold">FantaSottone</CardTitle>
          <CardDescription>
            Accedi con il tuo account Google per iniziare a giocare
          </CardDescription>
        </CardHeader>
        <CardContent>
          <GoogleLoginButton onSuccess={handleGoogleLogin} onError={() => {}} />
        </CardContent>
      </Card>
    </div>
  );
}
