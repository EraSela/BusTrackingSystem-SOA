# Deployment Guide

## 1. Run CI on GitHub

Push the repository to `main`. GitHub Actions runs:

- .NET restore, Release build, and all backend tests.
- Frontend dependency installation, linting, and production build.

The workflow is stored in `.github/workflows/ci.yml`.

## 2. Deploy the Backend and Database on Render

1. Open the Render dashboard.
2. Select **New > Blueprint**.
3. Connect `EraSela/BusTrackingSystem-SOA`.
4. Render detects the root `render.yaml`.
5. Confirm creation of:
   - `bus-tracking-api`
   - `bus-tracking-db`
6. Wait for the database and API deployment to finish.
7. Open `https://<render-api-name>.onrender.com/health`.

Render generates the database credentials and JWT secret. The API applies
Entity Framework migrations automatically when it starts.

## 3. Deploy the Frontend on Vercel

1. In Vercel, select **Add New > Project**.
2. Import `EraSela/BusTrackingSystem-SOA`.
3. Set the root directory to:
   `bus-tracking-frontend/bus-tracking-frontend`
4. Keep the detected Vite build settings.
5. Add this environment variable:

   `VITE_API_BASE_URL=https://<render-api-name>.onrender.com/api`

6. Deploy the project.

The included `vercel.json` makes React Router links work after page refreshes.

## 4. Automatic Deployment

- Vercel automatically redeploys the frontend after pushes to `main`.
- Render deploys the API after the GitHub CI checks pass.

Render free PostgreSQL databases are temporary. Export important demonstration
data or move to a persistent database plan before the free database expires.
