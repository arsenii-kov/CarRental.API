# 🚗 Car Rental API


A robust, production-ready RESTful API for a car rental service built with ASP.NET Core (.NET 10) and Entity Framework Core.

The project features role-based access control, complex booking overlap validation, automated pricing with tier discounts, unit testing, and containerization support.


---


## 🛠 Tech Stack


- Backend: C# / .NET 10, ASP.NET Core Web API

- Database & ORM: PostgreSQL / SQLite, Entity Framework Core (Code-First)

- Security: JWT Authentication, BCrypt Password Hashing, Role-Based Access Control (RBAC)

- Validation: Data Annotations & Business-Rule Validation

- Testing: xUnit, FluentAssertions, EF Core InMemory Provider

- DevOps: Docker, Docker Compose (Multi-stage build)

- API Documentation: Swagger / OpenAPI


---


## 🔑 Key Features


### 1. Role-Based Access Control (RBAC)

- Public: Browse available cars catalog, user registration, and login.

- Client (User): Create bookings, view personal booking history, cancel own upcoming bookings.

- Administrator (Admin): Manage fleet (CRUD cars), view all client bookings with filtering/pagination, change booking statuses, administrative cancellation.


### 2. Booking Business Logic

- Date Overlap Prevention: Validates that chosen time ranges do not intersect with existing active reservations for the same vehicle.

- Tiered Discount Engine:

  - Standard rate for rentals under 4 days.

  - 5% discount applied for rentals from 4 to 9 days.

  - 10% discount applied for rentals lasting 10 days or more.

- Optional Add-ons: Per-day insurance calculation.


### 3. Unit Testing Suite

Covered critical business logic using xUnit and FluentAssertions:

- Booking overlap detection across multiple edge cases (contained inside, surrounding, touching boundaries).

- Pricing & discount math verification.

- User authorization constraints for cancellations.

- State checks for cancelled vs active bookings.


---


## 🚀 Getting Started


### Option A: Run via Docker Compose (Recommended)

docker compose up --build


The API will be available at http://localhost:5000/swagger.


### Option B: Run Locally (.NET CLI)


1. Clone the repository:

   git clone https://github.com/arsenii-kov/CarRental.API.git

   cd CarRental.API


2. Run unit tests:

   dotnet test


3. Start the API:

   dotnet run --project CarRental.API


---


## 📋 API Endpoints Overview


- POST   /api/auth/register       [Public]        Register new user

- POST   /api/auth/login          [Public]        Authenticate & get JWT token

- GET    /api/cars                [Public]        Get list of available cars

- POST   /api/cars                [Admin]         Add new car to the fleet

- PUT    /api/cars/{id}           [Admin]         Update car details

- DELETE /api/cars/{id}           [Admin]         Remove car from the fleet

- POST   /api/bookings            [User / Admin]  Create a booking

- GET    /api/bookings/my         [User / Admin]  View current user's bookings

- GET    /api/bookings            [Admin]         View all bookings (supports pagination & filters)

- POST   /api/bookings/{id}/cancel [User / Admin] Cancel booking (rules enforced)