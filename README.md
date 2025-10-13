# Pickleball Scoreboard

A production-ready, full-stack pickleball web application built with enterprise-grade architecture and modern development practices.

## 💼 Enterprise-Grade Engineering

This isn't just a scoreboard—it's a demonstration of production-ready software engineering:

- **Scalable from Day 1**: Multi-tenant architecture supports growth from dozens to thousands of users
- **Security-First**: Google OAuth + JWT + ASP.NET Identity = enterprise authentication standards
- **Maintainable**: 60 automated tests ensure code changes don't break functionality
- **Cloud-Native**: Deployed on Azure with managed PostgreSQL (Supabase)
- **Type-Safe**: End-to-end type safety eliminates entire classes of bugs
- **CI/CD Ready**: Structured for automated deployment pipelines

## ☁️ Production Deployment

**Live on Azure Cloud Infrastructure:**

- **Frontend**: Azure Static Web Apps with global CDN
- **Backend API**: Azure App Service with auto-scaling capabilities
- **Database**: Supabase managed PostgreSQL with automatic backups
- **Authentication**: Google OAuth 2.0 with JWT token validation
- **Security**: HTTPS-only, CORS configured, connection strings in Azure configuration

## 🏗️ Architecture Overview

This project demonstrates a professional, scalable approach to full-stack development with:

- **Clean Architecture**: Separation of concerns with dedicated layers (Controllers, Services, Data)
- **Type Safety**: End-to-end TypeScript on frontend, C# strong typing on backend
- **Test-Driven Development**: Comprehensive unit, integration, and E2E tests across the stack
- **Secure Authentication**: Google OAuth + JWT with ASP.NET Identity
- **Multi-Tenant Design**: Complete user isolation with database-backed persistence

## 🔄 Architecture Evolution

**Phase 1**: Rapid prototyping with in-memory storage for proof of concept

**Phase 2**: User isolation problem identified during Azure deployment (shared global state)

**Phase 3**: Migration to full authentication system + PostgreSQL persistence + user-scoped data

**Phase 4**: Comprehensive E2E test suite added with Playwright for complete user journey validation

**Result**: Production-ready multi-tenant architecture with complete user isolation, data persistence, and full test coverage

## 🎯 Production Features

### Authentication & Security

- ✅ Google OAuth 2.0 integration with server-side token verification
- ✅ JWT bearer token authentication for stateless API authorization
- ✅ ASP.NET Core Identity for comprehensive user management
- ✅ Protected API endpoints with [Authorize] attributes
- ✅ User isolation ensuring multi-tenant data security
- ✅ Secure token storage with automatic refresh handling
- ✅ Protected routes with authentication guards

### Data Persistence & Database

- ✅ Entity Framework Core with Code-First migrations
- ✅ PostgreSQL production database via Supabase
- ✅ SQLite for local development environment
- ✅ In-memory database for isolated unit testing
- ✅ User-scoped queries preventing cross-user data access
- ✅ Automatic migration application on deployment

### Full-Stack Type Safety

- ✅ Strict TypeScript configuration with no implicit any
- ✅ Type-safe API client with Axios interceptors
- ✅ Shared type definitions between frontend and backend
- ✅ Strong typing in C# with nullable reference types enabled

### Game Features

- ✅ Real-time score tracking with live updates
- ✅ Game type selection (Singles/Doubles)
- ✅ Team management with player assignment
- ✅ Pickleball scoring rules (11 points to win, must win by 2)
- ✅ Match statistics tracking across multiple games
- ✅ Responsive mobile-first design with touch-friendly controls

## 🧪 Testing Strategy

**60 Total Tests** ensuring reliability and maintainability:

### Backend Tests (30 tests)

- **10 Authentication Tests** (xUnit + FluentAssertions)
  - Registration validation
  - Login flow with JWT generation
  - Google OAuth token verification
  - Authorization enforcement
- **20 Game Service & API Tests**
  - Pickleball scoring logic (11 points, win by 2)
  - User isolation verification
  - Match statistics tracking
  - Game state persistence
  - API endpoint integration tests

### Frontend Tests (17 tests)

- **Component Unit Tests** (Vitest + React Testing Library)
  - Authentication components
  - Score display and update logic
  - Game type selector
- **Integration Tests**
  - User authentication workflows
  - Protected route navigation
  - API service layer with mocked responses

### End-to-End Tests (13 tests)

- **Complete User Journey Testing** (Playwright)
  - User registration through UI
  - Game type selection flow
  - Complete scoring scenarios (11-0, 11-9, 11-10, 12-10)
  - "Win by 2" rule enforcement
  - Match statistics tracking across games
  - Clear stats functionality with dialog confirmation
  - Score persistence across page reloads
  - Button state management (disabled states)
  - Multi-game session workflows
  - Game completion detection

