# Etapa 1: build. Usa la imagen con el SDK completo de .NET 10 (compilador incluido)
# porque para compilar y publicar hace falta más que solo el runtime.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copia primero solo el .csproj y se restauran los paquetes NuGet.
# Docker cachea cada instrucción: si después solo cambia el código (no las
# dependencias), esta capa de restore no se vuelve a ejecutar.
COPY GestionTurnos.csproj ./
RUN dotnet restore

# Recién acá se copia el resto del código fuente.
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime. Imagen liviana que solo trae lo necesario para EJECUTAR
# una app ASP.NET Core (no el compilador ni las herramientas de build).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Documentativo: el puerto real lo define Render en runtime vía la
# variable de entorno PORT (Program.cs ya la lee).
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestionTurnos.dll"]
