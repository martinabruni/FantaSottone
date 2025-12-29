import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";
import { GoogleLoginButton } from "./GoogleLoginButton";

export function LoginForm() {
  const [username, setUsername] = useState("");
  const [accessCode, setAccessCode] = useState("");
  const [loading, setLoading] = useState(false);

  const { login, loginWithGoogle } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!username || !accessCode) {
      toast({
        variant: "error",
        title: "Errore di validazione",
        description: "Compila tutti i campi",
      });
      return;
    }

    setLoading(true);

    try {
      const result = await login({ username, accessCode });
      if (result) {
        toast({
          variant: "success",
          title: "Accesso riuscito",
          description: `Benvenuto, ${result.player.username}!`,
        });
        navigate(`/game/${result.game.id}`);
      } else {
        toast({
          variant: "error",
          title: "Accesso non riuscito",
          description: "Credenziali non valide",
        });
      }
    } catch (error) {
      toast({
        variant: "error",
        title: "Accesso non riuscito",
        description:
          error instanceof Error
            ? error.message
            : "Si è verificato un errore imprevisto",
      });
    } finally {
      setLoading(false);
    }
  };

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
          Inserisci le tue credenziali per accedere al gioco
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Google Login Section */}
        <div className="space-y-2">
          <GoogleLoginButton
            onSuccess={handleGoogleLogin}
            onError={() => setLoading(false)}
          />
        </div>

        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <Separator />
          </div>
          <div className="relative flex justify-center text-xs uppercase">
            <span className="bg-background px-2 text-muted-foreground">
              Oppure continua con
            </span>
          </div>
        </div>

        {/* Traditional Login Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="username">Username</Label>
            <Input
              id="username"
              type="text"
              placeholder="Il tuo username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={loading}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="accessCode">Codice di accesso</Label>
            <Input
              id="accessCode"
              type="password"
              placeholder="Il tuo codice"
              value={accessCode}
              onChange={(e) => setAccessCode(e.target.value)}
              disabled={loading}
            />
          </div>

          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? "Accesso in corso..." : "Accedi"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
