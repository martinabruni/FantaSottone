import { useParams, Navigate } from "react-router-dom";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useGames } from "@/providers/games/GamesProvider";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
// import { GameStatusBar } from "../components/GameStatusBar";
import { LeaderboardTab } from "../components/LeaderboardTab";
import { RulesTab } from "../components/RulesTab";
import { EndGameDialog } from "../components/EndGameDialog";
import { ActionButton } from "@/components/common/ActionButton";
import { useState } from "react";
import { useToast } from "@/hooks/useToast";
import { usePolling } from "@/hooks/usePolling";
import { useLeaderboard } from "@/providers/leaderboard/LeaderboardProvider";
import { GameStatus } from "@/types/entities";

export function GamePage() {
  const { gameId } = useParams<{ gameId: string }>();
  const { session, isAuthenticated } = useAuth();
  const { endGame } = useGames();
  const { getLeaderboard } = useLeaderboard();
  const { toast } = useToast();
  const [endGameDialogOpen, setEndGameDialogOpen] = useState(false);
  const [gameStatus, setGameStatus] = useState<GameStatus | null>(null);

  // Poll leaderboard to get game status
  const pollingInterval = parseInt(
    import.meta.env.VITE_POLLING_INTERVAL_MS || "3000"
  );

  usePolling(
    async () => {
      if (!gameId) return null;
      const leaderboard = await getLeaderboard(parseInt(gameId));
      // In a real implementation, you'd get the status from a dedicated endpoint
      // For now, we'll track it in state after ending
      return leaderboard;
    },
    {
      interval: gameStatus === GameStatus.Ended ? 10000 : pollingInterval,
      enabled: !!gameId && isAuthenticated,
    }
  );

  if (!isAuthenticated || !session) {
    return <Navigate to="/" replace />;
  }

  if (!gameId) {
    return <Navigate to="/" replace />;
  }

  const handleEndGame = async () => {
    try {
      const response = await endGame(parseInt(gameId));
      setGameStatus(response.game.status);
      toast({
        variant: "success",
        title: "Partita terminata",
        description: `Vincitore: ${response.winner.username} con ${response.winner.currentScore} punti!`,
      });
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore",
        description:
          error instanceof Error
            ? error.message
            : "Impossibile terminare la partita",
      });
    }
  };

  const isCreator = session.role === "creator";
  const canEndGame = isCreator && gameStatus !== GameStatus.Ended;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        {/* <GameStatusBar onStatusChange={setGameStatus} /> */}
        {canEndGame && (
          <ActionButton
            actionType="error"
            onClick={() => setEndGameDialogOpen(true)}
          >
            Termina partita
          </ActionButton>
        )}
      </div>

      <Tabs defaultValue="leaderboard" className="w-full">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="leaderboard">Classifica</TabsTrigger>
          <TabsTrigger value="rules">Regole</TabsTrigger>
        </TabsList>

        <TabsContent value="leaderboard" className="mt-6">
          <LeaderboardTab />
        </TabsContent>

        <TabsContent value="rules" className="mt-6">
          <RulesTab gameStatus={gameStatus} />
        </TabsContent>
      </Tabs>

      <EndGameDialog
        open={endGameDialogOpen}
        onOpenChange={setEndGameDialogOpen}
        onConfirm={handleEndGame}
      />
    </div>
  );
}
