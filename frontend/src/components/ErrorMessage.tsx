interface ErrorMessageProps {
  error: string | null;
  onDismiss: () => void;
}

export default function ErrorMessage({ error, onDismiss }: ErrorMessageProps) {
  if (!error) return null;
  return (
    <div className="bg-red-500 text-white p-3 rounded-lg mb-4 text-center">
      {error}
      <button
        onClick={onDismiss}
        className="ml-2 text-red-200 hover:text-white"
      >
        ✕
      </button>
    </div>
  );
}
