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
│       │   ├── Processing/
│       │   └── Jobs/
│       │
│       ├── Application/
│       │   ├── Abstractions/
│       │   └── Services/
│       │
│       ├── Domain/
│       │
│       ├── Infrastructure/
│       │   ├── Storage/
│       │   ├── Processing/
│       │   └── Persistence/
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
- **Application**: contratos (`Abstractions`) e implementaciones de servicios de aplicación (`Services`) que orquestan la lógica de negocio.
- **Domain**: entidades y lógica de dominio, sin dependencias externas.
- **Infrastructure**: implementaciones concretas de acceso a almacenamiento (`Storage`), procesamiento (`Processing`) y persistencia (`Persistence`).
- **Common**: utilidades transversales compartidas por el resto de capas.

> No se crean todavía carpetas como `DTOs`, `Factories`, `Helpers`, `Managers`, `Mappers`, `Validators`, etc. Se crearán únicamente cuando exista una necesidad real.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)

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
