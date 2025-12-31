import React, { createContext, useContext, useState, useEffect } from "react";
import { useAuth } from "../auth/AuthProvider";
import { useGames } from "../games/GamesProvider";

interface PlayerInfo {
  playerId: number;
  gameId: number;
  isCreator: boolean;
  currentScore: number;
}

interface GameContextValue {
  currentPlayer: PlayerInfo | null;
  joinGame: (gameId: number) => Promise<void>;
  clearCurrentPlayer: () => void;
}

const GameContext = createContext<GameContextValue | undefined>(undefined);

const PLAYER_INFO_KEY = "fantaSottone_currentPlayer";

export function GameProvider({ children }: { children: React.ReactNode }) {
  const [currentPlayer, setCurrentPlayer] = useState<PlayerInfo | null>(() => {
    // Load from localStorage on mount
    const stored = localStorage.getItem(PLAYER_INFO_KEY);
    if (stored) {
      try {
        return JSON.parse(stored);
      } catch {
        return null;
      }
    }
    return null;
  });

  const { isAuthenticated } = useAuth();
  const gamesProvider = useGames();

  // Clear player info on logout
  useEffect(() => {
    if (!isAuthenticated) {
      clearCurrentPlayer();
    }
  }, [isAuthenticated]);

  const joinGame = async (gameId: number) => {
    const response = await gamesProvider.joinGame(gameId);

    const playerInfo: PlayerInfo = {
      playerId: response.player.id,
      gameId: response.game.id,
      isCreator: response.player.isCreator,
      currentScore: response.player.currentScore,
    };

    setCurrentPlayer(playerInfo);
    localStorage.setItem(PLAYER_INFO_KEY, JSON.stringify(playerInfo));
  };

  const clearCurrentPlayer = () => {
    setCurrentPlayer(null);
    localStorage.removeItem(PLAYER_INFO_KEY);
  };

  const value: GameContextValue = {
    currentPlayer,
    joinGame,
    clearCurrentPlayer,
  };

  return <GameContext.Provider value={value}>{children}</GameContext.Provider>;
}

export function useGame() {
  const context = useContext(GameContext);
  if (!context) {
    throw new Error("useGame must be used within GameProvider");
  }
  return context;
}
