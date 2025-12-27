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
import { Plus, Trash2, UserCheck } from "lucide-react";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";
import { useGames } from "@/providers/games/GamesProvider";
import { RuleType } from "@/types/entities";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

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
    {
      id: "1",
      username: "",
      accessCode: "",
      isCreator: true,
    },
    {
      id: "2",
      username: "",
      accessCode: "",
      isCreator: false,
    },
  ]);
  const [rules, setRules] = useState<RuleInput[]>([]);
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();
  const { startGame } = useGames();

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
    if (players.length <= 2) {
      toast({
        variant: "error",
        title: "Errore",
        description: "Servono almeno 2 giocatori",
      });
      return;
    }
    setPlayers(players.filter((p) => p.id !== id));
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
    <Card className="w-full">
      <CardHeader>
        <CardTitle className="text-xl sm:text-2xl">
          Crea nuova partita
        </CardTitle>
        <CardDescription className="text-sm sm:text-base">
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
              className="text-base"
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
              className="text-base"
            />
          </div>

          <Separator />

          <div className="space-y-4">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2">
              <Label>Giocatori (min. 2: 1 creatore + 1 normale)</Label>
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={addPlayer}
                disabled={loading}
                className="w-full sm:w-auto"
              >
                <Plus className="h-4 w-4 mr-1" /> Aggiungi giocatore
              </Button>
            </div>
            {players.map((player, idx) => (
              <div key={player.id} className="flex flex-col sm:flex-row gap-2">
                <div className="flex-1 space-y-2">
                  <Input
                    placeholder="Nome utente"
                    value={player.username}
                    onChange={(e) =>
                      updatePlayer(player.id, "username", e.target.value)
                    }
                    disabled={loading}
                    className="text-base"
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
                    className="text-base"
                  />
                </div>
                <div className="flex gap-2">
                  {idx === 0 ? (
                    <Button
                      type="button"
                      size="icon"
                      variant={player.isCreator ? "default" : "outline"}
                      className="h-10 w-10 flex-shrink-0"
                      disabled
                    >
                      <UserCheck className="h-4 w-4" />
                    </Button>
                  ) : (
                    <>
                      {/* <Button
                        type="button"
                        size="icon"
                        variant={player.isCreator ? "default" : "outline"}
                        onClick={() =>
                          updatePlayer(
                            player.id,
                            "isCreator",
                            !player.isCreator
                          )
                        }
                        disabled={loading}
                        className="h-10 w-10 flex-shrink-0"
                      >
                        <UserCheck className="h-4 w-4" />
                      </Button> */}
                      <Button
                        type="button"
                        size="icon"
                        variant="destructive"
                        onClick={() => removePlayer(player.id)}
                        disabled={loading || players.length <= 2}
                        className="h-10 w-10 flex-shrink-0"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>

          <Separator />

          <div className="space-y-4">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2">
              <Label>Regole</Label>
              <div className="flex gap-2 w-full sm:w-auto">
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={() => addRule(RuleType.Bonus)}
                  disabled={loading}
                  className="flex-1 sm:flex-none"
                >
                  <Plus className="h-4 w-4 mr-1" /> Bonus
                </Button>
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={() => addRule(RuleType.Malus)}
                  disabled={loading}
                  className="flex-1 sm:flex-none"
                >
                  <Plus className="h-4 w-4 mr-1" /> Malus
                </Button>
              </div>
            </div>
            {rules.map((rule) => (
              <div key={rule.id} className="flex flex-col sm:flex-row gap-2">
                <div className="flex-1">
                  <Input
                    placeholder="Nome regola"
                    value={rule.name}
                    onChange={(e) =>
                      updateRule(rule.id, "name", e.target.value)
                    }
                    disabled={loading}
                    className="text-base"
                  />
                </div>
                <div className="flex gap-2">
                  <Select
                    value={rule.ruleType.toString()}
                    onValueChange={(value: string) =>
                      updateRule(rule.id, "ruleType", parseInt(value))
                    }
                    disabled={loading}
                  >
                    <SelectTrigger className="w-full sm:w-32">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={RuleType.Bonus.toString()}>
                        Bonus
                      </SelectItem>
                      <SelectItem value={RuleType.Malus.toString()}>
                        Malus
                      </SelectItem>
                    </SelectContent>
                  </Select>
                  <Input
                    type="number"
                    placeholder="Punti"
                    value={Math.abs(rule.scoreDelta)}
                    onChange={(e) =>
                      updateRule(
                        rule.id,
                        "scoreDelta",
                        rule.ruleType === RuleType.Bonus
                          ? Math.abs(parseInt(e.target.value) || 0)
                          : -Math.abs(parseInt(e.target.value) || 0)
                      )
                    }
                    disabled={loading}
                    className="w-20 sm:w-24 text-base"
                  />
                  <Button
                    type="button"
                    size="icon"
                    variant="destructive"
                    onClick={() => removeRule(rule.id)}
                    disabled={loading}
                    className="h-10 w-10 flex-shrink-0"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>

          <Button
            type="submit"
            className="w-full h-12 text-base sm:text-lg"
            disabled={loading}
          >
            {loading
              ? "Creazione e login in corso..."
              : "Crea partita e accedi"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
