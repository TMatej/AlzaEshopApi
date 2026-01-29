# AlzaEshop API

A modern e-commerce API built with ASP.NET Core using Minimal APIs architecture. This project provides a RESTful API with versioning support, comprehensive documentation via Scalar, and follows vertical slice architecture principles.

## Overview

AlzaEshop API is designed to handle e-commerce operations with a focus on:

- **Minimal API Architecture**: Leveraging ASP.NET Core's lightweight endpoint approach
- **API Versioning**: Supporting multiple API versions (v1, v2) with automatic OpenAPI documentation
- **Validation**: Using FluentValidation for robust request validation
- **Documentation**: Interactive API documentation powered by Scalar
- **Logging**: Structured logging with Serilog
- **Database**: Entity Framework Core integration

## Prerequisites

Before running the project, ensure you have the following installed:

### Required Software

- **.NET 10.0 SDK or later**
  - Download from: https://dotnet.microsoft.com/download
  - Verify installation: `dotnet --version`

- **Docker Desktop** or **Docker Engine**
  - Download from: https://www.docker.com/products/docker-desktop
  - Verify installation: `docker --version` and `docker compose version`

### Optional Tools

- **Visual Studio 2022** (17.8 or later) or **JetBrains Rider** (2023.3 or later)
- **Visual Studio Code** with C# extension
- **Git** for version control
- **Bruno** or similar API testing tool (optional, Scalar provides interactive docs)

## Getting Started

### Quick Start with Docker Compose (Recommended)

The easiest way to run the entire application stack (API + SQL Server + Database Seeding) is using Docker Compose.

#### 1. Clone the Repository

```bash
git clone https://github.com/TMatej/AlzaEshopApi.git
cd AlzaEshop.API
```

#### 2. Start the Application

```bash
# Build and start all services
docker compose up --build

# Or run in detached mode (background)
docker compose up --build -d
```

This single command will:
- Start SQL Server 2025 container
- Wait for SQL Server to be healthy
- Run database seed scripts from `./db/seed.sql`
- Build and start the AlzaEshop API container

#### 3. Access the Application

Once all containers are running, the application will be available at:

- **API**: `http://localhost:8080`
- **Scalar UI**: `http://localhost:8080/scalar/v1`
- **OpenAPI Spec v1**: `http://localhost:8080/openapi/v1.json`
- **OpenAPI Spec v2**: `http://localhost:8080/openapi/v2.json`

#### 4. Stop the Application

```bash
# Stop all services
docker compose down

# Stop and remove volumes (clean slate)
docker compose down -v
```

### Docker Compose Services

The `docker-compose.yml` defines three services:

1. **sqlserver**: SQL Server 2025 container
   - Port: `1433`
   - Persistent volume: `sql-data`

2. **db-seed**: Database seeding service
   - Automatically runs `./db/seed.sql` after SQL Server is healthy
   - Exits after seeding completes

3. **alzaeshop-api**: The API application
   - Port: `8080`
   - Automatically connects to SQL Server
   - Runs in Development mode with Scalar documentation enabled

### Manual Development Setup (Without Docker)

If you prefer to run the API locally without Docker:

#### 1. Configure Database Connection

Update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "ProductsConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductCatalog;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Or use the in-memory database option:

```json
{
  "DatabaseSettings": {
    "UseInMemoryDatabase": true
  }
}
```

#### 2. Apply Database Migrations (SQL Server only)

```bash
# Navigate to the project directory
cd AlzaEshop.API

# Apply migrations
dotnet ef database update
```

#### 3. Restore Dependencies

```bash
dotnet restore
```

#### 4. Run the Application

```bash
dotnet run
```

#### 5. Access the Application

- **HTTPS**: `https://localhost:5001` (or check console output for actual port)
- **HTTP**: `http://localhost:5000`
- **Scalar UI**: `https://localhost:5001/scalar/v1`

## API Versioning

The API supports multiple versions accessible through URL segments:

- **Version 1**: `/api/v1/...`
- **Version 2**: `/api/v2/...`

## Running Tests

### Unit Tests

To run all unit tests:

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal
```

## Project Structure

```
AlzaEshop.API/
├── AlzaEshop.API/
│   ├── Common/
│   │   ├── Endpoints/          # Endpoint definitions and registration
│   │   └── ...                 # Shared utilities and extensions
│   ├── Features/               # Feature-based organization
│   ├── appsettings.json        # Application configuration
│   ├── Program.cs              # Application entry point
│   ├── Dockerfile              # Docker configuration
│   └── AlzaEshop.API.csproj
├── db/
│   └── seed.sql                # Database seed scripts
├── docker-compose.yml          # Docker Compose configuration
└── README.md
```

## Configuration

### Environment Variables

The Docker Compose setup uses the following environment variables:

```yaml
- ASPNETCORE_ENVIRONMENT=Development
- ASPNETCORE_URLS=http://+:8080
- ConnectionStrings__ProductsConnection=Server=sqlserver,1433;Database=ProductCatalog;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
```

You can override these in a `.env` file or modify `docker-compose.yml`.

### Logging Configuration

Logging is configured in `appsettings.json`. Default log files are written to the `Logs/` directory.

Adjust log levels as needed:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```
