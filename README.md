# Pipeline Integrity Management System

A full-stack web application for managing pipeline assets, inspection records, and risk analytics.  
The platform enables engineers to track infrastructure health, visualize geospatial segments, and evaluate risk severity through inspection-driven scoring.

---

## Overview

The Pipeline Integrity Management System is designed to simulate real-world asset monitoring workflows in the energy sector. It provides tools to manage pipelines and segments, record inspection events, compute risk scores, and display analytics through a modern dashboard interface.

This project demonstrates full-stack software engineering skills across backend API design, relational database modeling, frontend UI development, and geospatial data visualization.

---

## Tech Stack

### Backend
- C# — ASP.NET Core Web API
- Entity Framework Core
- RESTful API design

### Database
- Microsoft SQL Server
- Relational schema modeling
- EF Core migrations

### Frontend
- React + TypeScript
- HTML / CSS / JavaScript
- Vite build tooling

### Geospatial
- Leaflet map visualization
- Coordinate-based segment plotting

---

## Key Features

- Pipeline asset management
- Segment tracking with coordinate mapping
- Inspection record logging
- Automated risk score computation
- Severity classification (Low / Medium / High)
- Dashboard analytics
- Full CRUD workflows
- Map-based geospatial visualization

---

## System Architecture
React Frontend
↓
ASP.NET Core Web API
↓
Entity Framework Core
↓
SQL Server Database


---

##  Database Schema

### Pipelines
Stores high-level infrastructure assets.

### Segments
Represents individual pipeline sections with geospatial coordinates.

### Inspections
Captures inspection events, methods, and defect depth metrics.

### RiskScores
Stores computed risk severity for each segment.

Relationships:

Pipeline → Segments → Inspections
↘ RiskScore


---

##  API Endpoints

### Pipelines
| Method | Endpoint | Description |
|--------|-----------|-------------|
| GET | /api/pipelines | List pipelines |
| POST | /api/pipelines | Create pipeline |
| PUT | /api/pipelines/{id} | Update pipeline |
| DELETE | /api/pipelines/{id} | Delete pipeline |

### Segments
| Method | Endpoint |
|--------|-----------|
| GET | /api/segments |
| GET | /api/segments?pipelineId= |
| POST | /api/segments |
| DELETE | /api/segments/{id} |

### Inspections
| Method | Endpoint |
|--------|-----------|
| GET | /api/inspections?segmentId= |
| POST | /api/inspections |
| DELETE | /api/inspections/{id} |

### Analytics
| Method | Endpoint |
|--------|-----------|
| GET | /api/analytics/summary |
| POST | /api/analytics/recompute/{segmentId} |

---

##  Risk Scoring Logic

Risk is computed from inspection data:

- Base score = Maximum defect depth (%)
- Method weight applied:
  - ILI → +10
  - CPCM → +5
  - Visual → +0
- Score clamped to 0–100

Severity classification:

| Score | Severity |
|------|-----------|
| 0–34 | Low |
| 35–69 | Medium |
| 70–100 | High |

---

##  Geospatial Visualization

Pipeline segments are plotted using start/end coordinates.  
Risk severity is visually indicated through map markers and polylines.

---

##  Dashboard

Displays:

- Pipeline count
- Segment count
- Inspection count
- High-risk segment count
- Top risk segments table

---

##  Local Development Setup

### Clone repo
```bash
git clone https://github.com/<your-username>/Pipeline-Integrity-Management-System.git
cd Pipeline-Integrity-Management-System
```

## Backend setup


### Install dependencies:

```bash
dotnet restore
```
### Configure connection string in appsettings.json:

```bash
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=PipelineIntegrity;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

## Run migrations:
```bash
dotnet ef database update
```

Start API:
```bash
dotnet run
```

Swagger available at:
```bash
http://localhost:<port>/swagger
```

## Frontend setup

```bash
cd pipeline-ui
npm install
npm run dev
```

Create .env:

VITE_API_BASE_URL=http://localhost:<api-port>

 ## Screenshots



Examples:

Dashboard overview
<img width="1713" height="889" alt="Screenshot 2026-02-19 at 3 52 21 AM" src="https://github.com/user-attachments/assets/1ca5160f-76ec-400d-93f8-927aa7c71778" />


Pipeline table
<img width="1710" height="961" alt="Screenshot 2026-02-19 at 3 52 45 AM" src="https://github.com/user-attachments/assets/37a1493f-3865-4e85-9b9e-5eb613128d48" />

Segment map
<img width="1708" height="959" alt="Screenshot 2026-02-19 at 3 53 00 AM" src="https://github.com/user-attachments/assets/94816576-5157-4705-9d8a-ba3b4301b679" />

Inspection form
<img width="1713" height="940" alt="Screenshot 2026-02-19 at 3 53 12 AM" src="https://github.com/user-attachments/assets/aba64cce-a408-4830-8933-36e658d8aea7" />

## Future Enhancements

- Role-based authentication
- Real-time telemetry ingestion
- ML-driven failure prediction
- Azure SQL deployment
- CI/CD pipelines


## Learning Outcomes

This project demonstrates:
- Full-stack development with .NET + React
- SQL erver relational modeling
- REST API architecture
- EF Core migrations & data access
- Applied analytics workflows
