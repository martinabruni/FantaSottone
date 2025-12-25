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
  const [showCredentials, setShowCredentials] = useState(false);
  const [credentials, setCredentials] = useState<
    Array<{ username: string; accessCode: string }>
  >([]);

  const { startGame } = useGames();
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
        variant: "destructive",
        title: "Errore di validazione",
        description: "Inserisci nome partita e punteggio iniziale",
      });
      return;
    }

    if (players.some((p) => !p.username || !p.accessCode)) {
      toast({
        variant: "destructive",
        title: "Errore di validazione",
        description: "Tutti i giocatori devono avere nome utente e codice di accesso",
      });
      return;
    }

    if (rules.some((r) => !r.name)) {
      toast({
        variant: "destructive",
        title: "Errore di validazione",
        description: "Tutte le regole devono avere un nome",
      });
      return;
    }

    setLoading(true);

    try {
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

      setCredentials(result.credentials);
      setShowCredentials(true);

      toast({
        title: "Partita creata!",
        description: "Salva le credenziali qui sotto per accedere alla partita",
      });
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Creazione partita non riuscita",
        description:
          error instanceof Error ? error.message : "Si e verificato un errore",
      });
    } finally {
      setLoading(false);
    }
  };

  if (showCredentials) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Partita creata!</CardTitle>
          <CardDescription>
            Condividi queste credenziali con i giocatori
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            {credentials.map((cred, idx) => (
              <div key={idx} className="p-3 border rounded-md space-y-1">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{cred.username}</span>
                  {idx === 0 && <Badge variant="secondary">Creatore</Badge>}
                </div>
                <p className="text-sm text-muted-foreground">
                  Codice: {cred.accessCode}
                </p>
              </div>
            ))}
          </div>
          <Button onClick={() => navigate("/")} className="w-full">
            Vai al login
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Crea nuova partita</CardTitle>
        <CardDescription>
          Configura una nuova partita con giocatori e regole
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
              <Label>Giocatori</Label>
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
            {loading ? "Creazione..." : "Crea partita"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
