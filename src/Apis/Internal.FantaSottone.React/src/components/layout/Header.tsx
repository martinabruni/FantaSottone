import { ThemeToggle } from "./ThemeToggle";
import { Separator } from "@/components/ui/separator";
import { LogOut, User } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";

export function Header() {
  const appName = import.meta.env.VITE_APP_NAME || "FantaSottone";
  const { session, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate("/");
  };

  const handleProfileClick = () => {
    navigate("/profile");
  };

  const handleTitleClick = () => {
    if (isAuthenticated) {
      navigate("/games");
    } else {
      navigate("/");
    }
  };

  return (
    <header className="border-b">
      <div className="container mx-auto px-4 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <h1
              className="text-2xl font-bold cursor-pointer hover:text-primary transition-colors"
              onClick={handleTitleClick}
            >
              {appName}
            </h1>
            {isAuthenticated && session && (
              <>
                <Separator orientation="vertical" className="h-6" />
                <span className="text-sm text-muted-foreground">
                  {session.username}
                  {session.role === "creator" && " (Creator)"}
                </span>
              </>
            )}
          </div>
          <div className="flex items-center space-x-2">
            {isAuthenticated && (
              <>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={handleProfileClick}
                  aria-label="Profilo"
                  title="Il tuo profilo"
                >
                  <User className="h-5 w-5" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={handleLogout}
                  aria-label="Logout"
                  title="Esci"
                >
                  <LogOut className="h-5 w-5" />
                </Button>
              </>
            )}
            <ThemeToggle />
          </div>
        </div>
      </div>
    </header>
  );
}
