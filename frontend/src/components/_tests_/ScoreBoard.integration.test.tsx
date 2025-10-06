import { render, screen, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { vi } from "vitest";
import PickleballScoreboard from "../ScoreBoard";
import AuthProvider from "../../contexts/AuthProvider";

// Mock the entire gameApi module
vi.mock("../../services/api", () => ({
  gameApi: {
    getCurrentGame: vi.fn(),
    startNewGame: vi.fn(),
    updateScore: vi.fn(),
    getGameStats: vi.fn(),
  },
}));

// Mock axios for AuthProvider
vi.mock("axios", () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    interceptors: {
      request: {
        use: vi.fn(() => 1),
        eject: vi.fn(),
      },
    },
  },
}));

// Mock react-router-dom navigation
vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual<typeof import("react-router-dom")>(
    "react-router-dom"
  );
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

// Import after mocking
import { gameApi } from "../../services/api";

test("shows game type selection when no current game", async () => {
  vi.mocked(gameApi.getCurrentGame).mockResolvedValue(null);

  render(
    <BrowserRouter>
      <AuthProvider>
        <PickleballScoreboard />
      </AuthProvider>
    </BrowserRouter>
  );

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

  render(
    <BrowserRouter>
      <AuthProvider>
        <PickleballScoreboard />
      </AuthProvider>
    </BrowserRouter>
  );

  await waitFor(() => {
    expect(screen.getByText("Singles Game")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });
});
