import { render, screen, waitFor } from "@testing-library/react";
import PickleballScoreboard from "../ScoreBoard";

// Mock the entire gameApi module
vi.mock("../../services/api", () => ({
  gameApi: {
    getCurrentGame: vi.fn(),
    startNewGame: vi.fn(),
    updateScore: vi.fn(),
    getGameStats: vi.fn(),
  },
}));

// Import after mocking
import { gameApi } from "../../services/api";
import { vi } from "vitest";

test("shows game type selection when no current game", async () => {
  // Cast to vi mock function and set return value
  vi.mocked(gameApi.getCurrentGame).mockResolvedValue(null);

  render(<PickleballScoreboard />);

  await waitFor(() => {
    expect(screen.getByText("Choose Game Type")).toBeInTheDocument();
  });
});

test("shows game interface when game is active", async () => {
  const mockGame = {
    id: "123",
    gameType: "Singles" as const,
    homeScore: 5,
    awayScore: 3,
    homeWins: 1,
    awayWins: 0,
    isGameComplete: false,
    createdAt: new Date().toISOString(),
  };

  vi.mocked(gameApi.getCurrentGame).mockResolvedValue(mockGame);

  render(<PickleballScoreboard />);

  await waitFor(() => {
    expect(screen.getByText("Singles Game")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });
});
