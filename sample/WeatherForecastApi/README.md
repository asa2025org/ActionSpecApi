# SampleApi - ActionSpecAPI Demo

This sample project demonstrates how to use the ActionSpecAPI (ASA) framework to create a functional weather forecast API.

## Overview

SampleApi showcases how to build an API using the declarative approach of ASA rather than traditional imperative code. The project implements a simple weather forecast service that generates random weather data for the next five days.

## Project Structure

```
SampleApi/
├── Program.cs                   # Application entry point
├── WeatherGenerator.cs          # Custom Module to generate weather data
└── asa.yaml                     # ASA specification file
```

## Key Files

### Program.cs

This file configures the ASP.NET Core application and sets up ASA:

### asa.yaml

This YAML file defines the API endpoints and their behavior:

## How It Works

1.  The ASA middleware processes incoming requests.
2.  For `/weatherforecast` GET requests:
    -   The `WeatherGenerator` module creates random weather data.
    -   The `ResponseFormatter` module formats the data as JSON and sends it to the client.
3.  The entire flow is defined in the YAML file, not in code.

## Getting Started

### Prerequisites

-   .NET 8.0 SDK or later
-   ASA NuGet packages (installed via project references)

### Running the Project

1.  Clone the repository
2.  Navigate to the SampleApi directory
3.  Run the application:

```
dotnet run
```

4.  Open a browser and test the API by making a GET request to [https://localhost:5264/weatherforecast](https://localhost:5264/weatherforecast)

## Extending the Sample

Here are some ideas for extending this sample project:

1.  Add a new endpoint that accepts parameters (e.g., location, date range)
2.  Implement data persistence using a database module
3.  Add caching for weather forecasts
4.  Implement authentication/authorization
5.  Add request validation

## Additional Resources

For more information about ASA, refer to the ASA project README.md file or visit the ASA documentation website.
