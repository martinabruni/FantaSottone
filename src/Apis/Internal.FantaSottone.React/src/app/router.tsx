import { createBrowserRouter, RouterProvider, Outlet } from "react-router-dom";
import { LandingPage } from "@/components/features/auth/pages/LandingPage";
import { GamePage } from "@/components/features/game/pages/GamePage";
import { AppShell } from "@/components/layout/AppShell";

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
        path: "/game/:gameId",
        element: <GamePage />,
      },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
