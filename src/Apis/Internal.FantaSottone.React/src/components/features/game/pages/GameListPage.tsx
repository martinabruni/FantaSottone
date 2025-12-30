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
import { useUsers } from "@/providers/users/UsersProvider"; // ADD THIS IMPORT

export function GameListPage() {
  const [games, setGames] = useState<GameInvitationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { toast } = useToast();
  
  // FIXED: Use useUsers() hook instead of creating transport directly
  const { getUserGames } = useUsers();

  useEffect(() => {
    loadGames();
  }, []);

  const loadGames = async () => {
    try {
      setLoading(true);
      // FIXED: Now this will include the bearer token in headers!
      const response = await getUserGames();
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
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Trophy className="h-16 w-16 text-muted-foreground mb-4" />
            <p className="text-xl font-semibold mb-2">
              Nessuna partita disponibile
            </p>
            <p className="text-muted-foreground mb-4 text-center">
              Non sei ancora stato invitato a nessuna partita. Crea la tua prima
              partita per iniziare!
            </p>
            <Button onClick={() => navigate("/games/create")}>
              <Plus className="mr-2 h-4 w-4" />
              Crea partita
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
                <div className="flex justify-between items-start">
                  <CardTitle className="text-lg">{game.gameName}</CardTitle>
                  {getStatusBadge(game.status)}
                </div>
                <CardDescription>
                  Creata il {formatDate(game.createdAt)}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <Users className="h-4 w-4 text-muted-foreground" />
                    <span>{game.playerCount} giocatori</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Trophy className="h-4 w-4 text-muted-foreground" />
                    <span>Punteggio: {game.initialScore}</span>
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
