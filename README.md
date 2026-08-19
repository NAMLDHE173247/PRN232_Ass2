# FUNewsManagementSystem - Assignment 2

## Project Overview
This repository contains the complete implementation of the FUNewsManagementSystem for PRN232 Assignment 02.
The solution is built using ASP.NET Core 8 Web API for backend services and ASP.NET Core MVC for the frontend.

## Solutions Included
1. **FUNewsManagement_CoreAPI.sln**: Core CRUD operations, Authentication (JWT), SignalR Notification Hub, Audit Logging.
2. **FUNewsManagement_AnalyticsAPI.sln**: Analytics API for Dashboard statistics and Trending News.
3. **FUNewsManagement_AIAPI.sln**: AI Tag Suggestion API with Tokenization and Learning Cache.
4. **FUNewsManagement_FE.sln**: Frontend MVC Application (Staff/Admin interfaces, Offline Mode).

## Setup Instructions

### Database Setup
1. Create a SQL Server Database named `FUNewsManagementDB`.
2. Run the provided script `FUNewsManagement.sql` located at the root of the repository to seed the tables and initial data.

### Configuration
Update the `appsettings.json` files across all API projects to point to your SQL Server instance if needed.

### Running the Application
Ensure that the 4 applications are started simultaneously. The frontend expects the backend services to be running on specific ports:
- Core API: `https://localhost:7001`
- Analytics API: `https://localhost:7002`
- AI API: `https://localhost:7003`
- Frontend: `https://localhost:7004`

## Key Features Implemented
- JWT Authentication & Proactive Token Refresh
- Comprehensive UI with Bootstrap Modals, Toasts, Loading states
- Advanced Dashboard with Chart.js
- AI Suggestion & Backend Tokenization Learning Cache
- Full Offline Mode with Background Worker
- Excel Export
- SignalR Notification System
- Audit Logging

## Test Accounts
You can use the following accounts to test the application:

- **Admin:** `admin@FUNewsManagementSystem.org` | Password: `@@abc123@@`
- **Staff:** `staff1@funews.com` | Password: `1`
- **Lecturer:** `lecturer1@funews.com` | Password: `1`
