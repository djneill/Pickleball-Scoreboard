interface TeamScoreProps {
  teamName: string;
  score: number;
  color: string;
  onScoreChange: (change: 1 | -1) => void;
  disabled: boolean;
  canDecrement: boolean;
}

export default function TeamScore({
  teamName,
  score,
  color,
  onScoreChange,
  disabled,
  canDecrement,
}: TeamScoreProps) {
  return (
    <div className="text-center">
      <h3 className="text-lg font-semibold text-gray-700 mb-2">
        {teamName.toUpperCase()}
      </h3>
      <div className={`text-6xl font-bold ${color} mb-4`}>{score}</div>
      <div className="space-y-2">
        <button
          onClick={() => onScoreChange(1)}
          disabled={disabled}
          className="w-full bg-green-500 hover:bg-green-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold py-3 px-4 rounded-lg text-xl transition-all duration-200"
        >
          +
        </button>
        <button
          onClick={() => onScoreChange(-1)}
          disabled={disabled || !canDecrement}
          className="w-full bg-red-500 hover:bg-red-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold py-3 px-4 rounded-lg text-xl transition-all duration-200"
        >
          -
        </button>
      </div>
    </div>
  );
}
