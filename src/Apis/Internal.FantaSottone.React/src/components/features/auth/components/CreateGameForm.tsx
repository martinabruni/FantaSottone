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
        title: "Validation error",
        description: "Please fill in game name and initial score",
      });
      return;
    }

    if (players.some((p) => !p.username || !p.accessCode)) {
      toast({
        variant: "destructive",
        title: "Validation error",
        description: "All players must have username and access code",
      });
      return;
    }

    if (rules.some((r) => !r.name)) {
      toast({
        variant: "destructive",
        title: "Validation error",
        description: "All rules must have a name",
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
        title: "Game created!",
        description: "Save the credentials below to access the game",
      });
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Failed to create game",
        description:
          error instanceof Error ? error.message : "An error occurred",
      });
    } finally {
      setLoading(false);
    }
  };

  if (showCredentials) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Game Created!</CardTitle>
          <CardDescription>
            Share these credentials with your players
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            {credentials.map((cred, idx) => (
              <div key={idx} className="p-3 border rounded-md space-y-1">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{cred.username}</span>
                  {idx === 0 && <Badge variant="secondary">Creator</Badge>}
                </div>
                <p className="text-sm text-muted-foreground">
                  Code: {cred.accessCode}
                </p>
              </div>
            ))}
          </div>
          <Button onClick={() => navigate("/")} className="w-full">
            Go to Login
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Create New Game</CardTitle>
        <CardDescription>
          Setup a new game with players and rules
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="gameName">Game Name</Label>
            <Input
              id="gameName"
              type="text"
              placeholder="Enter game name"
              value={gameName}
              onChange={(e) => setGameName(e.target.value)}
              disabled={loading}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="initialScore">Initial Score</Label>
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
              <Label>Players</Label>
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={addPlayer}
                disabled={loading}
              >
                <Plus className="h-4 w-4 mr-1" /> Add Player
              </Button>
            </div>
            {players.map((player, idx) => (
              <div key={player.id} className="flex gap-2 items-end">
                <div className="flex-1 space-y-2">
                  <Input
                    placeholder="Username"
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
                    placeholder="Access code"
                    value={player.accessCode}
                    onChange={(e) =>
                      updatePlayer(player.id, "accessCode", e.target.value)
                    }
                    disabled={loading}
                  />
                </div>
                {idx === 0 ? (
                  <Badge variant="secondary" className="h-10 px-3">
                    Creator
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
              <Label>Rules</Label>
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
                    placeholder="Rule name"
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
                    placeholder="Score"
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
            {loading ? "Creating..." : "Create Game"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