### Run Tests

```bash
# Backend (30 tests)
cd backend
dotnet test

# Frontend (17 tests)
cd frontend
npm test                # Watch mode
npm run test:run        # Single run
npm run test:coverage   # With coverage report
npm run test:ui         # Visual UI

# E2E Tests (13 tests)
npm run test:e2e        # Run all E2E tests (headless)
npm run test:e2e:ui     # Interactive UI mode (recommended)
npm run test:e2e:headed # Run with visible browser
npm run test:e2e:debug  # Step-by-step debugger
```

## 🏛️ Key Technical Decisions

| Decision                    | Rationale                                                                      |
| --------------------------- | ------------------------------------------------------------------------------ |
| .NET 9 + EF Core            | Type-safe ORM, excellent performance, enterprise support, long-term stability  |
| ASP.NET Identity            | Battle-tested authentication framework, extensible, regular security patches   |
| JWT + Google OAuth          | Stateless auth scales horizontally, trusted identity provider reduces friction |
| PostgreSQL via Supabase     | Managed service reduces ops burden, production-ready with automatic backups    |
| React + TypeScript          | Type safety catches bugs at compile-time, component reusability                |
| xUnit + Vitest + Playwright | Industry-standard testing frameworks, excellent CI/CD integration              |
| Monorepo Structure          | Simplified dependency management, atomic commits across stack                  |
| Playwright for E2E          | Reliable browser automation, cross-browser testing, excellent debugging tools  |

## 📁 Project Structure

```
PBscoreboard/
├── frontend/                              # React application
│   ├── src/
│   │   ├── components/                    # React components
│   │   │   ├── Header.tsx
│   │   │   ├── ScoreBoard.tsx
│   │   │   ├── LogoutButton.tsx
│   │   │   ├── PrivateRoute.tsx
│   │   │   └── __tests__/                 # Component tests (7 tests)
│   │   │       ├── ScoreBoard.integration.test.tsx
│   │   │       ├── auth.test.tsx
│   │   │       ├── GameTypeSelector.test.tsx
│   │   │       └── TeamScore.test.tsx
│   │   ├── contexts/                      # React Context providers
│   │   │   ├── auth-context.ts
│   │   │   └── AuthProvider.tsx
│   │   ├── hooks/                         # Custom React hooks
│   │   │   └── useAuth.ts
│   │   ├── pages/                         # Route pages
│   │   │   ├── Dashboard.tsx
│   │   │   └── AuthPage.tsx
│   │   ├── services/                      # API integration
│   │   │   ├── api.ts
│   │   │   └── __tests__/                 # API tests (10 tests)
│   │   │       └── api.test.ts
│   │   ├── types/                         # TypeScript definitions
│   │   │   └── auth.types.ts
│   │   └── utils/                         # Utility functions
│   ├── vitest.config.ts                   # Unit test configuration
│   └── package.json
│
├── e2e/                                   # End-to-end tests (13 tests)
│   ├── first-test.spec.ts                 # Complete scoring scenarios
│   └── helpers/                           # Test utilities (ready for expansion)
│
├── backend/                               # .NET solution
│   ├── PickleballScoreboard.slnx          # Modern solution file
│   ├── PickleballApi/                     # Web API project
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs          # OAuth & JWT auth
│   │   │   └── GameController.cs          # Game logic API
│   │   ├── Models/                        # Domain entities
│   │   │   ├── ApplicationUser.cs         # Identity user model
│   │   │   ├── Game.cs
│   │   │   ├── MatchStatistics.cs
│   │   │   └── Team.cs
│   │   ├── Services/                      # Business logic layer
│   │   │   ├── IGameService.cs
│   │   │   └── GameService.cs
│   │   ├── Data/                          # Database context
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── ApplicationDbContextFactory.cs
│   │   ├── Migrations/                    # EF Core migrations
│   │   ├── Program.cs                     # App configuration & DI
│   │   └── PickleballApi.csproj
│   │
│   └── PickleballScoreboard.Tests/        # Test project (30 tests)
│       ├── Controllers/
│       │   ├── AuthControllerTests.cs     # 10 auth tests
│       │   └── GameControllerTests.cs     # 20 game tests
│       └── PickleballScoreboard.Tests.csproj
│
├── playwright.config.ts                   # E2E test configuration
└── README.md
```

## 🚀 Tech Stack

### Backend Technologies

- **.NET 9** - Latest LTS framework with performance improvements
- **ASP.NET Core Web API** - RESTful service architecture
- **Entity Framework Core 9** - ORM with Code-First migrations
- **ASP.NET Core Identity** - Complete user management system
- **JWT Bearer Authentication** - Stateless authentication tokens
- **Npgsql** - PostgreSQL provider for EF Core
- **Google.Apis.Auth** - Google OAuth token verification
- **xUnit** - Unit testing framework
- **FluentAssertions** - Expressive test assertions
- **Swagger/OpenAPI** - Interactive API documentation

