# 🛠 DevResource API - Technology Stack

Bu proje, yazılımcıların kaynaklarını kategorize etmek ve yönetmek amacıyla modern .NET teknolojileri kullanılarak geliştirilmiştir.

## Core Technologies
* **Runtime:** .NET 9.0 SDK
* **Language:** C# 12
* **Framework:** ASP.NET Core Web API

## Data & Persistence
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core 8.0
* **Database Provider:** Npgsql.EntityFrameworkCore.PostgreSQL

## API Features & Middleware
* **API Documentation:** Swagger / OpenAPI
* **Partial Updates:** Microsoft.AspNetCore.JsonPatch (RFC 6902)
* **JSON Serialization:** Newtonsoft.Json (PATCH support)
* **Global Exception Handling:** Centralized Middleware

## Architectural Patterns
* **DTO Pattern:** Data Transfer Objects for decoupled data
* **Seed Data:** Initial database population for Categories and Resources