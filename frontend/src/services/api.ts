import axios from "axios";

export interface GameState {
  id: string;
  gameType: "Singles" | "Doubles";
  homeScore: number;
  awayScore: number;
  homeWins: number;
  awayWins: number;
  isGameComplete: boolean;
  createdAt: string;
  completedAt?: string;
}

export interface NewGameRequest {
  gameType: "Singles" | "Doubles";
}

export interface ScoreUpdateRequest {
  team: "Home" | "Away";
  change: 1 | -1;
}

export interface GameStatsResponse {
  totalGamesPlayed: number;
  homeWins: number;
  awayWins: number;
  currentGame?: GameState;
}

const API_BASE_URL =
  import.meta.env.VITE_API_URL ||
  "https://pickleball-scoreboard-api-gygycwa2c2hpgabr.centralus-01.azurewebsites.net";

const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    "Content-Type": "application/json",
  },
});

export const gameApi = {
  getCurrentGame: async (): Promise<GameState | null> => {
    try {
      const response = await apiClient.get<GameState>("/game");
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  startNewGame: async (gameType: "Singles" | "Doubles"): Promise<GameState> => {
    // Debug log
    console.log("Sending request:", { gameType });
    const response = await apiClient.post<GameState>("/game/new", {
      gameType: gameType,
    });
    return response.data;
  },

  updateScore: async (
    team: "Home" | "Away",
    change: 1 | -1
  ): Promise<GameState> => {
    const response = await apiClient.put<GameState>("/game/score", {
      team,
      change,
    });
    return response.data;
  },

  getGameStats: async (): Promise<GameStatsResponse> => {
    const response = await apiClient.get<GameStatsResponse>("/game/stats");
    return response.data;
  },

  clearStats: async (): Promise<GameStatsResponse> => {
    const response = await apiClient.delete<GameStatsResponse>("/game/stats");
    return response.data;
  },
};
