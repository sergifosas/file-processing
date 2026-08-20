# FileProcessing

File processing API.

## Project structure

```
FileProcessing/
│
├── src/
│   └── FileProcessing.Api/
│       │
│       ├── Features/
│       │   ├── Files/
│       │   │   └── Upload/
│       │   │       └── UploadEndpoint.cs
│       │   ├── Processing/
│       │   └── Jobs/
│       │
│       ├── Application/
│       │   ├── Abstractions/
│       │   └── Services/
│       │       ├── FileProcessingService.cs
│       │       ├── IFileStorage.cs
│       │       └── IFileRepository.cs
│       │
│       ├── Domain/
│       │   └── Files/
│       │       └── StoredFile.cs
│       │
│       ├── Infrastructure/
│       │   ├── Storage/
│       │   │   └── LocalFileStorage.cs
│       │   ├── Processing/
│       │   └── Persistence/
│       │       ├── FileProcessingDbContext.cs
│       │       ├── FileRepository.cs
│       │       └── Configurations/
│       │           └── StoredFileConfiguration.cs
│       │
│       ├── Common/
│       │
│       ├── Program.cs
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── tests/
│   ├── FileProcessing.UnitTests/
│   └── FileProcessing.IntegrationTests/
│
├── .gitignore
├── README.md
└── FileProcessing.slnx
```

### Conventions

- **Features**: groups the functionality by use case (endpoints and feature-specific requests/responses).
- **Application**: application contracts and services (`Services`) that orchestrate the business logic. `FileProcessingService` depends only on the `IFileStorage` and `IFileRepository` abstractions.
- **Domain**: entities and domain logic, with no external dependencies.
- **Infrastructure**: concrete implementations for storage (`Storage` → `LocalFileStorage`), processing (`Processing`) and persistence (`Persistence` → EF Core/PostgreSQL).
- **Common**: cross-cutting utilities shared across the rest of the layers.

> Folders such as `DTOs`, `Factories`, `Helpers`, `Managers`, `Mappers`, `Validators`, etc. are not created yet. They will only be created when there is a real need.

### Flow of file uploads

```
Features (UploadEndpoint receives the IFormFile)
    │
    ▼
Application (FileProcessingService orchestrates the use case)
    │
    ├── IFileStorage      → Infrastructure/Storage   → LocalFileStorage (disk)
    └── IFileRepository   → Infrastructure/Persistence → FileRepository (PostgreSQL)
```

The endpoint receives the ASP.NET Core `IFormFile` and turns it into a `Stream` plus metadata. `FileProcessingService` delegates physical storage to `IFileStorage` and metadata persistence to `IFileRepository`, so the Application layer depends on neither ASP.NET Core nor EF Core.

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL (to run locally). The `Testing` environment uses the EF Core in-memory database provider so that tests do not depend on an external server.

## How to run

```bash
dotnet restore
dotnet build
dotnet run --project src/FileProcessing.Api
```

## How to test

```bash
dotnet test
```