import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ActionButton } from "@/components/common/ActionButton";
import { Button } from "@/components/ui/button";

interface EndGameDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => Promise<void>;
}

export function EndGameDialog({
  open,
  onOpenChange,
  onConfirm,
}: EndGameDialogProps) {
  const [loading, setLoading] = useState(false);

  const handleConfirm = async () => {
    setLoading(true);
    try {
      await onConfirm();
      onOpenChange(false);
    } catch (error) {
      console.error("Failed to end game:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent variant="error">
        <DialogHeader>
          <DialogTitle>Terminare la partita?</DialogTitle>
          <DialogDescription>
            Questa azione è irreversibile. Il vincitore sarà decretato in base
            al punteggio attuale più alto. In caso di parità, vince il giocatore
            con l'ID più basso.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={loading}
          >
            Annulla
          </Button>
          <ActionButton
            actionType="error"
            onClick={handleConfirm}
            disabled={loading}
          >
            {loading ? "Terminando..." : "Termina partita"}
          </ActionButton>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
