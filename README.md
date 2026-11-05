# TaskFlow

TaskFlow is a small task-management REST API built with ASP.NET Core and .NET 10. It exposes CRUD endpoints for task items, a health-check endpoint, and an interactive Swagger UI for exploring the API.

> **Current status:** This project is an API foundation. Tasks are stored in process memory, so data is reset whenever the application restarts. No database, authentication, authorization, or frontend application is currently included.

## Features

- RESTful task management endpoints
- Create, read, update, and delete operations
- Automatic JSON request and response handling through ASP.NET Core controllers
- OpenAPI document generation
- Swagger UI for interactive API testing
- Health endpoint for basic availability checks
- HTTPS redirection in the application pipeline
- Multi-stage Docker image based on the official .NET 10 images
- Nullable reference types and implicit global usings enabled

## Technology stack

| Component | Version / implementation |
| --- | --- |
| Runtime and SDK | .NET 10 (`net10.0`) |
| Web framework | ASP.NET Core Web API |
| API style | Controller-based REST API |
| API documentation | `Microsoft.AspNetCore.OpenApi` and Swashbuckle |
| Storage | In-memory static list |
| Containerization | Docker with .NET 10 SDK and ASP.NET runtime images |
| Solution format | `TaskFlow.slnx` |

## Repository structure

```text
TaskFlow/
├── TaskFlow.slnx
├── README.md
└── TaskFlow.Api/
	├── Controllers/
	│   └── TasksController.cs
	├── Models/
	│   └── TaskItem.cs
	├── Properties/
	│   └── launchSettings.json
	├── Dockerfile
	├── TaskFlow.Api.csproj
	├── TaskFlow.Api.http
	├── Program.cs
	├── appsettings.json
	└── appsettings.Development.json
```

### Main files

- `Program.cs` configures controllers, OpenAPI, Swagger, HTTPS redirection, authorization middleware, controller routing, and `/health`.
- `Controllers/TasksController.cs` implements the task CRUD endpoints under `/api/Tasks`.
- `Models/TaskItem.cs` defines the task payload.
- `Properties/launchSettings.json` defines the local HTTP and HTTPS development profiles.
- `Dockerfile` builds and runs the API in a two-stage container image.

## Prerequisites

Install the following tools before running the project locally:

- .NET 10 SDK
- Git, if cloning the repository
- Docker Desktop or another Docker Engine, if using containers

Verify the .NET SDK installation:

```powershell
dotnet --version
```

The installed SDK should support the `net10.0` target framework.

## Run locally

From the repository root, restore dependencies and build the solution:

```powershell
dotnet restore .\TaskFlow.slnx
dotnet build .\TaskFlow.slnx
```

Start the API with the HTTP development profile:

```powershell
dotnet run --project .\TaskFlow.Api\TaskFlow.Api.csproj --launch-profile http
```

The API is available at:

- Base URL: `http://localhost:5088`
- Swagger UI: `http://localhost:5088/swagger`
- Health check: `http://localhost:5088/health`

To run with the HTTPS profile:

```powershell
dotnet run --project .\TaskFlow.Api\TaskFlow.Api.csproj --launch-profile https
```

The HTTPS profile uses:

- HTTPS URL: `https://localhost:7096`
- HTTP URL: `http://localhost:5088`
- Swagger UI: `https://localhost:7096/swagger`

A development HTTPS certificate may need to be trusted on the machine running the application. If necessary, use the .NET certificate tooling:

```powershell
dotnet dev-certs https --trust
```

## API reference

The controller route is `api/[controller]`, so the task resource is available at `/api/Tasks`.

### Task model

```json
{
  "id": 1,
  "title": "Learn Docker",
  "description": "Understand Docker fundamentals",
  "isCompleted": false
}
```

Property names are shown in the JSON casing produced by ASP.NET Core's default JSON configuration. `id` is assigned by the API when a task is created.

### Health check

```http
GET /health
```

Returns HTTP `200 OK` when the API process is running:

```json
{
  "status": "Healthy",
  "message": "TaskFlow API is running."
}
```

### List tasks

```http
GET /api/Tasks
```

Returns HTTP `200 OK` with an array of task items.

### Get a task

```http
GET /api/Tasks/{id}
```

Returns:

- `200 OK` when the task exists
- `404 Not Found` when no task has the specified ID

### Create a task

```http
POST /api/Tasks
Content-Type: application/json
```

Request body:

```json
{
  "title": "Add persistence",
  "description": "Store tasks in a database",
  "isCompleted": false
}
```

Returns HTTP `201 Created`, including the created task and a location link to its GET endpoint. The API assigns the next available integer ID.

### Update a task

```http
PUT /api/Tasks/{id}
Content-Type: application/json
```

Request body:

```json
{
  "title": "Add persistence",
  "description": "Store tasks in a database",
  "isCompleted": true
}
```

