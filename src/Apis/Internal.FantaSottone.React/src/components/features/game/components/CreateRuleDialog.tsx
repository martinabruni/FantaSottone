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

interface CreateRuleDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (data: CreateRuleData) => Promise<void>;
}

export interface CreateRuleData {
  name: string;
  ruleType: RuleType;
  scoreDelta: number;
}

export function CreateRuleDialog({
  open,
  onOpenChange,
  onConfirm,
}: CreateRuleDialogProps) {
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<CreateRuleData>({
    name: "",
    ruleType: RuleType.Bonus,
    scoreDelta: 10,
  });

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
      
      // Reset form
      setFormData({
        name: "",
        ruleType: RuleType.Bonus,
        scoreDelta: 10,
      });
      
      onOpenChange(false);
    } catch (error) {
      console.error("Failed to create rule:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent variant="success">
        <DialogHeader>
          <DialogTitle>Crea nuova regola</DialogTitle>
          <DialogDescription>
            Aggiungi una nuova regola bonus o malus alla partita in corso.
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
            actionType="success"
            onClick={handleConfirm}
            disabled={loading || !formData.name.trim()}
          >
            {loading ? "Creando..." : "Crea regola"}
          </ActionButton>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
