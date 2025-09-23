import { render, screen, fireEvent } from "@testing-library/react";
import TeamScore from "../TeamScore";
import { vi } from "vitest";

test("displays correct team name and score", () => {
  render(
    <TeamScore
      teamName="Home"
      score={5}
      color="text-blue-600"
      onScoreChange={vi.fn()} // Changed from jest.fn()
      disabled={false}
      canDecrement={true}
    />
  );

  expect(screen.getByText("HOME")).toBeInTheDocument();
  expect(screen.getByText("5")).toBeInTheDocument();
});

test("calls onScoreChange with correct values", () => {
  const mockOnScoreChange = vi.fn(); // Changed from jest.fn()
  render(
    <TeamScore
      teamName="Home"
      score={5}
      color="text-blue-600"
      onScoreChange={mockOnScoreChange}
      disabled={false}
      canDecrement={true}
    />
  );

  fireEvent.click(screen.getByText("+"));
  expect(mockOnScoreChange).toHaveBeenCalledWith(1);

  fireEvent.click(screen.getByText("-"));
  expect(mockOnScoreChange).toHaveBeenCalledWith(-1);
});
