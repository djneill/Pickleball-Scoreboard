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

const API_BASE_URL = import.meta.env.VITE_API_URL;

// Option 1: Use axios directly (recommended - simpler)
export const gameApi = {
  getCurrentGame: async (): Promise<GameState | null> => {
    try {
      const response = await axios.get<GameState>(`${API_BASE_URL}/game`);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  startNewGame: async (gameType: "Singles" | "Doubles"): Promise<GameState> => {
    console.log("Sending request:", { gameType });
    const response = await axios.post<GameState>(`${API_BASE_URL}/game/new`, {
      gameType: gameType,
    });
    return response.data;
  },

  updateScore: async (
    team: "Home" | "Away",
    change: 1 | -1
  ): Promise<GameState> => {
    const response = await axios.put<GameState>(`${API_BASE_URL}/game/score`, {
      team,
      change,
    });
    return response.data;
  },

  getGameStats: async (): Promise<GameStatsResponse> => {
    const response = await axios.get<GameStatsResponse>(
      `${API_BASE_URL}/game/stats`
    );
    return response.data;
  },

  clearStats: async (): Promise<GameStatsResponse> => {
    const response = await axios.delete<GameStatsResponse>(
      `${API_BASE_URL}/game/stats`
    );
    return response.data;
  },
};