### Frontend Technologies

- **React 19** - Latest React with modern concurrent features
- **TypeScript 5.8** - Strict type checking configuration
- **Vite** - Lightning-fast build tool and dev server
- **React Router v7** - Declarative client-side routing
- **TailwindCSS 4** - Utility-first CSS framework
- **Axios** - HTTP client with request/response interceptors
- **@react-oauth/google** - Google OAuth integration
- **Vitest** - Fast unit test runner compatible with Vite
- **React Testing Library** - User-centric component testing
- **@testing-library/user-event** - User interaction simulation

### Testing & Quality Assurance

- **Playwright** - Modern E2E testing framework with browser automation
- **xUnit** - Backend unit testing framework
- **FluentAssertions** - Readable test assertions
- **Vitest** - Frontend unit testing
- **React Testing Library** - Component testing

### Infrastructure & Database

- **Supabase PostgreSQL** - Managed PostgreSQL with automatic backups
- **Azure Static Web Apps** - Frontend hosting with global CDN
- **Azure App Service** - Backend API hosting with auto-scaling
- **GitHub** - Version control and collaboration

## ⚡ Quick Start

### Prerequisites

- Node.js 18-20 (enforced by package.json engines)
- .NET 9 SDK
- PostgreSQL (via Supabase) or SQLite for local dev
- Google OAuth credentials (for authentication)

### Run Locally in 3 Minutes

```bash
# 1. Backend (Terminal 1)
cd backend/PickleballApi
dotnet restore
dotnet run
# API runs at http://localhost:5284
# Swagger UI: http://localhost:5284/swagger

# 2. Frontend (Terminal 2)
cd frontend
npm install
npm run dev
# Opens at http://localhost:5173

# 3. Run All Tests
cd backend && dotnet test          # 30 backend tests
cd frontend && npm test            # 17 frontend tests
npm run test:e2e                   # 13 E2E tests
```

## 🛠️ Development Setup

### Environment Configuration

**Backend** (`backend/PickleballApi/appsettings.Development.json` or User Secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-supabase-host.supabase.co;Database=postgres;Username=postgres;Password=your-password"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "PickleballScoreboard",
    "Audience": "PickleballScoreboardApp",
    "ExpirationMinutes": 60
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  }
}
```

**Use User Secrets for Development** (recommended):

```bash
cd backend/PickleballApi
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key"
dotnet user-secrets set "Google:ClientId" "your-google-client-id"
```

**Frontend** (`.env` file):

```bash
VITE_API_URL=http://localhost:5284/api
VITE_GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
```

### Database Migrations

```bash
cd backend/PickleballApi

# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

## 🧪 E2E Testing with Playwright

### What E2E Tests Validate

End-to-end tests verify complete user workflows by automating a real browser:

- User registration through the UI
- Google OAuth authentication flow
- Game type selection (Singles/Doubles)
- Complete scoring scenarios validating pickleball rules
- Match statistics persistence across sessions
- UI state management (button disabled states, visibility)
- Data persistence across page reloads
- Multi-game workflows

### Running E2E Tests

```bash
# Run all E2E tests (headless - fast)
npm run test:e2e

# Interactive UI mode (recommended for development)
npm run test:e2e:ui

# Run with visible browser (see what's happening)
npm run test:e2e:headed

# Step-by-step debugger
npm run test:e2e:debug

# Run specific test
npx playwright test -g "should win at 11-9"

# Generate HTML report
npx playwright show-report
```

### E2E Test Architecture

Tests use helper functions for common workflows:

```typescript
// Register new user and start a game
await setupGame(page);

// Get current scores from UI
const scores = await getScores(page);

// All tests isolated with unique user accounts
// No test interference or cleanup needed
```

### Adding New E2E Tests

1. Create test file in `e2e/` directory
2. Import helpers from existing test files
3. Use `data-testid` attributes on components for reliable element selection
4. Run in UI mode for faster iteration: `npm run test:e2e:ui`

## 🔐 Authentication Flow

1. **User Initiates Login**: Clicks "Sign in with Google" button
2. **Google OAuth**: User authenticates via Google's OAuth 2.0 flow
3. **Token Verification**: Backend verifies Google token with Google's API
4. **User Registration/Login**: Creates or retrieves user from database
5. **JWT Generation**: Server issues custom JWT with user claims (userId, email)
6. **Client Storage**: Frontend stores JWT in AuthContext (localStorage)
7. **Protected Routes**: Frontend guards routes requiring authentication
8. **API Authorization**: Axios interceptor adds Bearer token to all API requests
9. **Token Validation**: Backend validates JWT on each protected endpoint

## 🚢 Deployment

