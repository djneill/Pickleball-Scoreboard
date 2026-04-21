import { useState, useEffect } from "react";
import { gameApi } from "../services/api";
import type { GameState } from "../services/api";
import ErrorMessage from "../components/ErrorMessage";
import LoadingSpinner from "../components/LoadingSpinner";
import GameTypeSelector from "../components/GameTypeSelector";
import TeamScore from "../components/TeamScore";
import GameInfo from "../components/GameInfo";
import MatchStats from "../components/MatchStats";
import LogoutButton from "../components/LogoutButton";

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
        await gameApi.clearStats();
        await loadCurrentGame();
      } catch (err) {
        setError("Failed to clear statistics");
        console.error("Error clearing stats:", err);
      }
    }
  };

  return (
    <div className="min-h-screen relative overflow-hidden bg-slate-950 flex flex-col">
      {/* Modern CSS Background: Deep Blue gradient with abstract glowing orbs */}
      <div className="absolute inset-0 z-0 bg-gradient-to-br from-blue-950 via-blue-900/80 to-slate-950" />
      <div className="absolute -top-40 -left-40 w-[30rem] h-[30rem] bg-cyan-600/30 rounded-full mix-blend-screen filter blur-[100px] opacity-60" />
      <div className="absolute top-1/4 -right-40 w-[30rem] h-[30rem] bg-blue-500/30 rounded-full mix-blend-screen filter blur-[100px] opacity-60" />
      <div className="absolute -bottom-40 left-1/3 w-[30rem] h-[30rem] bg-sky-600/30 rounded-full mix-blend-screen filter blur-[100px] opacity-60" />

      {/* Main Content */}
      <div className="relative z-10 w-full max-w-4xl mx-auto p-4 md:p-8 flex-1 flex flex-col justify-center">
        {!game && (
          <div className="text-center mb-10 flex flex-col items-center">
            <img 
              src="/8668927.png" 
              alt="Pickleball Logo" 
              className="w-48 md:w-64 mb-6 drop-shadow-2xl animate-fade-in-up"
            />
            <h1 className="text-4xl md:text-5xl font-extrabold text-transparent bg-clip-text bg-gradient-to-r from-white to-blue-100 tracking-tight drop-shadow-sm mb-2">
              Pickleball
            </h1>
            <h2 className="text-xl md:text-2xl font-light text-blue-200/80 tracking-wide uppercase letter-spacing-2">
              Scoreboard
            </h2>
          </div>
        )}

        <div className="w-full max-w-2xl mx-auto">
          <ErrorMessage error={error} onDismiss={() => setError(null)} />
          <LoadingSpinner isLoading={isLoading} />
        </div>

        {/* Game Type Selection */}
        {!game && !isLoading && (
          <div className="w-full max-w-md mx-auto space-y-6">
            <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-3xl p-6 md:p-8 shadow-2xl">
              <GameTypeSelector
                onSelectGameType={startNewGame}
                isLoading={isLoading}
              />
            </div>
            <div className="flex justify-center">
              <LogoutButton />
            </div>
          </div>
        )}

        {/* Active Game */}
        {game && (
          <div className="flex flex-col space-y-6 animate-fade-in-up">
            <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-3xl p-6 shadow-2xl">
              <GameInfo game={game} />
            </div>

            <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-3xl p-6 shadow-2xl">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 md:gap-12">
                <div className="transform transition-transform hover:scale-[1.02]">
                  <TeamScore
                    teamName="You"
                    score={game.homeScore}
                    color="text-cyan-300 drop-shadow-lg"
                    onScoreChange={(change) => updateScore("Home", change)}
                    disabled={game.isGameComplete || isLoading}
                    canDecrement={game.homeScore > 0}
                  />
                </div>
                <div className="transform transition-transform hover:scale-[1.02]">
                  <TeamScore
                    teamName="Opponent"
                    score={game.awayScore}
                    color="text-amber-400 drop-shadow-lg"
                    onScoreChange={(change) => updateScore("Away", change)}
                    disabled={game.isGameComplete || isLoading}
                    canDecrement={game.awayScore > 0}
                  />
                </div>
              </div>
            </div>

            <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-3xl p-6 shadow-2xl">
              <MatchStats game={game} onClearStats={clearAllStats} />
            </div>

            <div className="text-center pt-4 pb-8 space-y-6">
              <button
                onClick={() => setGame(null)}
                className="bg-gradient-to-r from-orange-500 to-rose-500 hover:from-orange-400 hover:to-rose-400 text-white font-bold py-4 px-10 rounded-full shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-1"
              >
                End / New Game
              </button>
              <div className="flex justify-center">
                <LogoutButton />
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
