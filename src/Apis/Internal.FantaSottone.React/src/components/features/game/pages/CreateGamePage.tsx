import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useToast } from "@/hooks/useToast";
import { ArrowLeft } from "lucide-react";
import { useGames } from "@/providers/games/GamesProvider";
import { useGame } from "@/providers/games/GameProvider";

export function CreateGamePage() {
  const [gameName, setGameName] = useState("");
  const [initialScore, setInitialScore] = useState("100");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();
  const { createGame } = useGames();
  const { joinGame } = useGame();

  const handleCreateGame = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!gameName.trim()) {
      toast({
        variant: "error",
        title: "Nome mancante",
        description: "Inserisci un nome per la partita",
      });
      return;
    }

    const score = parseInt(initialScore);
    if (isNaN(score) || score <= 0) {
      toast({
        variant: "error",
        title: "Punteggio non valido",
        description: "Il punteggio iniziale deve essere un numero positivo",
      });
      return;
    }

    try {
      setLoading(true);

      const response = await createGame({
        name: gameName,
        initialScore: score,
      });

      // Join the game automatically to get player role
      await joinGame(response.gameId);

      toast({
        variant: "success",
        title: "Partita creata!",
        description: `La partita "${response.gameName}" è stata creata con successo. Invita i giocatori dalla scheda Classifica.`,
      });

      // Navigate to the game page
      navigate(`/game/${response.gameId}`);
    } catch (error) {
      console.error("Error creating game:", error);
      toast({
        variant: "error",
        title: "Errore nella creazione",
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
    <div className="container mx-auto px-4 py-8 max-w-2xl">
      <Button
        variant="ghost"
        className="mb-4"
        onClick={() => navigate("/games")}
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Torna alle partite
      </Button>

      <Card>
        <CardHeader>
          <CardTitle>Crea una nuova partita</CardTitle>
          <CardDescription>
            Configura la partita. Potrai invitare i giocatori dalla scheda
            Classifica prima di avviare la partita.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleCreateGame} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="gameName">Nome partita</Label>
              <Input
                id="gameName"
                type="text"
                placeholder="La mia partita"
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
                min="1"
              />
            </div>

            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Creazione in corso..." : "Crea partita"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
