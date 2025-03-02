# ActionSpecAPI (ASA)

## Overview

ActionSpecAPI (ASA) is a declarative framework for building web APIs in .NET, inspired by GitHub Actions workflow syntax. ASA allows developers to define API endpoints and their behavior using YAML configuration files rather than writing imperative code.

The core philosophy of ASA is to separate the "what" from the "how" in API development:

-   **What**: Defined in YAML configuration files (endpoints, steps, data flow)
-   **How**: Implemented in reusable modules (business logic, data access, etc.)

This approach makes APIs easier to understand, maintain, and evolve while promoting code reuse across endpoints and even across different APIs.

## Key Features

-   **Declarative API Definition**: Define endpoints and their behavior in YAML
-   **Modular Architecture**: Break down API functionality into reusable modules
-   **Pipeline Processing**: Sequential execution of steps with data passing between them
-   **Variable Substitution**: Reference data from requests, configs, or previous steps
-   **Conditional Logic**: Control flow based on conditions
-   **Extensible Module System**: Create and share custom modules via NuGet packages

## Project Structure

```
ASA/
├── Core/       # Data models, interfaces, expression evaluation, context management, step execution
├── Host/       # ASP.NET Core integration, DI extensions for ASA 
└── Modules/    # Core modules
```

## Core Concepts

### ActionSpec

The main specification file that describes your API. It includes:

-   Basic metadata (name, description, version)
-   Reference to OpenAPI spec
-   List of endpoints with their steps

```
name: "Customer Service API"
description: "API for managing customer data"
version: "1.0.0"

endpoints:
  - path: "/customers/{id}"
    method: GET
    description: "Retrieve customer by ID"
    steps:
      # Steps defined here
```

### Endpoints

Each endpoint is defined with a path, HTTP method, description, and a sequence of steps to execute.

### Steps

Steps are the building blocks of your API logic. Each step:

-   References a module implementation
-   Provides configuration parameters
-   Can produce output for subsequent steps
-   Can have conditions for execution

```
steps:
  - name: "fetch-customer"
    uses: "ASA.Modules.Data/DbClient@2.1.0"
    with:
      connection: "${{ config.db.connection }}"
      query: "SELECT * FROM customers WHERE id = ${{ request.path.id }}"
    output: "customer"
```

### Modules

Modules are the reusable components that implement specific functionality:

-   Database operations
-   HTTP requests
-   Caching
-   Response formatting
-   Authentication/authorization
-   Custom business logic

Each module has a unique identifier and version, following the pattern: `[Package]/[Module]@[Version]`

### Context and Expression Language

ASA provides a powerful expression language for referencing data across your API:

-   `${{ request.path.id }}` - Access path parameters
-   `${{ request.query.filter }}` - Access query parameters
-   `${{ steps.fetch-customer.output.customer }}` - Access output from previous steps
-   `${{ config.api.timeout }}` - Access configuration values

## Module System

### Module Interface

All modules implement the `IModule` interface:

```
public interface IModule
{
    string Name { get; }
    string Version { get; }
    Task<StepOutput> ExecuteAsync(Dictionary<string, object> parameters, AsaExecutionContext context);
}
```

### Module Registration

Modules are registered via ioc:

```
// Custom modules
builder.Services.AddSingleton<IModule, CustomDataProcessor>();
```

### Built-in Modules

ASA comes with several built-in modules:

#### Echo 

Returns a message or "Hello, World!".

```
- name: "hello-world"
  uses: "asa.modules/echo@1.0.0"
  with:
    message: Happy Holidays!
```

#### Response Formatter

Formats and sends HTTP responses.

```
- name: "format-response"
  uses: "asa.modules/response-formatter@1.0.0"
  with:
    status: 200
    content-type: "application/json"
    body: "${{ steps.get-data.data.result }}"
```

## Getting Started

### Installation

[Todo: Create nuget packages]
```
dotnet add package ASA.Host
dotnet add package ASA.modules
# Add other module packages as needed
```

### Basic Setup

```
var builder = WebApplication.CreateBuilder(args);

// Add ASA services
builder.Services.AddActionSpecApi("./asa-spec.yaml");

// Custom modules
builder.Services.AddSingleton<IModule, CustomDataProcessor>();

var app = builder.Build();

// Configure middleware
app.UseActionSpecApi();

app.Run();
```

### Creating Custom Modules

1.  Implement the `IModule` interface:

```
public class CustomDataProcessor : IModule
{
    public string Name => "custom/data-processor";
    public string Version => "1.0.0";

    public async Task<StepOutput> ExecuteAsync(Dictionary<string, object> parameters, AsaExecutionContext context)
    {
        // Process parameters
        var inputData = parameters.GetValueOrDefault("input", null);
        
        // Implement your logic
        var result = ProcessData(inputData);
        
        // Return output
        return new StepOutput
        {
            Success = true,
            Data = result
        };
    }
    
    private object ProcessData(object input)
    {
        // Your custom processing logic
        return input;
    }
}
```

2.  Register your module:

```
// In Program.cs
builder.Services.AddSingleton<IModule, CustomDataProcessor>();
```

## Advanced Features

### Conditional Execution

Steps can be conditionally executed using the `if` property:

```
- name: "cache-result"
  if: "${{ steps.check-cache.output.hit == false }}"
  uses: "ASA.Modules.Cache/RedisCache@1.0.0"
  with:
    connection: "${{ config.redis.connection }}"
    key: "user:${{ request.path.id }}"
    value: "${{ steps.fetch-user.output.user }}"
    ttl: 3600
```

## Benefits

-   **Reduced Boilerplate**: Focus on business logic, not plumbing code
-   **Consistency**: Standardized approach to common patterns
-   **Visibility**: API behavior is clearly documented in YAML
-   **Maintainability**: Changes can often be made without code modification
-   **Reusability**: Share modules across endpoints and projects
-   **Flexibility**: Swap implementations without changing API contracts

## Roadmap

-   Dynamic endpoint registration
-   Error handling and request validation modules
-   Logging and metrics modules
-   Authentication/authorization modules
-   Database, Caching, ServiceBus, HttpClient modules
-   Transaction support
-   OpenAPI Integration: Automatic integration with Swagger/OpenAPI
-   Testing framework for ASA specifications
-   Visual editor for ASA specifications