import { useParams } from "react-router-dom";
import { useAssignments } from "@/providers/assignments/AssignmentsProvider";
import { usePolling } from "@/hooks/usePolling";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { GameStatus } from "@/types/entities";
import { Crown } from "lucide-react";
import { useEffect } from "react";

interface GameStatusBarProps {
  onStatusChange?: (status: GameStatus) => void;
}

export function GameStatusBar({ onStatusChange }: GameStatusBarProps) {
  const { gameId } = useParams<{ gameId: string }>();
  const { getGameStatus } = useAssignments();

  const pollingInterval = parseInt(
    import.meta.env.VITE_POLLING_INTERVAL_MS || "3000"
  );

  const { data: status } = usePolling(
    async () => getGameStatus(parseInt(gameId!)),
    {
      interval: pollingInterval,
      enabled: !!gameId,
    }
  );

  useEffect(() => {
    if (status && onStatusChange) {
      onStatusChange(status.game.status);
    }
  }, [status, onStatusChange]);

  if (!status) return null;

  const statusText =
    status.game.status === GameStatus.Draft
      ? "Bozza"
      : status.game.status === GameStatus.Started
      ? "In corso"
      : "Terminata";

  const statusVariant =
    status.game.status === GameStatus.Ended ? "default" : "secondary";

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span>Stato partita</span>
          <Badge variant={statusVariant}>{statusText}</Badge>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {status.game.status === GameStatus.Ended && status.winner && (
          <div className="flex items-center gap-2 p-4 bg-primary/10 rounded-md">
            <Crown className="h-5 w-5 text-primary" />
            <div>
              <p className="font-semibold">
                Vincitore: {status.winner.csername}
              </p>
              <p className="text-sm text-muted-foreground">
                Punteggio finale: {status.winner.currentScore}
              </p>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
