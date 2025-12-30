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
import { useGames } from "@/providers/games/GamesProvider";
import { useGame } from "@/providers/games/GameProvider";
import { useAuth } from "@/providers/auth/AuthProvider";

export function CreateGamePage() {
  const [gameName, setGameName] = useState("");
  const [initialScore, setInitialScore] = useState("100");
  const [emailInput, setEmailInput] = useState("");
  const [invitedEmails, setInvitedEmails] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();
  const { createGame } = useGames();
  const { joinGame } = useGame();
  const { session } = useAuth();

  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleAddEmail = () => {
    const trimmedEmail = emailInput.trim();

    if (!trimmedEmail) {
      toast({
        variant: "error",
        title: "Email vuota",
        description: "Inserisci un indirizzo email valido",
      });
      return;
    }

    if (!isValidEmail(trimmedEmail)) {
      toast({
        variant: "error",
        title: "Email non valida",
        description: "Inserisci un indirizzo email nel formato corretto",
      });
      return;
    }

    // Don't allow inviting yourself
    if (
      session?.email &&
      trimmedEmail.toLowerCase() === session.email.toLowerCase()
    ) {
      toast({
        variant: "error",
        title: "Email non valida",
        description: "Non puoi invitare te stesso",
      });
      return;
    }

    if (invitedEmails.includes(trimmedEmail)) {
      toast({
        variant: "error",
        title: "Email duplicata",
        description: "Questa email è già stata aggiunta",
      });
      return;
    }

    setInvitedEmails([...invitedEmails, trimmedEmail]);
    setEmailInput("");
  };

  const handleRemoveEmail = (email: string) => {
    setInvitedEmails(invitedEmails.filter((e) => e !== email));
  };

  const handleCreateGame = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!gameName.trim()) {
      toast({
        variant: "error",
        title: "Nome mancante",
        description: "Inserisci un nome per la partita",
      });
      return;
    }

    const score = parseInt(initialScore);
    if (isNaN(score) || score <= 0) {
      toast({
        variant: "error",
        title: "Punteggio non valido",
        description: "Il punteggio iniziale deve essere un numero positivo",
      });
      return;
    }

    try {
      setLoading(true);

      // ✅ FIXED: Use correct response structure
      const response = await createGame({
        name: gameName,
        initialScore: score,
        invitedEmails: invitedEmails,
      });

      // ✅ FIXED: Join the game automatically to get player role
      await joinGame(response.gameId);

      toast({
        variant: "success",
        title: "Partita creata!",
        description: `La partita "${response.gameName}" è stata creata con successo`,
      });

      if (response.invalidEmails.length > 0) {
        toast({
          variant: "warning",
          title: "Alcuni inviti non sono andati a buon fine",
          description: `Le seguenti email non sono state trovate: ${response.invalidEmails.join(
            ", "
          )}`,
        });
      }

      // Navigate to the game page
      navigate(`/game/${response.gameId}`);
    } catch (error) {
      console.error("Error creating game:", error);
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
        className="mb-4"
        onClick={() => navigate("/games")}
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Torna alle partite
      </Button>

      <Card>
        <CardHeader>
          <CardTitle>Crea una nuova partita</CardTitle>
          <CardDescription>
            Configura la partita e invita altri giocatori via email
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleCreateGame} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="gameName">Nome partita</Label>
              <Input
                id="gameName"
                type="text"
                placeholder="La mia partita"
                value={gameName}
                onChange={(e) => setGameName(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="initialScore">Punteggio iniziale</Label>
              <Input
                id="initialScore"
                type="number"
                placeholder="100"
                value={initialScore}
                onChange={(e) => setInitialScore(e.target.value)}
                disabled={loading}
                min="1"
              />
            </div>

            <div className="space-y-2">
              <Label>Invita giocatori (opzionale)</Label>
              <div className="flex gap-2">
                <Input
                  type="email"
                  placeholder="email@esempio.com"
                  value={emailInput}
                  onChange={(e) => setEmailInput(e.target.value)}
                  onKeyPress={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      handleAddEmail();
                    }
                  }}
                  disabled={loading}
                />
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleAddEmail}
                  disabled={loading}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
              {invitedEmails.length > 0 && (
                <div className="flex flex-wrap gap-2 mt-2">
                  {invitedEmails.map((email) => (
                    <Badge key={email} variant="secondary" className="gap-1">
                      {email}
                      <button
                        type="button"
                        onClick={() => handleRemoveEmail(email)}
                        disabled={loading}
                        className="ml-1 hover:text-destructive"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </Badge>
                  ))}
                </div>
              )}
            </div>

            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Creazione in corso..." : "Crea partita"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
