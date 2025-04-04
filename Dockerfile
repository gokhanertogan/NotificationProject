#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Notification.API/Notification.API.csproj", "src/Notification.API/"]
RUN dotnet restore "./src/Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/src/Notification.API"
RUN dotnet build "./Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/build


USER root

# Set build arguments for New Relic configuration
ARG NEW_RELIC_APP_NAME
ARG NEW_RELIC_LICENSE_KEY
ARG CORECLR_PROFILER
ARG CORECLR_ENABLE_PROFILING
ARG NEW_RELIC_DISTRIBUTED_ENABLED="true"
ARG CORECLR_NEWRELIC_HOME="/usr/local/newrelic-dotnet-agent"
ARG CORECLR_PROFILER_PATH="/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so"
ARG CORECLR_ENABLE_PROFILING="$CORECLR_ENABLE_PROFILING"
ARG CORECLR_PROFILER="$CORECLR_PROFILER"
ARG NEW_RELIC_LICENSE_KEY="$NEW_RELIC_LICENSE_KEY"
ARG NEW_RELIC_APP_NAME="$NEW_RELIC_APP_NAME"

# Install required dependencies
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    ca-certificates \
    apt-transport-https

# Add New Relic repository and GPG key (using their recommended method)
RUN curl -sSL https://download.newrelic.com/infrastructure_agent/linux/apt/newrelic-infra.gpg | tee /etc/apt/trusted.gpg.d/newrelic.asc
RUN echo "deb https://download.newrelic.com/infrastructure_agent/linux/apt stable main" | tee /etc/apt/sources.list.d/newrelic-infra.list

# Update apt-get and install necessary dependencies for the New Relic .NET agent
RUN apt-get update && apt-get install -y \
    newrelic-dotnet-agent \
    libc6-dev \
    libcurl4-openssl-dev \
    libunwind8

# Clean up unnecessary files to reduce image size
RUN apt-get clean && rm -rf /var/lib/apt/lists/* /var/cache/*

# Set environment variables for New Relic agent
ENV NEW_RELIC_LICENSE_KEY=$NEW_RELIC_LICENSE_KEY
ENV NEW_RELIC_APP_NAME=$NEW_RELIC_APP_NAME
ENV CORECLR_PROFILER=$CORECLR_PROFILER
ENV CORECLR_ENABLE_PROFILING=$CORECLR_ENABLE_PROFILING
ENV NEW_RELIC_DISTRIBUTED_ENABLED=$NEW_RELIC_DISTRIBUTED_ENABLED
ENV CORECLR_NEWRELIC_HOME=$CORECLR_NEWRELIC_HOME
ENV CORECLR_PROFILER_PATH=$CORECLR_PROFILER_PATH


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]