interface GameInfoProps {
  game: {
    gameType: string;
    isGameComplete: boolean;
  };
}

export default function GameInfo({ game }: GameInfoProps) {
  return (
    <div className="bg-white/90 backdrop-blur-sm rounded-xl p-4 mb-6 shadow-lg">
      <div className="text-center">
        <h2 className="text-lg font-semibold text-gray-800">
          {game.gameType} Game
        </h2>
        {game.isGameComplete && (
          <div className="mt-2 text-green-600 font-bold">Game Complete!</div>
        )}
      </div>
    </div>
  );
}
