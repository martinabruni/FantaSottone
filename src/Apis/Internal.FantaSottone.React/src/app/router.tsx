import { createBrowserRouter, RouterProvider, Outlet } from "react-router-dom";
import { LandingPage } from "@/components/features/auth/pages/LandingPage";
import { GameListPage } from "@/components/features/game/pages/GameListPage";
import { CreateGamePage } from "@/components/features/game/pages/CreateGamePage";
import { GamePage } from "@/components/features/game/pages/GamePage";
import { ProfilePage } from "@/components/features/user/pages/ProfilePage";
import { AppShell } from "@/components/layout/AppShell";
import { ProtectedRoute } from "@/components/features/auth/components/ProtectedRoute";

function Layout() {
  return (
    <AppShell>
      <Outlet />
    </AppShell>
  );
}

const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      {
        path: "/",
        element: <LandingPage />,
      },
      {
        path: "/games",
        element: (
          <ProtectedRoute>
            <GameListPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/games/create",
        element: (
          <ProtectedRoute>
            <CreateGamePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/game/:gameId",
        element: (
          <ProtectedRoute>
            <GamePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/profile",
        element: (
          <ProtectedRoute>
            <ProfilePage />
          </ProtectedRoute>
        ),
      },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
