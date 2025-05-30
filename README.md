# ⚡SmartCharging API

> 💡 A practical api sample based on `Vertical Slice Architecture`, `DDD`, `Minimal APIs` and `.Net 9`.

## Table of Contents

- [⚡SmartCharging API](#smartcharging-api)
  - [Table of Contents](#table-of-contents)
  - [Libraries](#libraries)
  - [Getting Started](#getting-started)
    - [1. Install .NET Core 9.0](#1-install-net-core-90)
    - [2. Install an IDE](#2-install-an-ide)
    - [3. Install `make`](#3-install-make)
    - [4. Set up and Start the Infrastructure](#4-set-up-and-start-the-infrastructure)
    - [5. Open the Solution](#5-open-the-solution)
    - [6. Run and Debug the Project](#6-run-and-debug-the-project)
      - [Using IDE](#using-ide)
      - [Using Make Commands](#using-make-commands)
    - [7. Host the Application](#7-host-the-application)
    - [8. Running Tests](#8-running-tests)
  - [Application Structure](#application-structure)
    - [Folder Structure](#folder-structure)
  - [Domain Model](#domain-model)
    - [Entities](#entities)
      - [Group](#group)
      - [Charge Station](#charge-station)
      - [Connector](#connector)
    - [Business Rules](#business-rules)
    - [Building and Running the Application](#building-and-running-the-application)

## Libraries

- ✔️ **[`.NET 9`](https://dotnet.microsoft.com/download)** - .NET Framework and .NET Core, including ASP.NET and ASP.NET Core
- ✔️ **[`Asp.Versioning`](https://github.com/dotnet/aspnet-api-versioning)** - A set of libraries for ASP.NET Core to manage API versioning
- ✔️ **[`Microsoft.AspNetCore.OpenApi`](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi)** - .NET 9 Built-in OpenAPI support for ASP.NET Core
- ✔️ **[`Microsoft.EntityFrameworkCore.Sqlite`](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/?tabs=dotnet-core-cli)** - Entity Framework Core provider for SQLite
- ✔️ **[`Respawn`](https://github.com/jbogard/Respawn)** - Library to reset database state for integration testing scenarios
- ✔️ **[`Testcontainers.PostgreSql`](https://github.com/testcontainers/testcontainers-dotnet)** - Integration testing with lightweight PostgreSQL containers
- ✔️ **[`EFCore.NamingConventions`](https://github.com/efcore/EFCore.NamingConventions)** - A plugin that enforces specific naming conventions in Entity Framework Core
- ✔️ **[`Npgsql.EntityFrameworkCore.PostgreSQL`](https://github.com/npgsql/efcore.pg)** - Entity Framework Core provider for PostgreSQL by Npgsql
- ✔️ **[`Swashbuckle.AspNetCore.SwaggerUI`](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** - Provides a Swagger UI implementation for ASP.NET Core
- ✔️ **[`Scalar.AspNetCore`](https://github.com/scalar/scalar/blob/main/integrations/aspnetcore/README.md)** - Provides additional OpenAPI/Swagger-related features
- ✔️ **[`Humanizer.Core`](https://github.com/Humanizr/Humanizer)** - A library for working with strings, dates, numbers, and times in a more human-readable way
- ✔️ **[`Scrutor`](https://github.com/khellang/Scrutor)** - Extensions for Microsoft.Extensions.DependencyInjection to allow easy assembly scanning
- ✔️ **[`xUnit`](https://xunit.net/)** - Popular testing framework for .NET applications
- ✔️ **[`Bogus`](https://github.com/bchavez/Bogus)** - A powerful .NET library for generating fake data
- ✔️ **[`Shouldly`](https://github.com/shouldly/shouldly)** - Simple and human-readable assertions for .NET tests
- ✔️ **[`NSubstitute`](https://github.com/nsubstitute/NSubstitute)** - A friendly .NET library for creating substitutes to use in testing

## Getting Started

Follow this guide to set up and run the project on your development environment.

### 1. Install .NET Core 9.0

Download and install the latest version of .NET Core 9.0 SDK from the official [.NET download page](https://dotnet.microsoft.com/download).

Ensure `dotnet` is available in your terminal or command prompt by running:

```bash
dotnet --version
```

### 2. Install an IDE

You can use any of the following IDEs for development:

- **[JetBrains Rider](https://www.jetbrains.com/rider/)** (Recommended)
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)**
- **[Visual Studio Code](https://code.visualstudio.com/)**

Ensure the IDE includes support for .NET Core and plugins for C#.

### 3. Install `make`

The project uses a `Makefile` for common tasks. Install `make` for your platform:

- **Windows**: Use [Chocolatey](https://chocolatey.org/install) or other package managers:

```bash
choco install make
```

- **MacOS**: Install via Homebrew:

```bash
brew install make
```

- **Linux**: Install with apt or your distribution's package manager:

```bash
sudo apt install make
```

Test `make` installation by running:

```bash
make --version
```

### 4. Set up and Start the Infrastructure

This project uses a PostgreSQL database running in a Docker container. Start the infrastructure using `make`:

```bash
# Start docker-compose
make run-docker-compose

# Stop docker-compose
make stop-docker-compose
```

This command will run the PostgreSQL docker container using docker-compose. Ensure Docker is installed and running on your machine.

### 5. Open the Solution

Open the solution file [SmartChargingApplication.slnx](./SmartChargingApplication.slnx) in your preferred IDE (e.g., Rider or Visual Studio).

Project structure:

- **`src/`**: Contains the main project code.
- **`tests/`**: Contains test projects (Unit, Integration, and End-to-End tests).

### 6. Run and Debug the Project

There are multiple ways to run the project:

#### Using IDE

- Open the solution [SmartChargingApplication.slnx](./SmartChargingApplication.slnx) in your IDE (e.g., Rider or Visual Studio).
- Run or debug the project directly within the IDE.

#### Using Make Commands

Alternatively, you can use the terminal commands below to build, run, or test the project:

- Run the project:

```bash
make run
```

- Run all tests:

```bash
make test
```

### 7. Host the Application

After running the project, the API will be available at:

- **API**: [http://localhost:5000](http://localhost:5000)
- **Swagger UI**: [https://localhost:4000/swagger](https://localhost:4000/swagger)
- **Scalar**: [https://localhost:4000/scalar/v1](https://localhost:4000/scalar/v1)

You can use these endpoints for exploring and testing the application.

### 8. Running Tests

You can run tests directly from the IDE or using commands:

- Run all tests:

```bash
make test
```

- Alternatively, you can execute specific test projects like unit or integration tests:

```bash
make unit-tests
make integration-tests
make end-to-end-tests
```

With these steps, you're ready to get started the project effectively.

## Application Structure

In this project I used [vertical slice architecture](https://jimmybogard.com/vertical-slice-architecture/) and
[feature folder structure](http://www.kamilgrzybek.com/design/feature-folders/):

- We treat each request as a distinct use case or slice, encapsulating and grouping all concerns from front-end to back.
- When we are adding or changing a feature in an application in traditional n-tier architecture, we are typically touching many different "layers" in an application. We are changing the user interface, adding fields to models, modifying validation, and so on. Instead of coupling across a layer, we couple vertically along a slice, and each change affects only one slice.
- We `minimize coupling` between slices and `maximize cohesion` within a slice, ensuring that related code is grouped together logically and independently.
- With this approach, each of our vertical slices can decide for itself how to best fulfill the request. New features only add code, and we're not changing shared code and worrying about side effects.
- By grouping all code related to a feature into a single slice, the architecture improves maintainability and makes it easier to understand and modify individual features without affecting others.
- Testing becomes more straightforward, as each slice encapsulates all logic related to its feature, enabling isolated and effective testing.

![](./assets/vertical-slice-architecture.jpg)

### Folder Structure

The project is organized into a folder structure that aligns with the principles of **Vertical Slice Architecture** and **Feature Folder Structure**:

```
/
├── Groups/                          # Related to the "Groups" aggregate in the domain
│   ├── Contracts/                   # Interfaces for the domain and contracts for integration
│   ├── Dtos/                        # Data Transfer Objects specific to Groups
│   ├── Features/                    # Contains vertical slices (use-cases) for Groups
│   │   ├── AddChargeStation/        # Slice for "Add Charge Station" functionality
│   │   ├── AddStationConnectors/    # Slice for adding connectors to a charge station
│   │   ├── CreateGroup/             # Slice for "Create Group" functionality
│   │   ├── GetGroupsByPage/         # Slice for retrieving groups with pagination
│   │   ├── GroupGetById/            # Slice for retrieving a single group by ID
│   │   ├── RemoveChargeStation/     # Slice for removing a charge station from a group
│   │   ├── RemoveGroup/             # Slice for deleting a group
│   │   ├── RemoveStationConnectors/ # Slice for removing connectors from a charge station
│   │   ├── UpdateChargeStationName/ # Slice for updating a charge station's name
│   │   ├── UpdateConnectorCurrentInAmps/ # Slice for updating a connector's current
│   │   ├── UpdateGroup/             # Slice for updating a group's properties
│   ├── Models/                      # Entities, AggregateRoots, and ValueObjects for Groups
│   ├── GroupsConfig.cs              # Configuration settings for Groups (e.g., Registering handlers and mapping endpoints)
│   ├── GroupsMappings.cs            # Mapping configurations specific to Groups
├── Shared/                          # Shared functionality to avoid duplication
│   ├── Application/                 # Application-level shared resources
│   │   ├── Data/                    # Data access and persistence logic (DBContext, EF Configurations,...)
│   │   ├── Extensions/              # Helpful extension methods
│   ├── BuildingBlocks/              # Cross-cutting concerns and infrastructure setup
```

- **Groups**: One of our business-related aggregates in the domain. Features and concerns that belong exclusively to this aggregate are encapsulated here.

  - **Contracts**: Contains interfaces related to this domain.
  - **Dtos**: Holds Data Transfer Objects for data communication.
  - **Features**: Each business functionality or use case is split into separate slices. Each slice consists of commands/queries, their handlers, endpoints, and validation logic.
  - **Models**: Contains Entities, AggregateRoot, and ValueObjects relevant to the domain.
  - **GroupsConfig.cs**: Configuration for setting up the Groups domain with underlying infrastructure (e.g.,
    Registering handlers and mapping endpoints).
  - **GroupsMappings.cs**: Contains mapping logic specific to this aggregate.

- **Shared**: Contains reusable resources and infrastructure functionality.
  - **Application**: Data persistence and extension methods for the application.
  - **BuildingBlocks**: Cross-cutting concerns, such as database configurations, OpenAPI settings, and validation utilities.

## Domain Model

### Entities

#### Group

- Unique Identifier (immutable)
- Name (mutable)
- Capacity in Amps (integer, > 0, mutable)
- Contains multiple Charge Stations

#### Charge Station

- Unique Identifier (immutable)
- Name (mutable)
- Contains 1-5 Connectors
- Always belongs to exactly one Group

#### Connector

- Integer Identifier (1-5, unique within a Charge Station)
- Max Current in Amps (integer, > 0, mutable)
- Always belongs to a Charge Station

### Business Rules

1. Groups, Charge Stations, and Connectors can be created, updated, and removed.
2. When a Group is removed, all its Charge Stations (and their Connectors) are automatically removed.
3. Only one Charge Station can be added or removed from a Group in a single operation.
4. A Charge Station can only belong to one Group at a time and cannot exist without a Group.
5. A Connector cannot exist without a Charge Station.
6. The Max Current in Amps of Connectors can be updated.
7. A Group's Capacity in Amps must always be greater than or equal to the sum of all Max Current values from all Connectors in the Group.
8. Operations that violate any of these rules are rejected.

### Building and Running the Application

The project includes a Makefile with several useful commands:

1. Build the project:

```bash
make build
```

2. Run the application:

```bash
make run
```

3. Run tests:

```bash
# run all tests
make test

# run unit tests
make unit-tests

# run integration tests
make integration-tests

# run end-to-end tests
make end-to-end-tests
```

4. Format code using CSharpier:

```bash
make format
```
