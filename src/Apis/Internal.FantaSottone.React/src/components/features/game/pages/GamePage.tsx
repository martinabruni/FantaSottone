import { useParams, Navigate } from "react-router-dom";
import { useAuth } from "@/providers/auth/AuthProvider";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { GameStatusBar } from "../components/GameStatusBar";
import { LeaderboardTab } from "../components/LeaderboardTab";
import { RulesTab } from "../components/RulesTab";

export function GamePage() {
  const { gameId } = useParams<{ gameId: string }>();
  const { session, isAuthenticated } = useAuth();

  if (!isAuthenticated || !session) {
    return <Navigate to="/" replace />;
  }

  if (!gameId) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <GameStatusBar />

      <Tabs defaultValue="leaderboard" className="w-full">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="leaderboard">Leaderboard</TabsTrigger>
          <TabsTrigger value="rules">Rules</TabsTrigger>
        </TabsList>

        <TabsContent value="leaderboard" className="mt-6">
          <LeaderboardTab />
        </TabsContent>

        <TabsContent value="rules" className="mt-6">
          <RulesTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}
