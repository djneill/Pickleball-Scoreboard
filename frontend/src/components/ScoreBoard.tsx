import { useState, useEffect } from "react";
import { gameApi } from "../services/api";
import type { GameState } from "../services/api";

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

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-400 via-blue-500 to-purple-600 p-4">
      <div className="max-2-md mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-white mb-2">🏓</h1>
          <h1 className="text-3xl font-bold text-white">Pickleball Score</h1>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-500 text-white p-3 rounded-lg mb-4 text-center">
            {error}
            <button
              onClick={() => setError(null)}
              className="ml-2 text-red-200 hover:text-white"
            >
              ✕
            </button>
          </div>
        )}

        {/* Loading State */}
        {isLoading && (
          <div className="text-center text-white mb-4">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-white"></div>
            <p className="mt-2">Loading...</p>
          </div>
        )}

        {/* Game Type Selection */}
        {!game && !isLoading && (
          <div className="bg-white/90 backdrop-blur-sm rounded-xl p-6 mb-6 shadow-lg">
            <h2 className="text-xl font-semibold text-gray-800 mb-4 text-center">
              Choose Game Type
            </h2>
            <div className="grid grid-cols-2 gap-4">
              <button
                onClick={() => startNewGame("Singles")}
                className="bg-blue-500 hover:bg-blue-600 text-white font-bold py-4 px-6 rounded-lg transition-all duration-200 transform hover:scale-105"
              >
                Singles
              </button>
              <button
                onClick={() => startNewGame("Doubles")}
                className="bg-purple-500 hover:bg-purple-600 text-white font-bold py-4 px-6 rounded-lg transition-all duration-200 transform hover:scale-105"
              >
                Doubles
              </button>
            </div>
          </div>
        )}

        {/* Active Game */}
        {game && (
          <>
            {/* Game Info */}
            <div className="bg-white/90 backdrop-blur-sm rounded-xl p-4 mb-6 shadow-lg">
              <div className="text-center">
                <h2 className="text-lg font-semibold text-gray-800">
                  {game.gameType} Game
                </h2>
                {game.isGameComplete && (
                  <div className="mt-2 text-green-600 font-bold">
                    🎉 Game Complete! 🎉
                  </div>
                )}
              </div>
            </div>

            {/* Scoreboard */}
            <div className="bg-white/90 backdrop-blur-sm rounded-xl p-6 mb-6 shadow-lg">
              <div className="grid grid-cols-2 gap-6">
                {/* Home Team */}
                <div className="text-center">
                  <h3 className="text-lg font-semibold text-gray-700 mb-2">
                    HOME
                  </h3>
                  <div className="text-6xl font-bold text-blue-600 mb-4">
                    {game.homeScore}
                  </div>
                  <div className="space-y-2">
                    <button
                      onClick={() => updateScore("Home", 1)}
                      disabled={game.isGameComplete || isLoading}
                      className="w-full bg-green-500 hover:bg-green-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold py-3 px-4 rounded-lg text-xl transition-all duration-200"
                    >
                      +
                    </button>
                    <button
                      onClick={() => updateScore("Home", -1)}
                      disabled={
                        game.isGameComplete || isLoading || game.homeScore === 0
                      }
                      className="w-full bg-red-500 hover:bg-red-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold py-3 px-4 rounded-lg text-xl transition-all duration-200"
                    >
                      -
                    </button>
                  </div>
                </div>

                {/* Away Team */}
                <div className="text-center">
                  <h3 className="text-lg font-semibold text-gray-700 mb-2">
                    AWAY
                  </h3>
                  <div className="text-6xl font-bold text-purple-600 mb-4">
                    {game.awayScore}
                  </div>
                  <div className="space-y-2">
                    <button
                      onClick={() => updateScore("Away", 1)}
                      disabled={game.isGameComplete || isLoading}
                      className="w-full bg-green-500 hover:bg-green-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold py-3 px-4 rounded-lg text-xl transition-all duration-200"
                    >
                      +
                    </button>
                    <button
                      onClick={() => updateScore("Away", -1)}
                      disabled={
                        game.isGameComplete || isLoading || game.awayScore === 0
                      }
                      className="w-full bg-red-500 hover:bg-red-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold py-3 px-4 rounded-lg text-xl transition-all duration-200"
                    >
                      -
                    </button>
                  </div>
                </div>
              </div>
            </div>

            {/* Game Statistics */}
            <div className="bg-white/90 backdrop-blur-sm rounded-xl p-6 mb-6 shadow-lg">
              <h3 className="text-lg font-semibold text-gray-800 mb-3 text-center">
                Match Stats
              </h3>
              <div className="grid grid-cols-2 gap-4 text-center">
                <div>
                  <div className="text-2xl font-bold text-blue-600">
                    {game.homeWins}
                  </div>
                  <div className="text-sm text-gray-600">Home Wins</div>
                </div>
                <div>
                  <div className="text-2xl font-bold text-purple-600">
                    {game.awayWins}
                  </div>
                  <div className="text-sm text-gray-600">Away Wins</div>
                </div>
              </div>
            </div>

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
