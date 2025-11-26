# Leaderboard Backend

This project is a backend system for a real-time leaderboard service. The service allows users to compete in various games or activities, track their scores, and view their rankings on a leaderboard. The system features user authentication, score submission, real-time leaderboard updates, and score history tracking. Redis sorted sets are used to manage and query the leaderboards efficiently.

## Features

- User authentication: Users can register and log in to the system.
- Score submission: Users can submit their scores for various games or activities.
- Real-time leaderboard updates: The leaderboard is updated in real-time to reflect the latest scores submitted by users.
- Score history tracking: Users can view their score history for a specific game or activity.
- Top Players Report: Generate reports on the top players for a specific period.

## Endpoints

The API provides the following endpoints:


Authentication:
- POST `api/auth/register`: Register a new user.
- POST `api/auth/login`: Log in a user. Returns a JWT.

Game:
- POST `api/games`: Create a new game. Requires 'Admin' role.
- GET  `api/games/{gameId}`: Get a specific game by ID.

Score:
- POST `api/scores`: Submit a score for a user in a specific game.
- GET  `api/leaderboard/{gameId}`: Get the leaderboard for a specific game.
- GET  `api/leaderboard/{gameId}/rank/{userId}`: Get the rank of a specific user in a specific game.
- GET  `api/leaderboard/{gameId}/top/{limit}`: Get the top N players in a specific game.

Reports:
- GET  `api/reports/top-players/{gameId}/{limit}`: Get the top players for a specific game.