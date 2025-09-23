import { useState, useEffect } from "react";
import { gameApi } from "../services/api";
import type { GameState } from "../services/api";
import ErrorMessage from "../components/ErrorMessage";
import LoadingSpinner from "../components/LoadingSpinner";
import GameTypeSelector from "../components/GameTypeSelector";
import TeamScore from "../components/TeamScore";
import GameInfo from "../components/GameInfo";
import MatchStats from "../components/MatchStats";

export default function PickleballScoreboard() {
  const [game, setGame] = useState<GameState | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCurrentGame();
  }, []);

  const loadCurrentGame = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const currentGame = await gameApi.getCurrentGame();
      setGame(currentGame);
    } catch (err) {
      setError("Failed to load game");
      console.error("Error loading game:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const startNewGame = async (gameType: "Singles" | "Doubles") => {
    try {
      setIsLoading(true);
      setError(null);
      const newGame = await gameApi.startNewGame(gameType);
      setGame(newGame);
    } catch (err) {
      setError("Failed to start new game");
      console.error("Error starting game:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const updateScore = async (team: "Home" | "Away", change: 1 | -1) => {
    if (!game || game.isGameComplete) return;

    try {
      setError(null);
      const updatedGame = await gameApi.updateScore(team, change);
      setGame(updatedGame);
    } catch (err) {
      setError(`Failed to update ${team.toLowerCase()} score`);
      console.error("Error updating score:", err);
    }
  };

  const clearAllStats = async () => {
    if (
      window.confirm("Are you sure you want to clear all match statistics?")
    ) {
      try {
        setError(null);
        await gameApi.clearStats(); // Use new endpoint
        // Reload current game to get updated stats
        await loadCurrentGame();
      } catch (err) {
        setError("Failed to clear statistics");
        console.error("Error clearing stats:", err);
      }
    }
  };

  const backgroundStyle = {
    backgroundImage: game
      ? 'url("/PBcourtBackground.png")'
      : 'url("/pickle-ball-landing.png")',
    backgroundSize: "cover",
    backgroundPosition: "center",
    backgroundRepeat: "no-repeat",
  };

  return (
    <div
      className="min-h-screen bg-gradient-to-br from-green-400 via-blue-500 to-purple-600 p-4"
      style={backgroundStyle}
    >
      <div className="max-w-md mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-white">
            Pickleball Scoreboard
          </h1>
        </div>

        <ErrorMessage error={error} onDismiss={() => setError(null)} />
        <LoadingSpinner isLoading={isLoading} />

        {/* Game Type Selection */}
        {!game && !isLoading && (
          <GameTypeSelector
            onSelectGameType={startNewGame}
            isLoading={isLoading}
          />
        )}

        {/* Active Game */}
        {game && (
          <>
            <GameInfo game={game} />

            {/* Scoreboard */}
            <div className="bg-white/50 backdrop-blur-sm rounded-xl p-2 mb-6 shadow-lg">
              <div className="grid grid-cols-2 gap-6">
                <TeamScore
                  teamName="Home"
                  score={game.homeScore}
                  color="text-blue-600"
                  onScoreChange={(change) => updateScore("Home", change)}
                  disabled={game.isGameComplete || isLoading}
                  canDecrement={game.homeScore > 0}
                />
                <TeamScore
                  teamName="Away"
                  score={game.awayScore}
                  color="text-purple-600"
                  onScoreChange={(change) => updateScore("Away", change)}
                  disabled={game.isGameComplete || isLoading}
                  canDecrement={game.awayScore > 0}
                />
              </div>
            </div>

            <MatchStats game={game} onClearStats={clearAllStats} />

            {/* New Game Button */}
            <div className="text-center">
              <button
                onClick={() => setGame(null)}
                className="bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 px-8 rounded-lg transition-all duration-200 transform hover:scale-105"
              >
                New Game
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
