\# Bus Tracking System



A service-oriented bus tracking and reservation system developed as a final project for the Service Oriented Architecture course at South East European University.



\## Team Members

\- Era Sela

\- Diellza Jusufi



\## Features

\- JWT Authentication and Authorization

\- Role-based access control (Admin, Driver, Passenger)

\- Real-time bus tracking using GPS/SIM808

\- Bus and trip management

\- Seat reservation system

\- QR code generation and verification

\- Notifications

\- Unit testing using xUnit, Moq, and EF Core InMemory



\## Technologies Used



\### Backend

\- ASP.NET Core Web API (.NET 8)

\- Entity Framework Core

\- PostgreSQL

\- JWT Authentication



\### Frontend

\- React

\- Vite



\### Testing

\- xUnit

\- Moq

\- Entity Framework Core InMemory



\## Project Structure



```

BusTrackingSystem

│

├── BusTrackingAPI

├── bus-tracking-frontend

└── README.md

```



\## Running the Backend



1\. Open the BusTrackingAPI solution in Visual Studio.

2\. Update the PostgreSQL connection string in `appsettings.json`.

3\. Run database migrations.

4\. Start the application.



\## Running the Frontend



```bash

npm install

npm run dev

```



\## Running Tests



```bash

dotnet test

```

\## Simulating GPS Data

Start a trip with device ID `SIM808_01`, then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\simulate-gps.ps1 `
  -GpxPath "C:\Users\User\Desktop\cap\Monday Morning Track.gpx"
```

The script samples the recorded GPX track and sends its coordinates to the deployed API.
By default, it sends 60 points at two-second intervals. Use `-Count`,
`-IntervalSeconds`, and `-DeviceId` to customize the simulation.



\## GitHub Repository



https://github.com/EraSela/BusTrackingSystem-SOA

