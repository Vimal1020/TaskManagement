# Task Management System

A lightweight task management API built with ASP.NET Core 8, featuring JWT authentication and Docker support.

## Features

- **Task Management**: Create, read, update, and delete tasks with filtering and search
- **Authentication**: Secure JWT-based authentication with role-based access
- **Infrastructure**: Redis caching, containerized with Docker

## Technology Stack

- ASP.NET Core 8 and Entity Framework Core
- MySQL database with Redis caching
- Docker containerization
- CI/CD with GitHub Actions

## Quick Start

### Prerequisites
- .NET 8 SDK
- Docker and Docker Compose

### Run with Docker
```bash
# Start all services
docker-compose up -d

# Access API at http://localhost:8080/swagger
```

### Development Setup
```bash
# Restore packages
dotnet restore

# Run the API
dotnet run --project TaskManagement.API

# Run tests
dotnet test
```

## API Endpoints

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| POST | /api/tasks | Create task | Admin |
| GET | /api/tasks/{id} | Get task | Admin, Manager, User |
| PUT | /api/tasks/{id} | Update task | Admin, Manager |
| DELETE | /api/tasks/{id} | Delete task | Admin |
| GET | /api/tasks/user/{userId} | Get user tasks | Admin, Manager, User |
| POST | /api/auth/register | Register user | Public |
| POST | /api/auth/login | Login | Public |

## Project Structure

```
TaskManagement/
├── TaskManagement.API/        # Web API and controllers
├── TaskManagement.Core/       # Domain models and interfaces
├── TaskManagement.Infrastructure/  # Data access and services
├── TaskManagement.Tests/      # Unit and integration tests
├── docker-compose.yml
└── .github/workflows/         # CI/CD configuration
```
