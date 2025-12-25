import { useParams } from "react-router-dom";
import { useRules } from "@/providers/rules/RulesProvider";
import { usePolling } from "@/hooks/usePolling";
import { useAuth } from "@/providers/auth/AuthProvider";
import { LoadingState } from "@/components/common/LoadingState";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import { ActionButton } from "@/components/common/ActionButton";
import { Badge } from "@/components/ui/badge";
import { RuleType } from "@/types/entities";
import { Clock, CheckCircle2, Pencil } from "lucide-react";
import { useToast } from "@/hooks/useToast";
import { useState } from "react";
import { ConflictError } from "@/lib/http/errors";
import { EditRuleDialog, EditRuleData } from "./EditRuleDialog";

export function RulesTab() {
  const { gameId } = useParams<{ gameId: string }>();
  const { getRules, assignRule, updateRule } = useRules();
  const { session } = useAuth();
  const { toast } = useToast();
  const [assigning, setAssigning] = useState<number | null>(null);
  const [editingRule, setEditingRule] = useState<{
    id: number;
    data: EditRuleData;
  } | null>(null);

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
        title: "Regola assegnata!",
        description: "Il tuo punteggio è stato aggiornato",
      });

      // Refetch to update UI
      await refetch();
    } catch (err) {
      if (err instanceof ConflictError) {
        toast({
          variant: "destructive",
          title: "Regola già assegnata",
          description: "Qualcun altro l'ha reclamata per primo!",
        });
        await refetch();
      } else {
        toast({
          variant: "destructive",
          title: "Assegnazione fallita",
          description:
            err instanceof Error ? err.message : "Si è verificato un errore",
        });
      }
    } finally {
      setAssigning(null);
    }
  };

  const handleEditRule = async (data: EditRuleData) => {
    if (!editingRule || !gameId) return;

    try {
      await updateRule(parseInt(gameId), editingRule.id, data);

      toast({
        title: "Regola modificata",
        description: "La regola è stata aggiornata con successo",
      });

      await refetch();
      setEditingRule(null);
    } catch (err) {
      if (err instanceof ConflictError) {
        toast({
          variant: "destructive",
          title: "Modifica non permessa",
          description:
            "Questa regola è già stata assegnata e non può essere modificata",
        });
        await refetch();
      } else {
        toast({
          variant: "destructive",
          title: "Modifica fallita",
          description:
            err instanceof Error ? err.message : "Si è verificato un errore",
        });
      }
    }
  };

  if (loading && !rules)
    return <LoadingState message="Caricamento regole..." />;
  if (error) return <ErrorState message={error.message} onRetry={refetch} />;
  if (!rules || rules.length === 0)
    return (
      <EmptyState
        title="Nessuna regola"
        message="Non ci sono regole definite per questa partita"
      />
    );

  const isCreator = session?.role === "creator";

  return (
    <>
      <div className="space-y-3">
        {rules.map(({ rule, assignment }) => {
          const isAssigned = !!assignment;
          const isAssignedToMe =
            assignment?.assignedToPlayerId === session?.playerId;
          const canAssign = !isAssigned && !assigning;
          const canEdit = isCreator && !isAssigned;

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
                      rule.RuleType === RuleType.Bonus
                        ? "default"
                        : "destructive"
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
                      Assegnata a{" "}
                      <strong>{assignment.assignedToUsername}</strong>
                    </span>
                    <Clock className="h-3 w-3 ml-2" />
                    <span>
                      {new Date(assignment.assignedAt).toLocaleString("it-IT")}
                    </span>
                  </div>
                )}
              </div>

              <div className="flex gap-2">
                {canEdit && (
                  <ActionButton
                    actionType="warning"
                    size="sm"
                    onClick={() =>
                      setEditingRule({
                        id: rule.Id,
                        data: {
                          name: rule.Name,
                          ruleType: rule.RuleType,
                          scoreDelta: rule.ScoreDelta,
                        },
                      })
                    }
                  >
                    <Pencil className="h-4 w-4" />
                  </ActionButton>
                )}

                <ActionButton
                  actionType={isAssignedToMe ? "info" : "success"}
                  onClick={() => handleAssign(rule.Id)}
                  disabled={!canAssign || assigning === rule.Id}
                  size="sm"
                >
                  {assigning === rule.Id
                    ? "Assegnando..."
                    : isAssignedToMe
                    ? "Assegnata a te"
                    : isAssigned
                    ? "Assegnata"
                    : "Assegna a me"}
                </ActionButton>
              </div>
            </div>
          );
        })}
      </div>

      <EditRuleDialog
        open={!!editingRule}
        onOpenChange={(open) => !open && setEditingRule(null)}
        onConfirm={handleEditRule}
        initialData={editingRule?.data}
      />
    </>
  );
}