Returns:

- `200 OK` with the updated task when the task exists
- `404 Not Found` when no task has the specified ID

The request replaces the editable task fields: `title`, `description`, and `isCompleted`. The route ID remains the task ID.

### Delete a task

```http
DELETE /api/Tasks/{id}
```

Returns:

- `204 No Content` when the task is deleted
- `404 Not Found` when no task has the specified ID

## Example requests with PowerShell

List the initial task:

```powershell
Invoke-RestMethod -Uri "http://localhost:5088/api/Tasks" -Method Get
```

Create a task:

```powershell
$body = @{
	title = "Review API design"
	description = "Check the TaskFlow endpoints"
	isCompleted = $false
} | ConvertTo-Json

Invoke-RestMethod `
	-Uri "http://localhost:5088/api/Tasks" `
	-Method Post `
	-ContentType "application/json" `
	-Body $body
```

Update task `1`:

```powershell
$body = @{
	title = "Learn Docker"
	description = "Understand Docker fundamentals"
	isCompleted = $true
} | ConvertTo-Json

Invoke-RestMethod `
	-Uri "http://localhost:5088/api/Tasks/1" `
	-Method Put `
	-ContentType "application/json" `
	-Body $body
```

Delete task `1`:

```powershell
Invoke-RestMethod -Uri "http://localhost:5088/api/Tasks/1" -Method Delete
```

## Swagger and OpenAPI

Swagger middleware is enabled by the application, including outside the Development environment. When the API is running, open `/swagger` on the selected base URL to:

- Browse the available endpoints
- Inspect request and response schemas
- Execute requests interactively
- Review generated OpenAPI metadata

For automated tooling, the generated OpenAPI document is available through the Swagger middleware's standard JSON endpoint, typically `/swagger/v1/swagger.json`.

## Docker

The Dockerfile uses a two-stage build:

1. `mcr.microsoft.com/dotnet/sdk:10.0` restores, builds, and publishes the application.
2. `mcr.microsoft.com/dotnet/aspnet:10.0` runs the published application.

Build the image from the API project directory because the Dockerfile copies `TaskFlow.Api.csproj` from its build context:

```powershell
Set-Location .\TaskFlow.Api
docker build -t taskflow-api .
```

Run the container and map the container's port `8080` to local port `8080`:

```powershell
docker run --rm -p 8080:8080 --name taskflow-api taskflow-api
```

The containerized API is then available at:

- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`
- Health check: `http://localhost:8080/health`

The container sets `ASPNETCORE_HTTP_PORTS=8080` and exposes port `8080`. Because the container configuration is HTTP-only, use `http://localhost:8080` for the mapped endpoints unless you add separate TLS termination or HTTPS configuration.

## Configuration

Application configuration is currently limited to logging and allowed hosts:

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning"
	}
  },
  "AllowedHosts": "*"
}
```

Environment-specific settings can be placed in `appsettings.Development.json` or supplied using standard ASP.NET Core configuration providers and environment variables.

## Data and concurrency limitations

Tasks are held in a static in-memory `List<TaskItem>`:

- Data is lost when the process stops or restarts.
- Data is not shared between multiple application instances.
- There is no persistence, migration, validation layer, or transaction support.
- The current list is mutable and is intended for demonstration or early development rather than production workloads.
- IDs are calculated from the current list and are not suitable as a durable identity strategy for concurrent production requests.

A production version should replace the static list with a persistence abstraction and a database-backed implementation, then add validation, error handling, and concurrency controls.

## Development workflow

1. Restore dependencies:
   ```powershell
   dotnet restore .\TaskFlow.slnx
   ```
2. Build the solution:
   ```powershell
   dotnet build .\TaskFlow.slnx
   ```
3. Run the API using the HTTP or HTTPS launch profile.
4. Exercise endpoints through Swagger, `TaskFlow.Api.http`, or PowerShell.
5. Rebuild before committing changes.

## Testing

No test project is currently included in the solution. Until automated tests are added, manually verify at least the following flows:

- `GET /health` returns `200 OK`.
- `GET /api/Tasks` returns the seeded task.
- `GET /api/Tasks/1` returns the seeded task.
- `POST /api/Tasks` returns `201 Created` and an assigned ID.
- `PUT /api/Tasks/{id}` updates the task.
- `DELETE /api/Tasks/{id}` returns `204 No Content`.
- Requests for missing IDs return `404 Not Found`.

## Potential next steps

The current API can be extended with:

- Database persistence, such as Entity Framework Core
- Request and response DTOs instead of using the domain model directly
- Model validation and consistent problem-details responses
- Automated unit and integration tests
- Authentication and authorization
- Filtering, sorting, paging, and task status transitions
- Structured logging and production health checks
- CI/CD workflows and deployment configuration

## License

No license file is currently included in the repository. Add a license before distributing the project or accepting external contributions.
