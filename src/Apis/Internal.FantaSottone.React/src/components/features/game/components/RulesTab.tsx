import { useParams } from "react-router-dom";
import { useRules } from "@/providers/rules/RulesProvider";
import { usePolling } from "@/hooks/usePolling";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useGame } from "@/providers/games/GameProvider";
import { LoadingState } from "@/components/common/LoadingState";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import { ActionButton } from "@/components/common/ActionButton";
import { Badge } from "@/components/ui/badge";
import { RuleType, GameStatus } from "@/types/entities";
import { Clock, CheckCircle2, Pencil, Trash2, Plus } from "lucide-react";
import { useToast } from "@/hooks/useToast";
import { useState } from "react";
import { ConflictError } from "@/lib/http/errors";
import { EditRuleDialog, EditRuleData } from "./EditRuleDialog";
import { CreateRuleDialog, CreateRuleData } from "./CreateRuleDialog";

interface RulesTabProps {
  gameStatus?: GameStatus | null;
}

export function RulesTab({ gameStatus }: RulesTabProps) {
  const { gameId } = useParams<{ gameId: string }>();
  const { getRules, assignRule, updateRule, createRule, deleteRule } =
    useRules();
  const { session } = useAuth();
  const { currentPlayer } = useGame();
  const { toast } = useToast();
  const [assigning, setAssigning] = useState<number | null>(null);
  const [editingRule, setEditingRule] = useState<{
    id: number;
    data: EditRuleData;
  } | null>(null);
  const [creatingRule, setCreatingRule] = useState(false);
  const [deletingRule, setDeletingRule] = useState<number | null>(null);

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
      await assignRule(parseInt(gameId), ruleId);

      toast({
        variant: "success",
        title: "Regola assegnata!",
        description: "Il tuo punteggio è stato aggiornato",
      });

      await refetch();
    } catch (err) {
      if (err instanceof ConflictError) {
        toast({
          variant: "error",
          title: "Regola già assegnata",
          description: "Qualcun altro l'ha reclamata per primo!",
        });
        await refetch();
      } else {
        toast({
          variant: "error",
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
        variant: "success",
        title: "Regola modificata",
        description: "La regola è stata aggiornata con successo",
      });

      await refetch();
      setEditingRule(null);
    } catch (err) {
      if (err instanceof ConflictError) {
        toast({
          variant: "error",
          title: "Modifica non permessa",
          description:
            "Questa regola è già stata assegnata e non può essere modificata",
        });
        await refetch();
      } else {
        toast({
          variant: "error",
          title: "Modifica fallita",
          description:
            err instanceof Error ? err.message : "Si è verificato un errore",
        });
      }
    }
  };

  const handleCreateRule = async (data: CreateRuleData) => {
    if (!gameId) return;

    try {
      await createRule(parseInt(gameId), data);

      toast({
        variant: "success",
        title: "Regola creata",
        description: "La nuova regola è stata aggiunta alla partita",
      });

      await refetch();
      setCreatingRule(false);
    } catch (err) {
      toast({
        variant: "error",
        title: "Creazione fallita",
        description:
          err instanceof Error ? err.message : "Si è verificato un errore",
      });
    }
  };

  const handleDeleteRule = async (ruleId: number) => {
    if (!gameId) return;

    setDeletingRule(ruleId);

    try {
      await deleteRule(parseInt(gameId), ruleId);

      toast({
        variant: "success",
        title: "Regola eliminata",
        description: "La regola è stata rimossa dalla partita",
      });

      await refetch();
    } catch (err) {
      if (err instanceof ConflictError) {
        toast({
          variant: "error",
          title: "Eliminazione non permessa",
          description:
            "Questa regola è già stata assegnata e non può essere eliminata",
        });
        await refetch();
      } else {
        toast({
          variant: "error",
          title: "Eliminazione fallita",
          description:
            err instanceof Error ? err.message : "Si è verificato un errore",
        });
      }
    } finally {
      setDeletingRule(null);
    }
  };

  if (loading && !rules)
    return <LoadingState message="Caricamento regole..." />;
  if (error) return <ErrorState message={error.message} onRetry={refetch} />;

  // ✅ FIXED: Use currentPlayer.isCreator instead of session.role
  const isCreator = currentPlayer?.isCreator ?? false;
  const isGameEnded = gameStatus === GameStatus.Ended;

  return (
    <>
      <div className="space-y-4">
        {/* ✅ Creator can create rules even during game */}
        {isCreator && (
          <div className="flex justify-end">
            <ActionButton
              actionType="success"
              size="sm"
              onClick={() => setCreatingRule(true)}
            >
              <Plus className="h-4 w-4 mr-1" />
              Crea nuova regola
            </ActionButton>
          </div>
        )}

        {!rules || rules.length === 0 ? (
          <EmptyState
            title="Nessuna regola"
            message="Non ci sono regole definite per questa partita"
          />
        ) : (
          rules.map(({ rule, assignment }) => {
            const isAssigned = !!assignment;
            const isAssignedToMe =
              assignment?.assignedToPlayerId === currentPlayer?.playerId;
            const canAssign = !isAssigned && !assigning;
            // ✅ Unassigned rules can be edited/deleted even during game
            const canEdit = isCreator && !isAssigned && !isGameEnded;
            const canDelete = isCreator && !isAssigned && !isGameEnded;

            return (
              <div
                key={rule.id}
                className={`flex items-center justify-between p-4 rounded-lg border ${
                  isAssignedToMe
                    ? "bg-blue-50 border-blue-400 dark:bg-blue-950/40 dark:border-blue-800"
                    : "bg-card"
                }`}
              >
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="font-semibold">{rule.name}</span>
                    <Badge
                      variant={
                        rule.ruleType === RuleType.Bonus
                          ? "default"
                          : "destructive"
                      }
                    >
                      {rule.ruleType === RuleType.Bonus ? "Bonus" : "Malus"}{" "}
                      {rule.scoreDelta > 0 ? "+" : ""}
                      {rule.scoreDelta}
                    </Badge>
                  </div>

                  {isAssigned ? (
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <CheckCircle2 className="h-4 w-4 text-green-600" />
                      <span>Assegnata a: {assignment.assignedAt}</span>
                    </div>
                  ) : (
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Clock className="h-4 w-4" />
                      <span>In attesa di assegnazione</span>
                    </div>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  {canEdit && (
                    <ActionButton
                      actionType="warning"
                      size="sm"
                      onClick={() =>
                        setEditingRule({
                          id: rule.id,
                          data: {
                            name: rule.name,
                            ruleType: rule.ruleType,
                            scoreDelta: rule.scoreDelta,
                          },
                        })
                      }
                    >
                      <Pencil className="h-4 w-4" />
                    </ActionButton>
                  )}

                  {canDelete && (
                    <ActionButton
                      actionType="error"
                      size="sm"
                      onClick={() => handleDeleteRule(rule.id)}
                      disabled={deletingRule === rule.id}
                    >
                      <Trash2 className="h-4 w-4" />
                    </ActionButton>
                  )}

                  <ActionButton
                    onClick={() => handleAssign(rule.id)}
                    disabled={!canAssign || assigning === rule.id}
                  >
                    {assigning === rule.id
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
          })
        )}
      </div>

      <EditRuleDialog
        open={!!editingRule}
        onOpenChange={(open) => !open && setEditingRule(null)}
        onConfirm={handleEditRule}
        initialData={editingRule?.data}
      />

      <CreateRuleDialog
        open={creatingRule}
        onOpenChange={setCreatingRule}
        onConfirm={handleCreateRule}
      />
    </>
  );
}
