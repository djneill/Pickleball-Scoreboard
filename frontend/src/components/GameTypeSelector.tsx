interface GameTypeSelectionProps {
  onSelectGameType: (gameType: "Singles" | "Doubles") => void;
  isLoading: boolean;
}

export default function GameTypeSelection({
  onSelectGameType,
  isLoading,
}: GameTypeSelectionProps) {
  return (
    <div className="bg-white/90 backdrop-blur-sm rounded-xl p-6 mb-6 shadow-lg">
      <h2 className="text-xl font-semibold text-gray-800 mb-4 text-center">
        Choose Game Type
      </h2>
      <div className="grid grid-cols-2 gap-4">
        <button
          onClick={() => onSelectGameType("Singles")}
          disabled={isLoading}
          className="bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 text-white font-bold py-4 px-6 rounded-lg transition-all duration-200 transform hover:scale-105 disabled:cursor-not-allowed"
        >
          Singles
        </button>
        <button
          onClick={() => onSelectGameType("Doubles")}
          disabled={isLoading}
          className="bg-purple-500 hover:bg-purple-600 disabled:bg-gray-300 text-white font-bold py-4 px-6 rounded-lg transition-all duration-200 transform hover:scale-105 disabled:cursor-not-allowed"
        >
          Doubles
        </button>
      </div>
    </div>
  );
}
