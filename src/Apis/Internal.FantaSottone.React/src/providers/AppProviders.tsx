import { AuthProvider } from "./auth/AuthProvider";
import { GamesProvider } from "./games/GamesProvider";
import { LeaderboardProvider } from "./leaderboard/LeaderboardProvider";
import { RulesProvider } from "./rules/RulesProvider";
import { AssignmentsProvider } from "./assignments/AssignmentsProvider";

export function AppProviders({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <GamesProvider>
        <LeaderboardProvider>
          <RulesProvider>
            <AssignmentsProvider>{children}</AssignmentsProvider>
          </RulesProvider>
        </LeaderboardProvider>
      </GamesProvider>
    </AuthProvider>
  );
}
