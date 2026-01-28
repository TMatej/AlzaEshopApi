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

- **Database Server**:
  - SQL Server 2022 or later (recommended)
  - In memory option by adjusting section in `appsettings.json` or `appsettings.Development.json`:
  
  ```json
  {
    "DatabaseSettings": {
      "UseInMemoryDatabase": true
    },
  }
  ```

### Optional Tools

- **Visual Studio 2022** (17.8 or later) or **JetBrains Rider** (2023.3 or later)
- **Visual Studio Code** with C# extension
- **Git** for version control
- **Bruno** or similar API testing tool (optional, Scalar provides interactive docs)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/TMatej/AlzaEshopApi.git
cd AlzaEshop.API
```

### 2. Configure Database Connection

Update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AlzaEshop;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Apply Database Migrations

```bash
# Navigate to the project directory
cd AlzaEshop.API

# Create and apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Run the Application

#### Using .NET CLI:

```bash
dotnet run
```

#### Using Visual Studio:
1. Open `AlzaEshop.API.sln`
2. Press `F5` or click "Start Debugging"

### 6. Access the Application

Once running, the application will be available at:

- **HTTPS**: `https://localhost:5001` (or check console output for actual port)
- **HTTP**: `http://localhost:5000`

#### API Documentation (Development Only)

- **Scalar UI**: `https://localhost:5001/scalar/v1`
- **OpenAPI Spec v1**: `https://localhost:5001/openapi/v1.json`
- **OpenAPI Spec v2**: `https://localhost:5001/openapi/v2.json`

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

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure

```
AlzaEshop.API/
├── Common/
│   ├── Endpoints/          # Endpoint definitions and registration
│   └── ...                 # Shared utilities and extensions
├── Features/               # Feature-based organization
├── appsettings.json        # Application configuration
├── Program.cs              # Application entry point
└── AlzaEshop.API.csproj
```

## Configuration

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
