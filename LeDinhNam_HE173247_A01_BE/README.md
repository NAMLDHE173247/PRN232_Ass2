# FUNewsManagement System

## Overview
This is a comprehensive News Management System built as part of PRN232 Assignment 1. It consists of two parts:
1. **Backend Web API (`ass01`)**: Provides RESTful APIs and OData endpoints for managing news, categories, tags, and accounts. Built with ASP.NET Core Web API, .NET 8.0, and Entity Framework Core (SQL Server).
2. **Frontend Web App (`ass01_FE`)**: An MVC application built with ASP.NET Core MVC (.NET 8.0) that consumes the backend API.

## Requirements Checklist
- [x] Backend runs on .NET 8.0 Web API
- [x] Frontend runs on .NET 8.0 MVC
- [x] Database scripted with `Database.sql` containing schema and sample records
- [x] Entity Framework Core Code-First/DB-First
- [x] Account authentication and role-based authorization (Admin, Staff, Lecturer)
- [x] Change Audit (Updated By and Modified Date without new tables)
- [x] Advanced Search with Keyword, Category, Tag, Author, Status, Date Range filters
- [x] Pagination & OData sorting support

## Setup and Run Instructions

### 1. Database Setup
The system requires SQL Server. We have provided a full SQL dump with test data.
1. Open SQL Server Management Studio (SSMS).
2. Open the `Database.sql` file located in the root folder.
3. Execute the script. It will create the database `FUNewsManagement`, all necessary tables, and insert meaningful sample records (at least 5 per table).

### 2. Backend Setup (`ass01`)
1. Navigate into the `ass01` folder.
2. Open `appsettings.json` and verify the connection string matches your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "MyCnn": "Server=(localdb)\\MSSQLLocalDB;uid=sa;password=123;database=FUNewsManagement;Encrypt=True;TrustServerCertificate=True;"
   }
   ```
3. Run the backend using the command:
   ```bash
   dotnet run
   ```
4. The API will typically run on `https://localhost:7111` or `http://localhost:5037`.

### 3. Frontend Setup (`ass01_FE/ass01_FE`)
1. Navigate into the `ass01_FE/ass01_FE` folder.
2. Open `appsettings.json` and ensure `ApiSettings:BaseUrl` points to the backend's URL.
3. Run the frontend using the command:
   ```bash
   dotnet run
   ```
4. Access the web interface via the browser.

## Credentials

The system comes with the following pre-configured accounts:

| Role | Email | Password | Account ID |
|------|-------|----------|------------|
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` | N/A (Configured in `appsettings.json`) |
| **Staff** | `staff1@funews.com` | `1` | 1 |
| **Staff** | `staff2@funews.com` | `1` | 2 |
| **Lecturer** | `lecturer1@funews.com` | `1` | 3 |

## API Endpoints Overview
- `POST /api/auth/login`: Authenticate and receive a JWT token.
- `GET /api/news`: Retrieve active news articles with advanced OData filtering and sorting (`$filter`, `$orderby`, `$skip`, `$top`).
- `GET /api/news/{id}`: Retrieve a specific news article.
- `POST /api/news`: (Staff Only) Create a new news article.
- `PUT /api/news/{id}`: (Staff Only) Update a news article (triggers Audit Trail).
- `DELETE /api/news/{id}`: (Staff Only) Delete a news article.
- `GET /api/report`: (Admin Only) View aggregated statistics for articles within a date range.

## Screenshots and Sample Outputs
*(Placeholders for actual application screenshots)*
- **Login Screen**: `[Insert Login Screenshot Here]`
- **News Management (Staff)**: `[Insert Staff Dashboard Screenshot Here showing Advanced Filters]`
- **Report View (Admin)**: `[Insert Report Screenshot Here showing Updated By column]`
- **Article Details (Lecturer)**: `[Insert Details Screenshot Here]`
