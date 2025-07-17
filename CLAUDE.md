# CLAUDE.md

This file provides architectural guidance to Claude Code when working with this codebase.

## Project Intent & Architecture

### Core Purpose
Remote connection (e.g. SSH) management CLI with rich terminal UI. Users create, manage, and connect to preconfigured profiles through an interactive menu system.

### Key Architectural Decisions

**ErrorOr Pattern Over Exceptions**
- Business logic failures return `ErrorOr<T>` instead of throwing exceptions
- Exceptions reserved for truly exceptional cases (system failures, configuration errors)
- Consistent error handling with structured error codes and messages

**Core/CLI Separation**
- **Core**: Business logic, data access, validation - no UI dependencies
- **CLI**: User interface, SSH execution, system interaction - consumes Core services
- Clean separation enables potential future UIs (web, desktop) without Core changes

**SQLite + Entity Framework Core**
- Simple, file-based persistence requiring no external dependencies
- Database location tied to CLI working directory for proper data isolation

**Rich Terminal UI**
- Spectre.Console for interactive menus, forms, and real-time feedback
- Menu-driven interface over command-line arguments for better UX
- Graceful error presentation with color coding and structure

## Development Patterns

### Adding New Services
- Organize by feature in `Services/FeatureName/` folders
- Return `ErrorOr<T>` from all business methods
- Use `IValidationService` for input validation
- Default to `internal` visibility, make `public` only when needed across projects
- Co-locate interfaces with implementations in same file for internal services, use an external file for the interface
  if it is public

### Adding New Entities
- Create domain entity as simple POCO in `Models/`
- Separate EF configuration in `Data/Configuration/`
- Create request DTO (e.g. `CreateConnectionProfileRequest`) with a FluentValidation validator co-located in the same file
  - if create and update requests only differ minimally, use a base class (and base validator) for shared properties
- Create response DTO with constructor accepting domain entity
- Implement business logic in dedicated service

### Error Handling Strategy
- **Validation Errors**: Use FluentValidation with ErrorOr conversion
- **Business Logic Errors**: Check business rules in service methods
- **Not Found**: Return `Error.NotFound` with descriptive messages
- **Conflicts**: Return `Error.Conflict` for uniqueness violations
- **Error Codes**: Use constants from `ErrorCodes` class

### Interface Visibility Guidelines
- `internal interface` by default for project-internal services
- `public interface` when service crosses project boundaries (e.g., Core → CLI)
- Example: `IConnectionProfilesService` is public because CLI consumes it

## Key Conventions

### Service Organization
- Feature-based folders: `Services/ConnectionProfiles/`, `Services/Ssh/`
- Interface and implementation in same file if internal, separate file for public interfaces
- Base classes for shared validation logic (e.g., `BaseConnectionProfileRequest`)
- Dependency injection configured with extension methods in `DependencyInjection.cs` for each project (so internal services can be registered without exposing)

### Validation Strategy
- FluentValidation for input structure validation
- Business logic validation (uniqueness, existence) in service methods
- Error codes from `ErrorCodes` constants for consistency
- Inheritance-based validation for shared rules

### Database Patterns
- Migrations in `Data/Migrations/` subfolder
  - do *not* manually edit migration files, use `dotnet ef migrations add` to generate them
- Entity configurations separate from models
- Use `AsNoTracking()` for read operations
- Project DTOs in LINQ queries for performance

### CLI Patterns
- Spectre.Console for all UI interactions
- Menu-driven navigation with `SelectionPrompt<T>`
- Form-based input with validation feedback
- Status indicators for async operations

## Development Workflow

### Essential Commands
```bash
# Run application (correct working directory critical for database)
cd src/Cli && dotnet run

# Entity Framework migrations
dotnet ef migrations add <name> --project src/Core --startup-project src/Cli --output-dir Data/Migrations
dotnet ef database update --project src/Core --startup-project src/Cli

# Code formatting (enforced in CI)
dotnet tool restore
dotnet csharpier format .
```

### Code Quality
- Format code after finishing a task with code changes: `dotnet csharpier format .`
- All packages managed in `Directory.Packages.props`
  - do not add new packages without discussion
  - use the dotnet CLI for package management, do not manually update `Directory.Packages.props` or `*.csproj` files
- Use structured logging with parameters: `_logger.LogDebug("Message {Parameter}", value)`

## Decision Context

### When to Make Interfaces Public
- Service used across project boundaries → `public interface`
- Service only used within same project → `internal interface`
- Example: `IConnectionProfilesService` is public (Core → CLI), `IValidationService` is internal (Core only)

### When to Add Dependencies
- All packages must be added to `Directory.Packages.props`
- Prefer existing patterns over new dependencies
- Consider impact on self-contained deployment

### Database Migration Considerations
- Database preserves user data during application updates
- Schema changes require careful migration planning
- Always test migrations with existing data

### Constants Visibility
- `internal static class` for project-internal constants
- `public static class` when used across projects (e.g., `ApplicationConstants` for validation limits)
- Example: `ErrorCodes` internal, `ApplicationConstants` public for CLI validation

---

*This document focuses on architectural intent and extensibility patterns. For specific implementation details, refer to the codebase itself.*