# Planning Poker Application

A real-time collaborative story point estimation tool built with ASP.NET Core 10 and Vue 3.

## Features

- **Real-time Updates**: Server-Sent Events (SSE) for instant synchronization
- **Session Management**: Create and join sessions with unique codes
- **Fibonacci Voting**: Standard planning poker cards (1, 2, 3, 5, 8, 13, 21)
- **Admin Controls**: Start voting, reveal votes, reset rounds
- **Vote Analytics**: Automatic calculation of low/high Fibonacci averages and distribution charts
- **Auto-Cleanup**: Inactive sessions removed after 10 minutes
- **In-Memory Storage**: No database required, simple deployment

## Tech Stack

### Backend
- ASP.NET Core 10 (.NET 10)
- .NET Aspire for orchestration
- Server-Sent Events (SSE) for real-time communication
- In-memory storage with thread-safe operations

### Frontend
- Vue 3 with TypeScript
- Vite for fast development and building
- Tailwind CSS for styling
- Pinia for state management
- Vue Router for navigation

## Project Structure

```
Planning Poker/
├── src/
│   ├── PlanningPoker.AppHost/         # Aspire orchestration
│   ├── PlanningPoker.ServiceDefaults/ # Shared Aspire configuration
│   ├── PlanningPoker.Api/             # ASP.NET Core backend
│   │   ├── Models/                    # Data models
│   │   ├── DTOs/                      # Request/Response objects
│   │   ├── Services/                  # Business logic
│   │   ├── Endpoints/                 # Minimal API endpoints
│   │   └── wwwroot/                   # Vue build output
│   └── PlanningPoker.Web/             # Vue 3 frontend
│       ├── src/
│       │   ├── components/            # Vue components
│       │   ├── composables/           # Reusable logic
│       │   ├── stores/                # Pinia stores
│       │   ├── types/                 # TypeScript types
│       │   └── views/                 # Route views
│       └── package.json
└── PlanningPoker.sln
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 18+ and npm

### Building the Application

1. **Build the Frontend:**
   ```bash
   cd "src/PlanningPoker.Web"
   npm install
   npm run build
   ```

2. **Build the Backend:**
   ```bash
   cd "src/PlanningPoker.Api"
   dotnet build
   ```

### Running the Application

#### Option 1: Run API Directly

```bash
cd "src/PlanningPoker.Api"
dotnet run
```

Then open your browser to `http://localhost:5000` or the URL shown in the console.

#### Option 2: Run with Aspire (Recommended)

```bash
cd "src/PlanningPoker.AppHost"
dotnet run
```

This will start the Aspire dashboard and the API automatically.

### Development Mode

For frontend development with hot-reload:

```bash
cd "src/PlanningPoker.Web"
npm run dev
```

The Vue dev server will proxy API requests to `http://localhost:5000`.

## How to Use

### Creating a Session

1. Click "Create New Session"
2. Enter your name
3. Share the session code or magic link with your team

### Joining a Session

1. Click "Join Existing Session"
2. Enter the session code and your name
3. Start collaborating!

### Voting Process

1. **Admin** clicks "Start Voting"
2. **Everyone** selects their estimate from Fibonacci cards
3. **Admin** clicks "Reveal Votes" when ready
4. View results:
   - Low Estimate (closest Fibonacci ≤ average)
   - Actual Average (arithmetic mean)
   - High Estimate (closest Fibonacci ≥ average)
   - Vote distribution chart
   - Individual votes
5. **Admin** clicks "New Round" to reset and start again

## Architecture Highlights

### Real-time Communication
- **SSE (Server-Sent Events)** for server-to-client updates
- Automatic reconnection with exponential backoff
- Events: user_joined, user_left, voting_started, vote_submitted, votes_revealed, votes_reset, session_closed

### Thread Safety
- `ConcurrentDictionary` for session storage
- `SemaphoreSlim` for complex mutations
- Thread-safe operations throughout

### Session Management
- Unique 8-character alphanumeric codes
- Cryptographically random generation
- Activity tracking with automatic cleanup
- Owner-based permissions

### State Management
- Pinia store for centralized state
- Computed properties for derived state
- Reactive updates from SSE events

## API Endpoints

### Sessions
- `POST /api/sessions` - Create session
- `GET /api/sessions/{code}` - Get session details

### Users
- `POST /api/sessions/{code}/users` - Join session
- `DELETE /api/sessions/{code}/users/{userId}` - Leave session

### Voting
- `POST /api/sessions/{code}/voting/start` - Start voting round
- `POST /api/sessions/{code}/voting/votes` - Submit/update vote
- `POST /api/sessions/{code}/voting/reveal` - Reveal votes
- `POST /api/sessions/{code}/voting/reset` - Reset votes

### SSE
- `GET /api/sessions/{code}/events?userId={id}` - SSE connection

## Configuration

### Backend
- Session timeout: 10 minutes of inactivity
- Cleanup interval: 1 minute
- Fibonacci values: 1, 2, 3, 5, 8, 13, 21

### Frontend
- API proxy (dev): `http://localhost:5000`
- Build output: `../PlanningPoker.Api/wwwroot`

## Troubleshooting

### Port Conflicts
If port 5000 is in use, modify `launchSettings.json` in the Api project.

### Build Errors
- Ensure .NET 10 SDK is installed: `dotnet --version`
- Ensure Node.js is installed: `node --version`
- Clear node_modules: `rm -rf node_modules && npm install`

### SSE Connection Issues
- Check browser console for errors
- Verify CORS settings if running separately
- Check that userId and sessionCode are valid

## Future Enhancements

- User authentication
- Session persistence (optional)
- Custom card values
- Voting timer
- Session history
- Export results to CSV
- Dark mode

## License

This project was created for demonstration purposes.
