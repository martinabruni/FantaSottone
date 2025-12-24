import { useParams } from "react-router-dom";
import { useRules } from "@/providers/rules/RulesProvider";
import { usePolling } from "@/hooks/usePolling";
import { useAuth } from "@/providers/auth/AuthProvider";
import { LoadingState } from "@/components/common/LoadingState";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { RuleType } from "@/types/entities";
import { Clock, CheckCircle2 } from "lucide-react";
import { useToast } from "@/hooks/useToast";
import { useState } from "react";
import { ConflictError } from "@/lib/http/errors";

export function RulesTab() {
  const { gameId } = useParams<{ gameId: string }>();
  const { getRules, assignRule } = useRules();
  const { session } = useAuth();
  const { toast } = useToast();
  const [assigning, setAssigning] = useState<number | null>(null);

  const pollingInterval = parseInt(
    import.meta.env.VITE_POLLING_INTERVAL_MS || "3000"
  );

  const {
    data: rules,
    loading,
    error,
    refetch,
  } = usePolling(async () => getRules(parseInt(gameId!)), {
    interval: pollingInterval,
    enabled: !!gameId,
  });

  const handleAssign = async (ruleId: number) => {
    if (!session || !gameId) return;

    setAssigning(ruleId);

    try {
      await assignRule(parseInt(gameId), ruleId, session.playerId);

      toast({
        title: "Rule assigned!",
        description: "Your score has been updated",
      });

      // Refetch to update UI
      await refetch();
    } catch (err) {
      if (err instanceof ConflictError) {
        toast({
          variant: "destructive",
          title: "Rule already assigned",
          description: "Someone else claimed this rule first!",
        });
        await refetch();
      } else {
        toast({
          variant: "destructive",
          title: "Assignment failed",
          description: err instanceof Error ? err.message : "An error occurred",
        });
      }
    } finally {
      setAssigning(null);
    }
  };

  if (loading && !rules) return <LoadingState message="Loading rules..." />;
  if (error) return <ErrorState message={error.message} onRetry={refetch} />;
  if (!rules || rules.length === 0)
    return (
      <EmptyState title="No rules" message="No rules defined for this game" />
    );

  return (
    <div className="space-y-3">
      {rules.map(({ rule, assignment }) => {
        const isAssigned = !!assignment;
        const isAssignedToMe =
          assignment?.assignedToPlayerId === session?.playerId;
        const canAssign = !isAssigned && !assigning;

        return (
          <div
            key={rule.Id}
            className={`flex items-center justify-between p-4 rounded-lg border ${
              isAssignedToMe ? "bg-primary/5 border-primary" : "bg-card"
            }`}
          >
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <span className="font-semibold">{rule.Name}</span>
                <Badge
                  variant={
                    rule.RuleType === RuleType.Bonus ? "default" : "destructive"
                  }
                >
                  {rule.RuleType === RuleType.Bonus ? "Bonus" : "Malus"}{" "}
                  {rule.ScoreDelta > 0 ? "+" : ""}
                  {rule.ScoreDelta}
                </Badge>
              </div>

              {isAssigned && assignment && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <CheckCircle2 className="h-4 w-4" />
                  <span>
                    Assigned to <strong>{assignment.assignedToUsername}</strong>
                  </span>
                  <Clock className="h-3 w-3 ml-2" />
                  <span>
                    {new Date(assignment.assignedAt).toLocaleString()}
                  </span>
                </div>
              )}
            </div>

            <Button
              onClick={() => handleAssign(rule.Id)}
              disabled={!canAssign || assigning === rule.Id}
              variant={isAssignedToMe ? "secondary" : "default"}
              size="sm"
            >
              {assigning === rule.Id
                ? "Assigning..."
                : isAssignedToMe
                ? "Assigned to You"
                : isAssigned
                ? "Assigned"
                : "Assign to Me"}
            </Button>
          </div>
        );
      })}
    </div>
  );
}
