import { GoogleOAuthProvider } from "@react-oauth/google";
import { AuthProvider } from "./auth/AuthProvider";
import { UsersProvider } from "./users/UsersProvider";
import { GamesProvider } from "./games/GamesProvider";
import { GameProvider } from "./games/GameProvider";
import { LeaderboardProvider } from "./leaderboard/LeaderboardProvider";
import { RulesProvider } from "./rules/RulesProvider";
import { AssignmentsProvider } from "./assignments/AssignmentsProvider";

export function AppProviders({ children }: { children: React.ReactNode }) {
  const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID || "";

  if (!googleClientId) {
    console.warn(
      "VITE_GOOGLE_CLIENT_ID not configured. Google OAuth will not work."
    );
  }

  return (
    <GoogleOAuthProvider clientId={googleClientId}>
      <AuthProvider>
        <UsersProvider>
          <GamesProvider>
            <GameProvider>
              <LeaderboardProvider>
                <RulesProvider>
                  <AssignmentsProvider>{children}</AssignmentsProvider>
                </RulesProvider>
              </LeaderboardProvider>
            </GameProvider>
          </GamesProvider>
        </UsersProvider>
      </AuthProvider>
    </GoogleOAuthProvider>
  );
}
