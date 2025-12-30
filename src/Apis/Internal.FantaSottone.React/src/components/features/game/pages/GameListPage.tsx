import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/hooks/useToast";
import { Plus, Users, Trophy } from "lucide-react";
import { GameInvitationDto } from "@/types/user-types";
import { createTransport } from "@/lib/http/transportFactory";

export function GameListPage() {
  const [games, setGames] = useState<GameInvitationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { toast } = useToast();
  const transport = createTransport();

  useEffect(() => {
    loadGames();
  }, []);

  const loadGames = async () => {
    try {
      setLoading(true);
      const response = await transport.get<{ games: GameInvitationDto[] }>(
        "/api/Users/games"
      );
      setGames(response.games);
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore nel caricamento",
        description: "Impossibile caricare le partite",
      });
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: number) => {
    switch (status) {
      case 0: // Started
        return <Badge variant="default">In corso</Badge>;
      case 1: // Ended
        return <Badge variant="secondary">Terminata</Badge>;
      default:
        return <Badge variant="outline">Sconosciuto</Badge>;
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("it-IT", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">Caricamento...</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold mb-2">Le tue partite</h1>
          <p className="text-muted-foreground">
            Partite a cui sei stato invitato
          </p>
        </div>
        <Button onClick={() => navigate("/games/create")} size="lg">
          <Plus className="mr-2 h-5 w-5" />
          Crea nuova partita
        </Button>
      </div>

      {games.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Trophy className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">Nessuna partita</h3>
            <p className="text-muted-foreground mb-4">
              Non sei ancora stato invitato a nessuna partita
            </p>
            <Button onClick={() => navigate("/games/create")}>
              Crea la tua prima partita
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {games.map((game) => (
            <Card
              key={game.gameId}
              className="hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => navigate(`/game/${game.gameId}`)}
            >
              <CardHeader>
                <div className="flex justify-between items-start mb-2">
                  <CardTitle className="text-xl">{game.gameName}</CardTitle>
                  {getStatusBadge(game.status)}
                </div>
                <CardDescription>
                  Creata il {formatDate(game.createdAt)}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between text-sm">
                  <div className="flex items-center text-muted-foreground">
                    <Users className="mr-2 h-4 w-4" />
                    <span>{game.playerCount} giocatori</span>
                  </div>
                  <div className="font-semibold">
                    Punteggio iniziale: {game.initialScore}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
