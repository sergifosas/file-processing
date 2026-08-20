# FileProcessing

API para el procesamiento de archivos.

## Estructura del proyecto

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

### Convenciones

- **Features**: agrupa la funcionalidad por caso de uso (endpoints, requests/responses propios de cada feature).
- **Application**: contratos y servicios de aplicación (`Services`) que orquestan la lógica de negocio. `FileProcessingService` depende únicamente de las abstracciones `IFileStorage` e `IFileRepository`.
- **Domain**: entidades y lógica de dominio, sin dependencias externas.
- **Infrastructure**: implementaciones concretas de almacenamiento (`Storage` → `LocalFileStorage`), procesamiento (`Processing`) y persistencia (`Persistence` → EF Core/PostgreSQL).
- **Common**: utilidades transversales compartidas por el resto de capas.

> No se crean todavía carpetas como `DTOs`, `Factories`, `Helpers`, `Managers`, `Mappers`, `Validators`, etc. Se crearán únicamente cuando exista una necesidad real.

### Flujo de subida de archivos

```
Features (UploadEndpoint recibe el IFormFile)
    │
    ▼
Application (FileProcessingService orquesta el caso de uso)
    │
    ├── IFileStorage      → Infrastructure/Storage   → LocalFileStorage (disco)
    └── IFileRepository   → Infrastructure/Persistence → FileRepository (PostgreSQL)
```

El endpoint recibe el `IFormFile` de ASP.NET Core y lo transforma a un `Stream` más metadatos. `FileProcessingService` delega el guardado físico en `IFileStorage` y el registro de metadatos en `IFileRepository`, de modo que `Application` no depende ni de ASP.NET Core ni de EF Core.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL (para ejecutar en local). En el entorno `Testing` se usa la base de datos en memoria de EF Core para que los tests no dependan de un servidor externo.

## Cómo ejecutar

```bash
dotnet restore
dotnet build
dotnet run --project src/FileProcessing.Api
```

## Cómo probar

```bash
dotnet test
```