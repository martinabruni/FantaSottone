import { useState, useEffect } from "react";
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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { RuleType } from "@/types/entities";

interface EditRuleDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (data: EditRuleData) => Promise<void>;
  initialData?: EditRuleData;
}

export interface EditRuleData {
  name: string;
  ruleType: RuleType;
  scoreDelta: number;
}

export function EditRuleDialog({
  open,
  onOpenChange,
  onConfirm,
  initialData,
}: EditRuleDialogProps) {
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<EditRuleData>(
    initialData || {
      name: "",
      ruleType: RuleType.Bonus,
      scoreDelta: 0,
    }
  );

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  const handleConfirm = async () => {
    if (!formData.name.trim()) {
      return;
    }

    setLoading(true);
    try {
      // Normalize scoreDelta based on ruleType
      const normalizedData = {
        ...formData,
        scoreDelta:
          formData.ruleType === RuleType.Bonus
            ? Math.abs(formData.scoreDelta)
            : -Math.abs(formData.scoreDelta),
      };

      await onConfirm(normalizedData);
      onOpenChange(false);
    } catch (error) {
      console.error("Failed to update rule:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Modifica regola</DialogTitle>
          <DialogDescription>
            Modifica i dettagli della regola. Solo le regole non ancora
            assegnate possono essere modificate.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="name">Nome</Label>
            <Input
              id="name"
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
              placeholder="Es. Prima birra, Ultimo arrivato..."
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="ruleType">Tipo</Label>
            <Select
              value={formData.ruleType.toString()}
              onValueChange={(value: string) =>
                setFormData({
                  ...formData,
                  ruleType: parseInt(value) as RuleType,
                })
              }
            >
              <SelectTrigger id="ruleType">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={RuleType.Bonus.toString()}>Bonus</SelectItem>
                <SelectItem value={RuleType.Malus.toString()}>Malus</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="scoreDelta">Punti</Label>
            <Input
              id="scoreDelta"
              type="number"
              value={Math.abs(formData.scoreDelta)}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  scoreDelta: parseInt(e.target.value) || 0,
                })
              }
              placeholder="Es. 10"
            />
            <p className="text-sm text-muted-foreground">
              {formData.ruleType === RuleType.Bonus
                ? "Punti da aggiungere"
                : "Punti da sottrarre"}
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={loading}
          >
            Annulla
          </Button>
          <ActionButton
            actionType="warning"
            onClick={handleConfirm}
            disabled={loading || !formData.name.trim()}
          >
            {loading ? "Salvando..." : "Salva modifiche"}
          </ActionButton>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
