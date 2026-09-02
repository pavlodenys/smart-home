# Smart Home Project

This repository contains the source code for a smart home project, which includes a C# backend and a Svelte user interface.

## Features

The smart home project includes the following features:

- User authentication and authorization
- Management of smart home devices (lights, thermostats, etc.)
- Real-time updates and notifications
- Integration with third-party services (weather, news, etc.)

## Getting Started

The complete local stack runs with Docker Compose. Docker Desktop is the only required runtime.

```powershell
docker compose up --build -d
```

Open the UI at `http://localhost:5173`. The API and Swagger UI are available at
`http://localhost:5200` and `http://localhost:5200/swagger`.

RabbitMQ management is available at `http://localhost:15672`. Its local username
and password are read from the ignored root `.env` file. For a fresh local setup,
generate random service credentials before starting Compose:

```powershell
Copy-Item .env.example .env
.\scripts\rotate-local-secrets.ps1
```

To follow the services or stop the stack:

```powershell
docker compose logs -f
docker compose down
```

### Backend Setup

The backend of the smart home project is built using C# and .NET. To set up the backend, follow these steps:

1. Install .NET on your system if you haven't already.
2. Open a terminal or command prompt and navigate to the `backend` directory in the repository.
3. Run `dotnet restore` to restore the project dependencies.
4. Run `dotnet run` to start the backend server.

The backend server should now be running on port 5000.

### Frontend Setup

The user interface of the smart home project is built using Svelte. To set up the frontend, follow these steps:

1. Open a terminal or command prompt and navigate to the `frontend` directory in the repository.
2. Run `npm install` to install the project dependencies.
3. Run `npm run dev` to start the frontend server.

The frontend server should now be running on port 3000.

### Using the Smart Home Project

To use the smart home project, open a web browser and navigate to `http://localhost:3000`. You should see the user interface for the smart home project. From here, you can log in, view and control smart home devices, and manage your account settings.

## Contributing

If you would like to contribute to the smart home project, please follow these guidelines:

1. Fork this repository and make your changes on a separate branch.
2. Open a pull request with a clear description of your changes.
3. Your pull request will be reviewed by a project maintainer and merged if it is deemed appropriate.

## License

The smart home project is licensed under the MIT License. See the `LICENSE` file for more information.
