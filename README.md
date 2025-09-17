# Pickleball Scoreboard

A mobile-focused pickleball scoreboard web application with real-time score tracking and match statistics.

## Tech Stack

**Frontend:**

- React 18 with TypeScript
- Vite (build tool)
- Testing Library + Vitest

**Backend:**

- .NET 8 Web API with Controllers
- Entity Framework Core (In-Memory)
- xUnit + FluentAssertions for testing

**Deployment:**

- Azure Static Web Apps (Frontend)
- Azure App Service (Backend)

## Project Structure

```
pickleball-scoreboard/
├── frontend/                           # React + Vite application
│   ├── src/
│   │   ├── components/
│   │   │   └── __tests__/
│   │   ├── services/
│   │   │   └── __tests__/
│   │   └── utils/
│   │       └── __tests__/
│   └── package.json
├── backend/                            # .NET solution
│   ├── PickleballScoreboard.slnx       # Modern solution file
│   ├── PickleballApi/                  # Web API project
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Program.cs
│   └── PickleballScoreboard.Tests/     # Test project
└── README.md
```

## Features (Planned)

- [x] Project structure and boilerplate
- [ ] Game type selection (Singles/Doubles)
- [ ] Real-time score tracking with +/- buttons
- [ ] Pickleball win condition logic (11 points, win by 2)
- [ ] Match statistics (Home wins: X, Away wins: Y)
- [ ] New game functionality
- [ ] Mobile-responsive design
- [ ] Local storage for offline use
- [ ] API integration for cross-device sync

## Getting Started

### Prerequisites

- Node.js 18+
- .NET 8 SDK
- VS Code (recommended)

### Frontend Development

```bash
cd frontend
npm install
npm run dev
# Opens at http://localhost:5173
```

### Backend Development

```bash
cd backend
dotnet restore
dotnet run --project PickleballApi
# API runs at http://localhost:5000
```

### Running Tests

**Frontend:**

```bash
cd frontend
npm test
```

**Backend:**

```bash
cd backend
dotnet test
```

## Development Status

🚧 **Currently in development** - Setting up API contracts and core functionality

## API Endpoints (Planned)

- `GET /api/game` - Get current game state
- `POST /api/game/new` - Start new game
- `PUT /api/game/score` - Update score
- `GET /api/game/stats` - Get match statistics
