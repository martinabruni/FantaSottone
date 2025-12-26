// src/Apis/Internal.FantaSottone.React/src/components/features/auth/components/CreateGameForm.tsx

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
import { useGames } from "@/providers/games/GamesProvider";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";
import { RuleType } from "@/types/entities";
import { Plus, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";

interface PlayerInput {
  id: string;
  username: string;
  accessCode: string;
  isCreator: boolean;
}

interface RuleInput {
  id: string;
  name: string;
  ruleType: RuleType;
  scoreDelta: number;
}

export function CreateGameForm() {
  const [gameName, setGameName] = useState("");
  const [initialScore, setInitialScore] = useState("100");
  const [players, setPlayers] = useState<PlayerInput[]>([
    { id: "1", username: "", accessCode: "", isCreator: true },
  ]);
  const [rules, setRules] = useState<RuleInput[]>([]);
  const [loading, setLoading] = useState(false);

  const { startGame } = useGames();
  const { login } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();

  const addPlayer = () => {
    setPlayers([
      ...players,
      {
        id: Date.now().toString(),
        username: "",
        accessCode: "",
        isCreator: false,
      },
    ]);
  };

  const removePlayer = (id: string) => {
    if (players.length > 1) {
      setPlayers(players.filter((p) => p.id !== id));
    }
  };

  const updatePlayer = (
    id: string,
    field: keyof PlayerInput,
    value: string | boolean
  ) => {
    setPlayers(
      players.map((p) => (p.id === id ? { ...p, [field]: value } : p))
    );
  };

  const addRule = (type: RuleType) => {
    setRules([
      ...rules,
      {
        id: Date.now().toString(),
        name: "",
        ruleType: type,
        scoreDelta: type === RuleType.Bonus ? 10 : -10,
      },
    ]);
  };

  const removeRule = (id: string) => {
    setRules(rules.filter((r) => r.id !== id));
  };

  const updateRule = (
    id: string,
    field: keyof RuleInput,
    value: string | number
  ) => {
    setRules(rules.map((r) => (r.id === id ? { ...r, [field]: value } : r)));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!gameName || !initialScore) {
      toast({
        variant: "error",
        title: "Errore di validazione",
        description: "Inserisci nome partita e punteggio iniziale",
      });
      return;
    }

    if (players.some((p) => !p.username || !p.accessCode)) {
      toast({
        variant: "error",
        title: "Errore di validazione",
        description:
          "Tutti i giocatori devono avere nome utente e codice di accesso",
      });
      return;
    }

    // FEATURE 1: Validazione minimo 2 giocatori (1 creatore + 1 normale)
    const creatorCount = players.filter((p) => p.isCreator).length;
    const normalPlayerCount = players.filter((p) => !p.isCreator).length;

    if (creatorCount === 0) {
      toast({
        variant: "error",
        title: "Errore di validazione",
        description: "Deve esserci almeno un creatore",
      });
      return;
    }

    if (normalPlayerCount === 0) {
      toast({
        variant: "error",
        title: "Errore di validazione",
        description:
          "Deve esserci almeno un giocatore normale oltre al creatore",
      });
      return;
    }

    if (rules.some((r) => !r.name)) {
      toast({
        variant: "error",
        title: "Errore di validazione",
        description: "Tutte le regole devono avere un nome",
      });
      return;
    }

    setLoading(true);

    try {
      // Crea la partita
      const result = await startGame({
        name: gameName,
        initialScore: parseInt(initialScore),
        players: players.map((p) => ({
          username: p.username,
          accessCode: p.accessCode,
          isCreator: p.isCreator,
        })),
        rules: rules.map((r) => ({
          name: r.name,
          ruleType: r.ruleType,
          scoreDelta: r.scoreDelta,
        })),
      });

      toast({
        variant: "success",
        title: "Partita creata!",
        description: "Login automatico in corso...",
      });

      // FEATURE 2: Login automatico del creatore
      const creatorCredentials = result.credentials.find((c) => c.isCreator);
      if (!creatorCredentials) {
        toast({
          variant: "error",
          title: "Errore",
          description:
            "Impossibile trovare le credenziali del creatore per il login automatico",
        });
        return;
      }

      // Esegui il login del creatore
      const loginResult = await login({
        username: creatorCredentials.username,
        accessCode: creatorCredentials.accessCode,
      });

      if (loginResult) {
        toast({
          variant: "success",
          title: "Accesso riuscito",
          description: `Benvenuto nella partita, ${loginResult.player.username}!`,
        });
        // Naviga alla pagina di gioco
        navigate(`/game/${result.gameId}`);
      } else {
        toast({
          variant: "error",
          title: "Login automatico fallito",
          description:
            "Partita creata con successo, ma il login automatico è fallito",
        });
      }
    } catch (error) {
      toast({
        variant: "error",
        title: "Creazione partita non riuscita",
        description:
          error instanceof Error ? error.message : "Si è verificato un errore",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Crea nuova partita</CardTitle>
        <CardDescription>
          Configura una nuova partita con giocatori e regole (minimo 2
          giocatori: 1 creatore + 1 normale)
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="gameName">Nome partita</Label>
            <Input
              id="gameName"
              type="text"
              placeholder="Inserisci il nome della partita"
              value={gameName}
              onChange={(e) => setGameName(e.target.value)}
              disabled={loading}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="initialScore">Punteggio iniziale</Label>
            <Input
              id="initialScore"
              type="number"
              placeholder="100"
              value={initialScore}
              onChange={(e) => setInitialScore(e.target.value)}
              disabled={loading}
            />
          </div>

          <Separator />

          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label>Giocatori (min. 2: 1 creatore + 1 normale)</Label>
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={addPlayer}
                disabled={loading}
              >
                <Plus className="h-4 w-4 mr-1" /> Aggiungi giocatore
              </Button>
            </div>
            {players.map((player, idx) => (
              <div key={player.id} className="flex gap-2 items-end">
                <div className="flex-1 space-y-2">
                  <Input
                    placeholder="Nome utente"
                    value={player.username}
                    onChange={(e) =>
                      updatePlayer(player.id, "username", e.target.value)
                    }
                    disabled={loading}
                  />
                </div>
                <div className="flex-1 space-y-2">
                  <Input
                    type="password"
                    placeholder="Codice di accesso"
                    value={player.accessCode}
                    onChange={(e) =>
                      updatePlayer(player.id, "accessCode", e.target.value)
                    }
                    disabled={loading}
                  />
                </div>
                {idx === 0 ? (
                  <Badge variant="secondary" className="h-10 px-3">
                    Creatore
                  </Badge>
                ) : (
                  <Button
                    type="button"
                    size="icon"
                    variant="ghost"
                    onClick={() => removePlayer(player.id)}
                    disabled={loading}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                )}
              </div>
            ))}
          </div>

          <Separator />

          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label>Regole</Label>
              <div className="flex gap-2">
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={() => addRule(RuleType.Bonus)}
                  disabled={loading}
                >
                  <Plus className="h-4 w-4 mr-1" /> Bonus
                </Button>
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={() => addRule(RuleType.Malus)}
                  disabled={loading}
                >
                  <Plus className="h-4 w-4 mr-1" /> Malus
                </Button>
              </div>
            </div>
            {rules.map((rule) => (
              <div key={rule.id} className="flex gap-2 items-end">
                <div className="flex-1">
                  <Input
                    placeholder="Nome regola"
                    value={rule.name}
                    onChange={(e) =>
                      updateRule(rule.id, "name", e.target.value)
                    }
                    disabled={loading}
                  />
                </div>
                <div className="w-24">
                  <Input
                    type="number"
                    placeholder="Punteggio"
                    value={rule.scoreDelta}
                    onChange={(e) =>
                      updateRule(
                        rule.id,
                        "scoreDelta",
                        parseInt(e.target.value)
                      )
                    }
                    disabled={loading}
                  />
                </div>
                <Badge
                  variant={
                    rule.ruleType === RuleType.Bonus ? "default" : "destructive"
                  }
                  className="h-10 px-3"
                >
                  {rule.ruleType === RuleType.Bonus ? "Bonus" : "Malus"}
                </Badge>
                <Button
                  type="button"
                  size="icon"
                  variant="ghost"
                  onClick={() => removeRule(rule.id)}
                  disabled={loading}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ))}
          </div>

          <Button type="submit" className="w-full" disabled={loading}>
            {loading
              ? "Creazione e login in corso..."
              : "Crea partita e accedi"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
