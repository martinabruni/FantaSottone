import { useParams } from "react-router-dom";
import { useLeaderboard } from "@/providers/leaderboard/LeaderboardProvider";
import { usePolling } from "@/hooks/usePolling";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { GameStatus } from "@/types/entities";
import { Crown, Trophy, FileEdit } from "lucide-react";
import { useEffect } from "react";

interface GameStatusBarProps {
  onStatusChange?: (status: GameStatus) => void;
}

export function GameStatusBar({ onStatusChange }: GameStatusBarProps) {
  const { gameId } = useParams<{ gameId: string }>();
  const { getLeaderboard } = useLeaderboard();

  const pollingInterval = parseInt(
    import.meta.env.VITE_POLLING_INTERVAL_MS || "3000"
  );

  const { data: leaderboard } = usePolling(
    async () => getLeaderboard(parseInt(gameId!)),
    {
      interval: pollingInterval,
      enabled: !!gameId,
    }
  );

  const gameStatus =
    leaderboard && leaderboard.length > 0 ? leaderboard[0].gameStatus : null;

  useEffect(() => {
    if (gameStatus && onStatusChange) {
      onStatusChange(gameStatus);
    }
  }, [gameStatus, onStatusChange]);

  if (!leaderboard || leaderboard.length === 0) return null;

  const winner = leaderboard[0];

  const statusText =
    gameStatus === GameStatus.Draft
      ? "Bozza"
      : gameStatus === GameStatus.Started
      ? "In corso"
      : "Terminata";

  const statusVariant =
    gameStatus === GameStatus.Draft
      ? "outline"
      : gameStatus === GameStatus.Ended
      ? "default"
      : "secondary";

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span>Stato partita</span>
          <Badge variant={statusVariant}>{statusText}</Badge>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {gameStatus === GameStatus.Draft && (
          <div className="flex items-center gap-3 p-4 bg-yellow-50 rounded-md border border-yellow-200">
            <FileEdit className="h-6 w-6 text-yellow-600" />
            <div className="flex-1">
              <p className="font-semibold text-yellow-800">
                Partita in preparazione
              </p>
              <p className="text-sm text-yellow-700">
                Il creatore può aggiungere giocatori, modificare le regole e
                avviare la partita quando è pronto.
              </p>
            </div>
          </div>
        )}
        {gameStatus === GameStatus.Ended && winner && (
          <div className="flex items-center gap-3 p-4 bg-primary/10 rounded-md border border-primary/20">
            <Crown className="h-6 w-6 text-yellow-500" />
            <div className="flex-1">
              <p className="font-semibold text-lg flex items-center gap-2">
                <Trophy className="h-4 w-4" />
                Vincitore: {winner.email}
              </p>
              <p className="text-sm text-muted-foreground">
                Punteggio finale: {winner.currentScore} punti
              </p>
            </div>
          </div>
        )}
        {gameStatus === GameStatus.Started && (
          <p className="text-sm text-muted-foreground">
            La partita è in corso. I giocatori possono assegnare le regole.
          </p>
        )}
      </CardContent>
    </Card>
  );
}
