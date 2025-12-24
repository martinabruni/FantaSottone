import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/providers/auth/AuthProvider";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/useToast";

export function LoginForm() {
  const [username, setUsername] = useState("");
  const [accessCode, setAccessCode] = useState("");
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!username || !accessCode) {
      toast({
        variant: "destructive",
        title: "Validation error",
        description: "Please fill in all fields",
      });
      return;
    }

    setLoading(true);

    try {
      const result = await login({ username, accessCode });

      if (result) {
        toast({
          title: "Login successful",
          description: `Welcome, ${result.player.Username}!`,
        });
        navigate(`/game/${result.game.Id}`);
      } else {
        toast({
          variant: "destructive",
          title: "Login failed",
          description: "Invalid credentials",
        });
      }
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Login failed",
        description:
          error instanceof Error ? error.message : "An error occurred",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Join Game</CardTitle>
        <CardDescription>
          Enter your credentials to join an existing game
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="username">Username</Label>
            <Input
              id="username"
              type="text"
              placeholder="Enter your username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={loading}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="accessCode">Access Code</Label>
            <Input
              id="accessCode"
              type="password"
              placeholder="Enter your access code"
              value={accessCode}
              onChange={(e) => setAccessCode(e.target.value)}
              disabled={loading}
            />
          </div>
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? "Logging in..." : "Login"}
          </Button>
          <div className="text-xs text-muted-foreground space-y-1">
            <p>Test credentials:</p>
            <p>Username: test1, Code: code1 (Creator)</p>
            <p>Username: test2, Code: code2 (Player)</p>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
