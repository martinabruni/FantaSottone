import { useState } from "react";
import { LoginForm } from "../components/LoginForm";
import { CreateGameForm } from "../components/CreateGameForm";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Users, PlusCircle } from "lucide-react";

type ViewMode = "initial" | "join" | "create";

export function LandingPage() {
  const [viewMode, setViewMode] = useState<ViewMode>("initial");

  if (viewMode === "join") {
    return (
      <div className="w-full max-w-md mx-auto px-4">
        <Button 
          variant="ghost" 
          onClick={() => setViewMode("initial")}
          className="mb-4"
        >
          ← Indietro
        </Button>
        <LoginForm />
      </div>
    );
  }

  if (viewMode === "create") {
    return (
      <div className="w-full max-w-2xl mx-auto px-4">
        <Button 
          variant="ghost" 
          onClick={() => setViewMode("initial")}
          className="mb-4"
        >
          ← Indietro
        </Button>
        <CreateGameForm />
      </div>
    );
  }

  return (
    <div className="w-full max-w-2xl mx-auto px-4">
      <Card className="border-2">
        <CardHeader className="text-center pb-4">
          <CardTitle className="text-3xl sm:text-4xl font-bold">Benvenuto in FantaSottone</CardTitle>
          <CardDescription className="text-base sm:text-lg">
            Scegli un'opzione per iniziare
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4 pb-8">
          <Button
            onClick={() => setViewMode("join")}
            className="w-full h-20 sm:h-24 text-base sm:text-lg flex flex-col gap-2 bg-blue-600 hover:bg-blue-700"
            size="lg"
          >
            <Users className="h-6 w-6 sm:h-8 sm:w-8" />
            <span>Unisciti ad una partita</span>
          </Button>
          
          <Button
            onClick={() => setViewMode("create")}
            className="w-full h-20 sm:h-24 text-base sm:text-lg flex flex-col gap-2 bg-green-600 hover:bg-green-700"
            size="lg"
          >
            <PlusCircle className="h-6 w-6 sm:h-8 sm:w-8" />
            <span>Crea una nuova partita</span>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
