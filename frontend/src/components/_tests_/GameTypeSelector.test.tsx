import { render, screen, fireEvent } from "@testing-library/react";
import GameTypeSelector from "../GameTypeSelector";
import { vi } from "vitest";

test("calls onSelectGameType with Singles when Singles button clicked", () => {
  const mockOnSelect = vi.fn(); // Changed from jest.fn()
  render(
    <GameTypeSelector onSelectGameType={mockOnSelect} isLoading={false} />
  );

  fireEvent.click(screen.getByText("Singles Game"));
  expect(mockOnSelect).toHaveBeenCalledWith("Singles");
});

test("disables buttons when loading", () => {
  const mockOnSelect = vi.fn(); // Changed from jest.fn()
  render(<GameTypeSelector onSelectGameType={mockOnSelect} isLoading={true} />);

  expect(screen.getByText("Singles Game")).toBeDisabled();
  expect(screen.getByText("Doubles Game")).toBeDisabled();
});
