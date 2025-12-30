import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";
import { GoogleLoginButton } from "./GoogleLoginButton";

export function LoginForm() {
  const [loading, setLoading] = useState(false);

  const { loginWithGoogle } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();

  const handleGoogleLogin = async (idToken: string) => {
    setLoading(true);
    try {
      const response = await loginWithGoogle(idToken);
      
      toast({
        variant: "success",
        title: response.isFirstLogin ? "Account creato!" : "Accesso riuscito",
        description: response.isFirstLogin
          ? `Benvenuto! Account creato per ${response.email}`
          : `Bentornato, ${response.email}!`,
      });

      // Redirect to home or games list
      navigate("/games");
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore durante il login con Google",
        description:
          error instanceof Error
            ? error.message
            : "Si è verificato un errore imprevisto",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card className="w-full max-w-md mx-auto">
      <CardHeader>
        <CardTitle>Accedi</CardTitle>
        <CardDescription>
          Accedi con il tuo account Google per iniziare a giocare
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* ✅ SOLO Google Login - rimosso il form tradizionale con username/accessCode */}
        <div className="space-y-2">
          <GoogleLoginButton
            onSuccess={handleGoogleLogin}
            onError={() => setLoading(false)}
          />
        </div>
        {loading && (
          <p className="text-sm text-center text-muted-foreground">
            Accesso in corso...
          </p>
        )}
      </CardContent>
    </Card>
  );
}
