# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EatKath.API.dll"]




#// ================================================================================================================
#// DOCKER - COMPLETE BASIC FLOW
#// ================================================================================================================
#//
#// | Stage                  | 🏠 Real-life analogy                | 🐳 What Docker is doing                    | 💻 What you do                         | 🖥️ Docker Desktop              |
#// |------------------------|--------------------------------------|--------------------------------------------|----------------------------------------|----------------------------------|
#// | 1. Docker Desktop      | Turn on the factory                  | Starts the Docker Engine                   | Open Docker Desktop                    | Docker Desktop opens             |
#// | 2. Dockerfile          | Recipe / instructions                | Reads your instructions                    | You already have Dockerfile            | View Dockerfile in Visual Studio  |
#// | 3. Build               | Factory makes the package            | Builds EatKath inside Docker               | docker build -t eatkath-api .          | Images → see eatkath-api          |
#// | 4. Image               | 📦 Finished packaged product         | Stores the built EatKath application       | Nothing else needed                    | Images → eatkath-api              |
#// | 5. Run                 | Take package and start using it      | Creates a running container from image     | docker run -p 8080:8080 eatkath-api    | Containers → see it running        |
#// | 6. Container           | 🏠 Running house                    | Runs EatKath inside an isolated environment| Nothing else                           | Container shows Running            |
#// | 7. Use API             | Visit the house                     | Sends requests to your running API        | http://localhost:8080                  | Container shows port 8080          |
#//
#// ================================================================================================================
#//
#// 🔑 MEMORY:
#//
#// Dockerfile = Recipe 📝
#// Image      = Finished package 📦
#// Container  = Running package 🚀
#// Docker Desktop = Factory machinery 🖥️
#//
#// ================================================================================================================


# ==========================================================
# COMPLETE FLOW
# ==========================================================
#
# 🖥️ Docker Desktop
#       ↓
# 🐳 Docker Engine
#       ↓
# 📝 Dockerfile
#       ↓
# 🏗️ docker build
#       ↓
# 🖼️ IMAGE
#       ↓
# ▶️ docker run
#       ↓
# 📦 CONTAINER
#       ↓
# 🚀 EatKath API
#       ↓
# 🌐 localhost:8080
#
#
# 🔑 MEMORY:
#
# Dockerfile = Recipe 📝
# Image      = Finished package 📦
# Container  = Running package 🚀
# Docker Desktop = Factory machinery 🖥️
#
# ==========================================================


#docker build -t eatkath-api .
#docker run -p 8080:8080 eatkath-api