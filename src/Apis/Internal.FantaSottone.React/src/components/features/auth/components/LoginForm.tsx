import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";
import { GoogleLoginButton } from "./GoogleLoginButton";

export function LoginForm() {
  const [loading, setLoading] = useState(false);
  const [isRegisterMode, setIsRegisterMode] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const { loginWithGoogle, loginWithEmail, registerWithEmail } = useAuth();
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
    } catch (error: any) {
      const errorTitle =
        error?.response?.title ||
        error?.title ||
        "Errore durante il login con Google";
      const errorDetail =
        error?.response?.detail ||
        error?.detail ||
        error?.message ||
        "Si è verificato un errore imprevisto";

      toast({
        variant: "error",
        title: errorTitle,
        description: errorDetail,
      });
    } finally {
      setLoading(false);
    }
  };

  const handleEmailSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (isRegisterMode) {
        const response = await registerWithEmail(email.trim(), password.trim());
        toast({
          variant: "success",
          title: "Registrazione completata!",
          description: `Account creato per ${response.email}`,
        });
      } else {
        const response = await loginWithEmail(email.trim(), password.trim());
        toast({
          variant: "success",
          title: "Accesso riuscito",
          description: `Bentornato, ${response.email}!`,
        });
      }

      navigate("/games");
    } catch (error: any) {
      const errorTitle =
        error?.response?.title ||
        error?.title ||
        (isRegisterMode
          ? "Errore durante la registrazione"
          : "Errore durante il login");
      const errorDetail =
        error?.response?.detail ||
        error?.detail ||
        error?.message ||
        "Si è verificato un errore imprevisto";

      toast({
        variant: "error",
        title: errorTitle,
        description: errorDetail,
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card className="w-full max-w-md mx-auto">
      <CardHeader>
        <CardTitle>{isRegisterMode ? "Registrati" : "Accedi"}</CardTitle>
        <CardDescription>
          {isRegisterMode
            ? "Crea un nuovo account con email e password"
            : "Accedi con il tuo account per iniziare a giocare"}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Email/Password Form */}
        <form onSubmit={handleEmailSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="text"
              placeholder="tua@email.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              disabled={loading}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={loading}
            />
          </div>
          <Button type="submit" className="w-full" disabled={loading}>
            {loading
              ? "Caricamento..."
              : isRegisterMode
              ? "Registrati"
              : "Accedi"}
          </Button>
        </form>

        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <span className="w-full border-t" />
          </div>
          <div className="relative flex justify-center text-xs uppercase">
            <span className="bg-background px-2 text-muted-foreground">
              Oppure
            </span>
          </div>
        </div>

        {/* Google Login */}
        <div className="space-y-2">
          <GoogleLoginButton
            onSuccess={handleGoogleLogin}
            onError={() => setLoading(false)}
          />
        </div>

        {/* Toggle Register/Login */}
        <div className="text-center text-sm">
          <button
            type="button"
            onClick={() => setIsRegisterMode(!isRegisterMode)}
            className="text-primary hover:underline"
            disabled={loading}
          >
            {isRegisterMode
              ? "Hai già un account? Accedi"
              : "Non hai un account? Registrati"}
          </button>
        </div>

        {loading && (
          <p className="text-sm text-center text-muted-foreground">
            {isRegisterMode
              ? "Registrazione in corso..."
              : "Accesso in corso..."}
          </p>
        )}
      </CardContent>
    </Card>
  );
}
