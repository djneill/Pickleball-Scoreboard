interface GameTypeSelectionProps {
  onSelectGameType: (gameType: "Singles" | "Doubles") => void;
  isLoading: boolean;
}

export default function GameTypeSelection({
  onSelectGameType,
  isLoading,
}: GameTypeSelectionProps) {
  return (
    <div className="bg-white/95 backdrop-blur-sm rounded-2xl p-8 shadow-xl max-w-sm mx-auto">
      <h2 className="text-2xl font-bold text-gray-800 mb-6 text-center">
        Choose Game Type
      </h2>
      <div className="space-y-4">
        <button
          onClick={() => onSelectGameType("Singles")}
          disabled={isLoading}
          className="w-full bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 text-white font-bold py-4 px-6 rounded-xl text-lg transition-all duration-200 transform hover:scale-105 disabled:cursor-not-allowed"
        >
          Singles Game
        </button>
        <button
          onClick={() => onSelectGameType("Doubles")}
          disabled={isLoading}
          className="w-full bg-purple-500 hover:bg-purple-600 disabled:bg-gray-300 text-white font-bold py-4 px-6 rounded-xl text-lg transition-all duration-200 transform hover:scale-105 disabled:cursor-not-allowed"
        >
          Doubles Game
        </button>
      </div>
    </div>
  );
}
