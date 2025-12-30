import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/hooks/useToast";
import { X, Plus, ArrowLeft } from "lucide-react";
import { CreateGameRequest, CreateGameResponse } from "@/types/user-types";
import { createTransport } from "@/lib/http/transportFactory";

export function CreateGamePage() {
  const [gameName, setGameName] = useState("");
  const [initialScore, setInitialScore] = useState("100");
  const [emailInput, setEmailInput] = useState("");
  const [invitedEmails, setInvitedEmails] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();
  const transport = createTransport();

  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleAddEmail = () => {
    const trimmedEmail = emailInput.trim().toLowerCase();

    if (!trimmedEmail) {
      return;
    }

    if (!isValidEmail(trimmedEmail)) {
      toast({
        variant: "error",
        title: "Email non valida",
        description: "Inserisci un indirizzo email valido",
      });
      return;
    }

    if (invitedEmails.includes(trimmedEmail)) {
      toast({
        variant: "error",
        title: "Email già aggiunta",
        description: "Questa email è già stata aggiunta alla lista",
      });
      return;
    }

    setInvitedEmails([...invitedEmails, trimmedEmail]);
    setEmailInput("");
  };

  const handleRemoveEmail = (email: string) => {
    setInvitedEmails(invitedEmails.filter((e) => e !== email));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleAddEmail();
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!gameName.trim()) {
      toast({
        variant: "error",
        title: "Nome partita richiesto",
        description: "Inserisci un nome per la partita",
      });
      return;
    }

    const score = parseInt(initialScore);
    if (isNaN(score) || score <= 0) {
      toast({
        variant: "error",
        title: "Punteggio non valido",
        description: "Inserisci un punteggio iniziale valido",
      });
      return;
    }

    setLoading(true);

    try {
      const request: CreateGameRequest = {
        name: gameName.trim(),
        initialScore: score,
        invitedEmails: invitedEmails,
      };

      const response = await transport.post<
        CreateGameRequest,
        CreateGameResponse
      >("/api/Games/create", request);

      let successMessage = `Partita "${response.gameName}" creata con successo!`;
      if (response.invitedEmails.length > 0) {
        successMessage += ` ${response.invitedEmails.length} giocatori invitati.`;
      }
      if (response.invalidEmails.length > 0) {
        successMessage += ` ${response.invalidEmails.length} email non trovate.`;
      }

      toast({
        variant: "success",
        title: "Partita creata!",
        description: successMessage,
      });

      // Reindirizza alla partita appena creata
      navigate(`/game/${response.gameId}`);
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore nella creazione",
        description:
          error instanceof Error
            ? error.message
            : "Si è verificato un errore imprevisto",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-2xl">
      <Button
        variant="ghost"
        onClick={() => navigate("/games")}
        className="mb-4"
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Torna alle partite
      </Button>

      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">Crea nuova partita</CardTitle>
          <CardDescription>
            Crea una partita e invita altri giocatori tramite email
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="gameName">Nome partita *</Label>
              <Input
                id="gameName"
                type="text"
                placeholder="Es: Partita del weekend"
                value={gameName}
                onChange={(e) => setGameName(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="initialScore">Punteggio iniziale *</Label>
              <Input
                id="initialScore"
                type="number"
                min="1"
                placeholder="100"
                value={initialScore}
                onChange={(e) => setInitialScore(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="emailInput">Invita giocatori (opzionale)</Label>
              <div className="flex gap-2">
                <Input
                  id="emailInput"
                  type="email"
                  placeholder="email@esempio.com"
                  value={emailInput}
                  onChange={(e) => setEmailInput(e.target.value)}
                  onKeyPress={handleKeyPress}
                  disabled={loading}
                />
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleAddEmail}
                  disabled={loading || !emailInput.trim()}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
              <p className="text-sm text-muted-foreground">
                Premi Invio o clicca + per aggiungere un'email
              </p>
            </div>

            {invitedEmails.length > 0 && (
              <div className="space-y-2">
                <Label>Email invitate ({invitedEmails.length})</Label>
                <div className="flex flex-wrap gap-2">
                  {invitedEmails.map((email) => (
                    <Badge key={email} variant="secondary" className="text-sm">
                      {email}
                      <button
                        type="button"
                        onClick={() => handleRemoveEmail(email)}
                        className="ml-2 hover:text-destructive"
                        disabled={loading}
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            <div className="flex gap-2 pt-4">
              <Button type="submit" disabled={loading} className="flex-1">
                {loading ? "Creazione..." : "Crea partita"}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate("/games")}
                disabled={loading}
              >
                Annulla
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
