# DataSneeq - Dynamic Excel Data Upload & Mapping Portal

A full-stack application for uploading Excel files and inserting data into PostgreSQL databases with dynamic schema discovery, automatic column matching, foreign key resolution, and batch inserts.

## Tech Stack

**Backend:** .NET 8 Web API, ClosedXML, Npgsql, EF Core (SQLite for app storage)
**Frontend:** Next.js 16, TypeScript, Tailwind CSS, shadcn/ui, TanStack Query

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- PostgreSQL (target database for uploads)

## Getting Started

### Backend

```bash
cd backend
dotnet build
dotnet run --project src/DataSneeq.Api
```

The API starts at `http://localhost:5041`. Swagger UI is available at `http://localhost:5041/swagger`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The app starts at `http://localhost:3000`.

## Features

- Excel file upload with drag-and-drop (.xlsx/.xls)
- Automatic Excel column detection
- Dynamic database connection via connection string
- Schema discovery (tables, columns, types, PKs, FKs)
- Intelligent column auto-matching (exact, normalized, abbreviation, fuzzy)
- Interactive column mapping UI with dropdowns
- Foreign key lookup resolution with auto-create
- Data validation (required fields, types, dates, FK references)
- Data preview before insert
- Batch insert for high performance
- Reusable mapping templates

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/upload/excel` | Upload Excel file |
| POST | `/api/schema/connect` | Test database connection |
| GET | `/api/schema/tables` | List database tables |
| GET | `/api/schema/tables/{table}/columns` | Get table schema |
| POST | `/api/mapping/suggest` | Auto-suggest column mappings |
| POST | `/api/upload/preview` | Preview processed data |
| POST | `/api/upload/commit` | Insert data into database |
| POST | `/api/mapping-templates` | Save mapping template |
| GET | `/api/mapping-templates` | List saved templates |
| DELETE | `/api/mapping-templates/{id}` | Delete template |

## Docker Deployment (with nginx)

DataSneeq expects to run alongside nginx in Docker. Both must share the `portal-net` network.

1. Create the shared network (once):
   ```bash
   docker network create portal-net
   ```

2. Add your nginx service to `portal-net` in its compose file:
   ```yaml
   services:
     nginx:
       # ...
       networks:
         - portal-net
   networks:
     portal-net:
       external: true
   ```

3. Include the DataSneeq location blocks from `nginx-datasneeq.conf` in your nginx config (before `location /`).

4. Start DataSneeq:
   ```bash
   docker compose up -d
   ```

## Architecture

```
backend/
  src/
    DataSneeq.Api/              # Controllers, middleware, Program.cs
    DataSneeq.Application/      # Services, DTOs, interfaces, validators
    DataSneeq.Domain/           # Domain models, enums
    DataSneeq.Infrastructure/   # DB providers, Excel parsing, EF persistence

frontend/
  src/
    app/                        # Next.js pages
    components/                 # UI components (wizard, upload, mapping, preview)
    hooks/                      # TanStack Query hooks
    lib/                        # API client, utilities
    types/                      # TypeScript interfaces
```