### Frontend (Azure Static Web Apps)

```bash
cd frontend
npm run build
# Deploy dist/ folder to Azure Static Web Apps
# Configure environment variables in Azure portal:
# - VITE_API_URL
# - VITE_GOOGLE_CLIENT_ID
```

### Backend (Azure App Service)

```bash
cd backend/PickleballApi
dotnet publish -c Release -o ./publish
# Deploy publish/ folder to Azure App Service
# Configure app settings in Azure portal (use __ for nested config):
# - ConnectionStrings__DefaultConnection
# - JwtSettings__SecretKey
# - JwtSettings__Issuer
# - JwtSettings__Audience
# - Google__ClientId
```

### Database (Supabase)

1. Create Supabase project and obtain connection string
2. Migrations apply automatically on first API startup
3. Connection string stored securely in Azure App Service configuration

## 📈 Code Quality Standards

- **TypeScript Strict Mode**: Maximum type safety with `strict: true`
- **ESLint**: Code linting with React and TypeScript best practices
- **Entity Framework Migrations**: Version-controlled database schema
- **Nullable Reference Types**: Enabled in C# to reduce null reference exceptions
- **CORS Security**: Configured for specific allowed origins only
- **User Secrets**: No credentials in source control
- **Comprehensive Testing**: Unit, integration, and E2E tests across stack
- **API Documentation**: Swagger/OpenAPI automatically generated
- **Test Data Isolation**: Each test creates unique users for complete isolation

## 🔒 Security Best Practices

- ✅ JWT tokens with expiration (configurable lifetime)
- ✅ HTTPS-only enforced in production
- ✅ CORS configured for specific trusted origins
- ✅ User secrets for sensitive configuration data
- ✅ Google OAuth for trusted third-party authentication
- ✅ ASP.NET Identity with password hashing (PBKDF2)
- ✅ SQL injection protection via EF Core parameterization
- ✅ Authorization enforcement on all protected endpoints
- ✅ Bearer token validation on every API request

## 📄 API Documentation

Interactive API documentation available at `/swagger` when running the backend locally or on Azure.

### Key Endpoints

**Authentication:**

- `POST /api/auth/register` - Email/password registration
- `POST /api/auth/login` - Email/password login
- `POST /api/auth/google` - Google OAuth authentication

**Game Management:**

- `GET /api/game` - Get current game state (requires auth)
- `POST /api/game/new` - Start new game with game type (requires auth)
- `PUT /api/game/score` - Update team score +1/-1 (requires auth)
- `GET /api/game/stats` - Retrieve match statistics (requires auth)
- `DELETE /api/game/stats` - Clear match statistics (requires auth)

All game endpoints require JWT authentication via `Authorization: Bearer <token>` header.

## 🤝 Development Principles

This project follows industry best practices:

- **SOLID Principles**: Single responsibility, dependency injection, interface segregation
- **Clean Code**: Readable, maintainable, self-documenting code
- **Test-Driven Development**: Tests guide implementation and prevent regressions
- **Convention over Configuration**: Following .NET and React framework standards
- **Security by Design**: Authentication and authorization built from the ground up
- **Scalable Architecture**: Stateless API ready for horizontal scaling
- **Separation of Concerns**: Clear boundaries between layers (UI, API, Service, Data)
- **Comprehensive Testing**: Unit, integration, and E2E coverage

## 🎯 Future Enhancements

- 🔲 CI/CD pipeline with GitHub Actions running all 60 tests
- 🔲 Password reset functionality via email
- 🔲 Match history with user-specific game archive
- 🔲 Tournament bracket support for competitive play
- 🔲 Player profiles with detailed statistics
- 🔲 Real-time multiplayer sync using SignalR
- 🔲 PWA support for offline functionality
- 🔲 Social features (friend lists, challenges)
- 🔲 Advanced analytics and performance insights
- 🔲 Visual regression testing with Playwright screenshots
- 🔲 Cross-browser E2E testing (Chrome, Firefox, Safari)

## 📊 Project Metrics

- **Lines of Code**: ~3,500+ across frontend, backend, and tests
- **Test Coverage**: 60 automated tests (30 backend, 17 frontend, 13 E2E)
- **Technology Stack**: 20+ modern technologies
- **Development Time**: Evolved through multiple architectural phases
- **Deployment Platforms**: 3 (Azure Static Web Apps, Azure App Service, Supabase)
- **Test Execution Time**: ~3 minutes for complete test suite

---

**Built as a demonstration of enterprise-level full-stack development practices suitable for production environments.**

This project showcases:

- Modern authentication patterns
- Cloud-native architecture
- Comprehensive testing strategies (unit, integration, E2E)
- Clean code principles
- Production deployment experience
- Full-stack type safety
- Database design and migrations
- API design and documentation
- Browser automation and E2E testing
