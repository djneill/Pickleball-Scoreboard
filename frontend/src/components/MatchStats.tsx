interface MatchStatsProps {
  game: {
    homeWins: number;
    awayWins: number;
  };
  onClearStats: () => void;
}

export default function MatchStats({ game, onClearStats }: MatchStatsProps) {
  return (
    <div className="bg-white/50 backdrop-blur-sm rounded-xl p-6 mb-6 shadow-lg">
      <h3 className="text-lg font-semibold text-gray-800 mb-3 text-center">
        Match Stats
      </h3>
      <div className="grid grid-cols-2 gap-4 text-center mb-4">
        <div>
          <div className="text-2xl font-bold text-blue-600">
            {game.homeWins}
          </div>
          <div className="text-sm text-gray-600">Your Wins</div>
        </div>
        <div>
          <div className="text-2xl font-bold text-purple-600">
            {game.awayWins}
          </div>
          <div className="text-sm text-gray-600">Opponent Wins</div>
        </div>
      </div>
      {(game.homeWins > 0 || game.awayWins > 0) && (
        <div className="text-center">
          <button
            onClick={onClearStats}
            className="bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-4 rounded-lg text-sm transition-all duration-200"
          >
            Clear All Stats
          </button>
        </div>
      )}
    </div>
  );
}
