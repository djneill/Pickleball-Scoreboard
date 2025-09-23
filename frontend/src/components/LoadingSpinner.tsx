interface LoadingSpinnerProps {
  isLoading: boolean;
}

export default function LoadingSpinner({ isLoading }: LoadingSpinnerProps) {
  if (!isLoading) return null;

  return (
    <div className="text-center text-white mb-4">
      <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-white"></div>
      <p className="mt-2">Loading...</p>
    </div>
  );
}
