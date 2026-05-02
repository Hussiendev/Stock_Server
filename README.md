# Stock Manager API

ASP.NET Core 8 backend – products, suppliers, users.  
JWT auth (HttpOnly cookies) + refresh tokens + PostgreSQL + role-based access.

## Default Admin
- Username: `ali`
- Password: `33`
- Role: `Admin`

## Roles & Permissions

| Role     | Products | Suppliers | Users |
|----------|----------|-----------|-------|
| Admin    | full CRUD | full CRUD | full CRUD |
| Employee | full CRUD | read-only | no access |

## Tech Stack
.NET 8, EF Core, PostgreSQL, JWT (cookies), BCrypt, GitHub Actions
## Key Endpoints

| Method | Endpoint | Who can access |
|--------|----------|----------------|
| POST   | /api/auth/login | public |
| POST   | /api/auth/refresh | public (cookie) |
| POST   | /api/auth/logout | authenticated |
| GET    | /api/products | authenticated |
| POST/PUT/DELETE /api/products | Admin + Employee |
| GET    | /api/suppliers | authenticated |
| POST/PUT/DELETE /api/suppliers | Admin only |
| GET    | /api/users | Admin only |
| POST/DELETE /api/users | Admin only |

## Environment defaults
- JWT_EXPIRY_MINUTES = 15
- JWT_REFRESH_EXPIRY_MINUTES = 10080 (7 days)

## CI/CD
GitHub Actions builds on push to main/develop.

---
Made by Hussien Zoughaib – [GitHub](https://github.com/Hussiendev)