import { useState } from "react";
import { useParams } from "react-router-dom";
import { useLeaderboard } from "@/providers/leaderboard/LeaderboardProvider";
import { usePolling } from "@/hooks/usePolling";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useGame } from "@/providers/games/GameProvider";
import { useGames } from "@/providers/games/GamesProvider";
import { LoadingState } from "@/components/common/LoadingState";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useToast } from "@/hooks/useToast";
import { Trophy, Medal, Award, UserPlus } from "lucide-react";

// Game status enum matching backend
const GameStatus = {
  Draft: 1,
  Started: 2,
  Ended: 3,
};

export function LeaderboardTab() {
  const { gameId } = useParams<{ gameId: string }>();
  const { getLeaderboard } = useLeaderboard();
  const { session } = useAuth();
  const { currentPlayer } = useGame();
  const { invitePlayerByEmail } = useGames();
  const { toast } = useToast();

  const [isInviteDialogOpen, setIsInviteDialogOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState("");
  const [isInviting, setIsInviting] = useState(false);

  const pollingInterval = parseInt(
    import.meta.env.VITE_POLLING_INTERVAL_MS || "3000"
  );

  const {
    data: leaderboard,
    loading,
    error,
    refetch,
  } = usePolling(async () => getLeaderboard(parseInt(gameId!)), {
    interval: pollingInterval,
    enabled: !!gameId,
  });

  const handleInvitePlayer = async () => {
    if (!inviteEmail.trim()) {
      toast({
        variant: "error",
        title: "Username vuoto",
        description: "Inserisci un username valido",
      });
      return;
    }

    // Don't allow inviting yourself
    if (
      session?.email &&
      inviteEmail.trim().toLowerCase() === session.email.toLowerCase()
    ) {
      toast({
        variant: "error",
        title: "Username non valido",
        description: "Non puoi invitare te stesso",
      });
      return;
    }

    try {
      setIsInviting(true);
      await invitePlayerByEmail(parseInt(gameId!), {
        email: inviteEmail.trim(),
      });
      toast({
        variant: "success",
        title: "Giocatore invitato",
        description: `${inviteEmail} è stato invitato alla partita`,
      });
      setInviteEmail("");
      setIsInviteDialogOpen(false);
      refetch(); // Refresh leaderboard
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore nell'invito",
        description:
          error instanceof Error ? error.message : "Si è verificato un errore",
      });
    } finally {
      setIsInviting(false);
    }
  };

  if (loading && !leaderboard)
    return <LoadingState message="Caricamento classifica..." />;
  if (error) return <ErrorState message={error.message} onRetry={refetch} />;
  if (!leaderboard || leaderboard.length === 0)
    return (
      <EmptyState
        title="Nessun giocatore"
        message="Non ci sono ancora giocatori in questa partita"
      />
    );

  // Check if user is creator and game is in draft state
  const gameStatus = leaderboard[0]?.gameStatus;
  const isCreator = currentPlayer?.isCreator ?? false;
  const canInvitePlayers = isCreator && gameStatus === GameStatus.Draft;

  const getRankIcon = (index: number) => {
    if (index === 0) return <Trophy className="h-5 w-5 text-yellow-500" />;
    if (index === 1) return <Medal className="h-5 w-5 text-gray-400" />;
    if (index === 2) return <Award className="h-5 w-5 text-orange-600" />;
    return <span className="text-muted-foreground">#{index + 1}</span>;
  };

  return (
    <div className="space-y-3">
      {canInvitePlayers && (
        <div className="flex justify-end mb-4">
          <Dialog
            open={isInviteDialogOpen}
            onOpenChange={setIsInviteDialogOpen}
          >
            <DialogTrigger asChild>
              <Button variant="outline" size="sm">
                <UserPlus className="h-4 w-4 mr-2" />
                Invita giocatore
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Invita un giocatore</DialogTitle>
                <DialogDescription>
                  Inserisci lo username del giocatore da invitare alla partita.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label htmlFor="email">Username</Label>
                  <Input
                    id="email"
                    type="text"
                    placeholder="username"
                    value={inviteEmail}
                    onChange={(e) => setInviteEmail(e.target.value)}
                    onKeyPress={(e) => {
                      if (e.key === "Enter") {
                        e.preventDefault();
                        handleInvitePlayer();
                      }
                    }}
                    disabled={isInviting}
                  />
                </div>
              </div>
              <DialogFooter>
                <Button
                  variant="outline"
                  onClick={() => setIsInviteDialogOpen(false)}
                  disabled={isInviting}
                >
                  Annulla
                </Button>
                <Button onClick={handleInvitePlayer} disabled={isInviting}>
                  {isInviting ? "Invio in corso..." : "Invita"}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </div>
      )}
      {leaderboard.map((player, index) => {
        // Confronta l'email invece del playerId per identificare l'utente corrente
        const isCurrentPlayer =
          session?.email?.toLowerCase() === player.email?.toLowerCase();

        return (
          <div
            key={player.id}
            className={`flex items-center justify-between p-4 rounded-lg border transition-all ${
              isCurrentPlayer
                ? "bg-blue-50 border-blue-500 dark:bg-blue-950/40 dark:border-blue-600 shadow-md border-2 ring-2 ring-blue-200 dark:ring-blue-900"
                : "bg-card hover:bg-accent/50"
            }`}
          >
            <div className="flex items-center gap-4">
              <div className="w-8 flex items-center justify-center">
                {getRankIcon(index)}
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <span
                    className={`font-semibold ${
                      isCurrentPlayer ? "text-blue-700 dark:text-blue-300" : ""
                    }`}
                  >
                    {player.email}
                  </span>
                  {isCurrentPlayer && (
                    <Badge variant="default" className="bg-blue-600">
                      Tu
                    </Badge>
                  )}
                  {player.isCreator && (
                    <Badge variant="outline">Creatore</Badge>
                  )}
                </div>
              </div>
            </div>
            <div className="text-right">
              <span
                className={`text-2xl font-bold ${
                  isCurrentPlayer ? "text-blue-700 dark:text-blue-300" : ""
                }`}
              >
                {player.currentScore}
              </span>
              <p className="text-xs text-muted-foreground">punti</p>
            </div>
          </div>
        );
      })}
    </div>
  );
}
