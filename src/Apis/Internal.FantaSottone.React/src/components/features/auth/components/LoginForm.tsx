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
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";

export function LoginForm() {
  const [username, setUsername] = useState("");
  const [accessCode, setAccessCode] = useState("");
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
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
          error instanceof Error ? error.message : "Si e verificato un errore",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Entra in partita</CardTitle>
        <CardDescription>
          Inserisci le tue credenziali per entrare in una partita esistente
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="username">Nome utente</Label>
            <Input
              id="username"
              type="text"
              placeholder="Inserisci il nome utente"
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
              placeholder="Inserisci il codice di accesso"
              value={accessCode}
              onChange={(e) => setAccessCode(e.target.value)}
              disabled={loading}
            />
          </div>
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? "Accesso in corso..." : "Accedi"}
          </Button>
          <div className="text-xs text-muted-foreground space-y-1">
            <p>Credenziali di test:</p>
            <p>Nome utente: test1, Codice: code1 (Creatore)</p>
            <p>Nome utente: test2, Codice: code2 (Giocatore)</p>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
