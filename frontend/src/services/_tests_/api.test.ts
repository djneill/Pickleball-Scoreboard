import { vi } from "vitest";
import { gameApi } from "../api";

// Mock the entire API module
vi.mock("../api", () => ({
  gameApi: {
    startNewGame: vi.fn(),
    getCurrentGame: vi.fn(),
    updateScore: vi.fn(),
    getGameStats: vi.fn(),
  },
}));

const mockGameApi = gameApi as unknown as {
  startNewGame: ReturnType<typeof vi.fn>;
  getCurrentGame: ReturnType<typeof vi.fn>;
  updateScore: ReturnType<typeof vi.fn>;
  getGameStats: ReturnType<typeof vi.fn>;
};

test("startNewGame returns expected data", async () => {
  const expectedGame = {
    id: "123",
    gameType: "Singles" as const,
    homeScore: 0,
    awayScore: 0,
    homeWins: 0,
    awayWins: 0,
    isGameComplete: false,
    createdAt: "2023-01-01T00:00:00.000Z",
  };

  mockGameApi.startNewGame.mockResolvedValue(expectedGame);

  const result = await gameApi.startNewGame("Singles");
  expect(result).toEqual(expectedGame);
});
