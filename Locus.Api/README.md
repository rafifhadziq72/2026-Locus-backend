# Locus Room Booking System - Backend API

## 📝 Description

**Locus** is a room booking management system developed for the **PBL 2026** university project. This Backend API serves as the core engine, managing room availability, user booking requests, and administrative approval workflows.

The system implements a specialized **"Gatekeeper"** workflow:
* **Flexibility:** Multiple users can submit `Pending` booking requests for the same room and time slot simultaneously.
* **Strict Enforcement:** Collision prevention is strictly enforced at the **Approval** stage. Once a request is approved, the system blocks all other overlapping slots to prevent double-booking.

## 🚀 Tech Stack

* **Framework:** ASP.NET Core 10.0
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core (EF Core)
* **Communication:** RESTful API with JSON
* **Standards:** Semantic Versioning (SemVer) & Conventional Commits

## ⚙️ Getting Started

### Prerequisites

* .NET SDK 10.0+
* PostgreSQL 17+
* `dotnet-ef` global tool (for running migrations)

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-github-username/2026-Locus-backend.git
   cd 2026-Locus-backend
   ```

2. **Environment Configuration:**
   Update the connection string in `appsettings.Development.json` with your local PostgreSQL credentials:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=locus_db;Username=postgres;Password=your_password"
     }
   }
   ```

3. **Database Initialization:**
   Apply the migrations to create the database schema:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```

The API will be accessible at `http://localhost:5000` or `https://localhost:5001`.

## 🛠 Project Standards & Workflow

This project follows the professional industry standards required for the PBL 2026 audit:

* **Branching Strategy:** GitHub Flow. All development occurs in `feat/` (features) or `fix/` (bug fixes) branches.
* **Commit Format:** Conventional Commits (e.g., `fix(logic): resolve timezone mismatch in approval`).
* **Versioning:** Managed via Git Tags following Semantic Versioning (Current Release: `v1.1.0`).
* **Soft Deletion:** Records use `IsDeleted` flags to maintain historical data for audit trails without losing record integrity.

## 📡 API Architecture & Logic

### Timezone Integrity

To ensure consistent validation across different environments, the system enforces:
* All timestamps are converted to UTC using `.ToUniversalTime()` before comparison.
* This prevents overlapping approvals caused by server-to-database offset mismatches (PostgreSQL +07).

### Core Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/bookings` | Fetches all records (includes active and soft-deleted history). |
| `POST` | `/api/bookings` | Creates a new request (allows overlapping `Pending` requests). |
| `PATCH` | `/api/bookings/{id}` | Approval Point: Validates time collisions and updates status. |
| `DELETE` | `/api/bookings/{id}` | Flag a record as deleted (Soft Delete). |

## 🧪 Testing

Execute the automated test suite using the .NET CLI:

```bash
dotnet test
```

## 📄 License & Audit Info

This project is developed for the PBL 2026 Track at Politeknik Elektronika Negeri Surabaya (PENS).

* **Target Deadline:** February 17, 2026
* **Audit Status:** Ready for Final Review
* **License:** MIT License