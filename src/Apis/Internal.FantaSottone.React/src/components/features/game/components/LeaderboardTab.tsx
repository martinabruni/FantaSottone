import { useParams } from "react-router-dom";
import { useLeaderboard } from "@/providers/leaderboard/LeaderboardProvider";
import { usePolling } from "@/hooks/usePolling";
import { useAuth } from "@/providers/auth/AuthProvider";
import { LoadingState } from "@/components/common/LoadingState";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import { Badge } from "@/components/ui/badge";
import { Trophy, Medal, Award } from "lucide-react";

export function LeaderboardTab() {
  const { gameId } = useParams<{ gameId: string }>();
  const { getLeaderboard } = useLeaderboard();
  const { session } = useAuth();

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

  if (loading && !leaderboard)
    return <LoadingState message="Loading leaderboard..." />;
  if (error) return <ErrorState message={error.message} onRetry={refetch} />;
  if (!leaderboard || leaderboard.length === 0)
    return (
      <EmptyState title="No players" message="No players in this game yet" />
    );

  const getRankIcon = (index: number) => {
    if (index === 0) return <Trophy className="h-5 w-5 text-yellow-500" />;
    if (index === 1) return <Medal className="h-5 w-5 text-gray-400" />;
    if (index === 2) return <Award className="h-5 w-5 text-orange-600" />;
    return <span className="text-muted-foreground">#{index + 1}</span>;
  };

  return (
    <div className="space-y-3">
      {leaderboard.map((player, index) => {
        const isCurrentPlayer = session?.playerId === player.Id;

        return (
          <div
            key={player.Id}
            className={`flex items-center justify-between p-4 rounded-lg border ${
              isCurrentPlayer ? "bg-primary/5 border-primary" : "bg-card"
            }`}
          >
            <div className="flex items-center gap-4">
              <div className="w-8 flex items-center justify-center">
                {getRankIcon(index)}
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <span className="font-semibold">{player.Username}</span>
                  {isCurrentPlayer && <Badge variant="secondary">You</Badge>}
                  {player.IsCreator && <Badge variant="outline">Creator</Badge>}
                </div>
              </div>
            </div>
            <div className="text-right">
              <span className="text-2xl font-bold">{player.CurrentScore}</span>
              <p className="text-xs text-muted-foreground">points</p>
            </div>
          </div>
        );
      })}
    </div>
  );
}
