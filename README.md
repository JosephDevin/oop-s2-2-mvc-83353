# Food Safety Inspection Tracker

An ASP.NET Core MVC application for tracking food premises inspections, outcomes, and follow-up actions.

## 🚀 Overview
This project manages local council food safety inspections with a focus on audit trails, structured logging, and role-based security.

### 🛠️ Tech Stack
*   **Framework:** ASP.NET Core MVC 10.0
*   **Database:** Entity Framework Core + SQLite
*   **Security:** ASP.NET Core Identity (Admin, Inspector, Viewer)
*   **Logging:** Serilog (Console + Rolling File)
*   **Testing:** xUnit (Unit Tests + In-Memory DB)
*   **CI/CD:** GitHub Actions

## 📋 Features
1.  **Dashboard Aggregation:** Monthly inspection counts, failure tracking, and overdue follow-up monitoring.
2.  **Filtering:** Advanced search by Town and Risk Rating using LINQ.
3.  **Audit Trail:** Every critical action (create/update) is logged with enriched context using Serilog.
4.  **Role-Based Access Control (RBAC):**
    *   **Admin:** Full system access.
    *   **Inspector:** Can create inspections and follow-ups.
    *   **Viewer:** Read-only access to dashboards and records.
5.  **Global Error Handling:** Middleware-level exception catching with "friendly" failure pages.

## ⚙️ Setup & Installation
1.  **Dependencies:** Ensure you have the .NET 10.0 SDK installed.
2.  **Database Migration:** Run the following in the Package Manager Console:
    ```powershell
    Add-Migration InitialCreate
    Update-Database
    ```
3.  **Seed Data:** The system automatically seeds test users and 12+ premises on first run.
    *   **Admin:** `admin@test.com` / `Admin123!`
    *   **Inspector:** `inspector@test.com` / `Inspector123!`
    *   **Viewer:** `viewer@test.com` / `Viewer123!`

## 🧪 Testing
Run tests via Visual Studio Test Explorer or CLI:
```bash
dotnet test
```

## 📝 Logging
Logs are stored in the `/Logs` folder and updated daily. They capture:
*   `Information`: Record creations.
*   `Warning`: Validation/Business rule violations.
*   `Error`: System exceptions.
# oop-s2-2-mvc-83353
# oop-s2-2-mvc-83353
