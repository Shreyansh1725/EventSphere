# 🎓 EventSphere – College Event & Activity Management System

> **Semester 5 | .NET Technologies Project**  
> Built with ASP.NET Core MVC + ADO.NET + SQL Server

---

## 📋 Table of Contents
1. [Project Overview](#-project-overview)
2. [Problem Statement](#-problem-statement)
3. [Features](#-features)
4. [User Roles](#-user-roles)
5. [Technology Stack](#-technology-stack)
6. [System Architecture](#-system-architecture)
7. [Project Structure](#-project-structure)
8. [Database Design](#-database-design)
9. [Prerequisites](#-prerequisites)
10. [Installation & Setup](#-installation--setup)
11. [Running the Project](#-running-the-project)
12. [Demo Credentials](#-demo-credentials)
13. [API Endpoints](#-api-endpoints)
14. [Syllabus Coverage](#-syllabus-coverage)
15. [Team Members](#-team-members)

---

## 📖 Project Overview

**EventSphere** is a full-stack College Event & Activity Management System that allows students to discover and register for events, event coordinators to propose new events, and administrators to manage the entire event lifecycle — all from a single unified web application.

The application demonstrates core .NET concepts including:
- Object-Oriented Programming (OOP) in C#
- ASP.NET Core MVC pattern
- ADO.NET data access (no Entity Framework)
- Role-Based Access Control (RBAC)
- REST API development

---

## ❗ Problem Statement

Managing college events traditionally involves:
- Manual paperwork for registrations
- No central system for tracking attendance
- Difficult certificate issuance
- Poor communication between coordinators and administration

**EventSphere** solves all of this with a centralized, role-based digital platform.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🔐 Role-Based Login | Single login page auto-routes Admin, Coordinator, or Student |
| 📅 Event Management | Full CRUD for events with categories, venue, date, capacity |
| 📝 Event Registration | Students register for events; seats tracked in real-time |
| ✅ Attendance Tracking | Admin marks Present/Absent per registered student |
| 🏅 Certificates | Auto-generated participation certificates for attended events |
| 🔔 Notifications | Admin broadcasts notifications to all students |
| 📊 Reports | Department-wise and event-wise analytics using DataSet |
| 🤝 Volunteers | Assign student volunteers to specific events |
| 🗂️ Event Requests | Coordinators submit proposals; Admin approves or rejects |
| 🌐 REST API | JSON endpoints for event data |

---

## 👥 User Roles

### 👨‍🎓 Student
- Register/Login to the system
- Browse and search upcoming events
- Register for events and cancel registrations
- View attendance and certificates
- Manage profile and change password
- Receive notifications from admin

### 🧑‍💼 Event Coordinator
- Submit new event proposals (with venue, date, time, capacity)
- View status of their requests (Pending / Approved / Rejected)
- Dashboard with statistics on submitted events

### 🛡️ Admin
- Approve or reject event requests from coordinators
- Create and manage events directly
- Manage student accounts (activate/deactivate)
- Mark attendance for any event
- Assign volunteers to events
- Send broadcast notifications
- View detailed analytics reports

---

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| **Language** | C# 12 |
| **Framework** | ASP.NET Core MVC (.NET 10) |
| **Data Access** | ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlDataAdapter`, `DataSet`, `DataTable`) |
| **Database** | Microsoft SQL Server LocalDB |
| **Frontend** | Razor Views, Bootstrap 5, HTML5, CSS3 |
| **State Management** | ASP.NET Core Session |
| **API** | ASP.NET Core Web API (REST) |
| **IDE** | Visual Studio 2022 / VS Code |

---

## 🏗️ System Architecture

```
EventSphere/
├── Presentation Layer     → Controllers + Razor Views (MVC)
├── Business Logic Layer   → Services (DashboardService, ReportService)
├── Data Access Layer      → Repository Pattern (ADO.NET only)
└── Cross-Cutting          → Middleware (Auth), Session, DI Container
```

The app uses the **Repository Pattern** with interfaces for clean separation:
- `IUserRepository` → `UserRepository`
- `IEventRepository` → `EventRepository`
- `IRegistrationRepository` → `RegistrationRepository`
- `IAttendanceRepository` → `AttendanceRepository`
- `INotificationRepository` → `NotificationRepository`
- `ICertificateRepository` → `CertificateRepository`
- `IVolunteerRepository` → `VolunteerRepository`

---

## 📁 Project Structure

```
EventSphere/
│
├── Controllers/
│   ├── AccountController.cs       # Login, Register, Logout
│   ├── StudentController.cs       # Student pages
│   ├── CoordinatorController.cs   # Coordinator pages
│   ├── AdminController.cs         # Admin pages
│   ├── EventsController.cs        # Public event browsing
│   └── API/EventsApiController.cs # REST API
│
├── Models/                        # Domain entities
├── ViewModels/                    # Page-specific data models
├── Repositories/                  # ADO.NET data access
├── Services/                      # Business logic
├── Middleware/
│   └── AuthMiddleware.cs          # Role-based route protection
│
├── Views/
│   ├── Shared/_Layout.cshtml      # Master layout
│   ├── Account/                   # Login, Register
│   ├── Student/                   # 7 student pages
│   ├── Coordinator/               # 3 coordinator pages
│   ├── Admin/                     # 7 admin pages
│   └── Events/                    # Browse, Details
│
├── Database/
│   ├── EventSphereDB.sql          # Schema creation script
│   └── SampleData.sql             # Seed data script
│
├── wwwroot/                       # Static files (CSS, JS)
├── appsettings.json               # Connection string config
└── Program.cs                     # DI registrations & middleware
```

---

## 🗄️ Database Design

**Database Name:** `EventSphereDB`  
**Server:** `(localdb)\mssqllocaldb`

| Table | Purpose |
|---|---|
| `Users` | Stores all users (Admin, Coordinator, Student) |
| `EventCategories` | Lookup for event types (Technical, Cultural, etc.) |
| `Events` | All events with status, venue, capacity |
| `Registrations` | Student ↔ Event registration records |
| `Attendance` | Tracks Present/Absent per registration |
| `Volunteers` | Students assigned as event volunteers |
| `Notifications` | System notifications (per-user or broadcast) |
| `Certificates` | Auto-issued certificates after attendance |

---

## ✅ Prerequisites

Make sure you have the following installed before running the project:

- [x] **.NET SDK 10.0+** — [Download](https://dotnet.microsoft.com/download)
- [x] **SQL Server LocalDB** — Installed with Visual Studio
- [x] **Visual Studio 2022** or **VS Code**
- [x] **sqlcmd** utility (included with SQL Server tools)

---

## ⚙️ Installation & Setup

### 1. Clone or Extract the Project
```
git clone <your-repo-url>
```
Or extract the ZIP file to any folder.

### 2. Set Up the Database
Open a terminal inside the project folder and run:

```shell
# Start LocalDB
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb

# Create database schema
sqlcmd -S "(localdb)\mssqllocaldb" -i "Database\EventSphereDB.sql"

# Insert sample/seed data
sqlcmd -S "(localdb)\mssqllocaldb" -i "Database\SampleData.sql"
```

### 3. Verify Connection String
Open `appsettings.json` and confirm:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EventSphereDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 4. Restore NuGet Packages
```shell
dotnet restore
```

---

## ▶️ Running the Project

### Option A: Visual Studio 2022
1. Open `EventSphere.csproj` in Visual Studio 2022
2. Press **`F5`** to run with debugging
3. Press **`Ctrl + F5`** to run without debugging
4. Browser opens automatically at `https://localhost:xxxx`

### Option B: VS Code
1. Open the `EventSphere` folder in VS Code
2. Press **`F5`** (uses the `.vscode/launch.json` config)

### Option C: Terminal
```shell
cd EventSphere
dotnet run
```
Then open [http://localhost:5000](http://localhost:5000) in your browser.

---

## 🔑 Demo Credentials

| Role | Email | Password |
|---|---|---|
| 🛡️ Admin | `admin@gmail.com` | `Admin@123` |
| 🧑‍💼 Coordinator | `coordinator@gmail.com` | `Coordinator@123` |
| 👨‍🎓 Student | `student@eventsphere.com` | `Student@123` |

> **One Login Page for all roles!** The system automatically redirects each role to their respective dashboard after login.

---

## 🌐 API Endpoints

Base URL: `http://localhost:5000/api`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/events` | Get all published events |
| `GET` | `/api/events/{id}` | Get a specific event |
| `POST` | `/api/events` | Create a new event |
| `PUT` | `/api/events/{id}` | Update an event |
| `DELETE` | `/api/events/{id}` | Delete an event |

---

## 📚 Syllabus Coverage

| Unit | Topic | Where Used |
|---|---|---|
| Unit 1 | CLR, Assemblies, C# OOP | All Models, Repositories, Services |
| Unit 1 | Classes, Inheritance, Interfaces | `IUserRepository` → `UserRepository`, etc. |
| Unit 1 | Abstract Classes, Polymorphism | Service layer, Repository interfaces |
| Unit 2 | ASP.NET Core MVC | All Controllers & Razor Views |
| Unit 2 | Routing, Middleware | `Program.cs`, `AuthMiddleware.cs` |
| Unit 3 | Session, State Management | Login session in `AccountController` |
| Unit 3 | Data Validation | ViewModels with Data Annotations |
| Unit 4 | ADO.NET | All Repository classes |
| Unit 4 | `SqlConnection`, `SqlCommand` | `UserRepository`, `EventRepository` |
| Unit 4 | `SqlDataReader` | All `GetById` methods |
| Unit 4 | `SqlDataAdapter`, `DataTable`, `DataSet` | `GetAllStudents`, `ReportService` |
| Unit 5 | REST API | `EventsApiController.cs` |
| Unit 5 | Dependency Injection | `Program.cs` service registrations |

---

## 👨‍💻 Team Members

| Member | Roll No | Contribution |
|---|---|---|
| Shreyansh Bhaliya | 92400103292 | Student Module, Events Browsing, UI Design |
| Hinesh Kanani | 92400103375 | Admin Module, Reports, Notifications |
| Bhavy Pujara | 92400103316 | Database, ADO.NET Layer, API, Coordinator Module |

---

## 📝 Acknowledgement

This project was developed as part of the **Semester 5 .NET Technologies** course requirement.  
*AI-assisted development tools were used during the implementation phase.*

---

*© 2026 EventSphere — College Event Management System*
