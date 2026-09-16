# Gestión de Turnos

Sistema web para gestionar los turnos de un centro de monitoreo: qué operador cubre qué turno, qué día. Proyecto de portfolio para búsqueda de trabajo freelance/remoto como desarrollador.

## Qué problema resuelve

En un centro de monitoreo hay operadores que se turnan para cubrir distintos horarios del día (mañana, tarde, noche), todos los días de la semana. Coordinar manualmente quién cubre qué turno cada día (por ejemplo, en una planilla suelta o por mensajes) es propenso a errores: turnos sin cubrir, doble asignación, falta de un registro claro de quién estuvo a cargo cuándo.

Este sistema centraliza esa información:

- Mantiene un listado de **Operadores** (nombre, contacto, rol) y de **Tipos de Turno** (nombre, hora de inicio, hora de fin), totalmente editables.
- Ofrece una **vista semanal** donde se ve, para cada Tipo de Turno y cada día de la semana, qué Operador lo tiene asignado — y permite cambiar cualquier asignación con un clic.
- Requiere **login** para poder modificar los datos, así no cualquiera puede alterar la planificación.

## Stack

- **ASP.NET Core MVC** (.NET 10) con Razor Views — renderizado en el servidor, sin frontend separado.
- **Entity Framework Core** con **SQLite** — toda la base vive en un único archivo, sin necesidad de instalar un motor de base de datos aparte.
- **ASP.NET Core Identity** para el login (cookies de autenticación, hasheo de contraseñas, protección CSRF).
- **Bootstrap** (el que trae la plantilla estándar de .NET) para los estilos.

## Cómo instalar y correr el proyecto localmente

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Las herramientas globales de EF Core (si no las tenés ya instaladas):

  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Pasos

1. Cloná el repositorio y entrá a la carpeta del proyecto backend:

   ```bash
   git clone https://github.com/Vale-davila11/gestion-turnos.git
   cd gestion-turnos/backend/GestionTurnos
   ```

2. Restaurá los paquetes NuGet:

   ```bash
   dotnet restore
   ```

3. Creá la base de datos aplicando las migraciones (esto genera `database/gestionturnos.db`, que no está versionado en git):

   ```bash
   dotnet ef database update
   ```

4. Corré la aplicación:

   ```bash
   dotnet run
   ```

5. Abrí el navegador en la URL que muestra la consola (por ejemplo `http://localhost:5239`).

### Usuario de prueba

Al iniciar la aplicación por primera vez, se crea automáticamente un usuario administrador de prueba:

- **Email:** `admin@gestionturnos.local`
- **Contraseña:** `Admin123!`

Es un usuario ficticio pensado solo para probar el sistema — cambiá la contraseña o creá tu propio usuario si vas a usar esto más allá de una demo. En un entorno deployado, estas credenciales se pueden reemplazar por variables de entorno (`AdminSeed__Email`, `AdminSeed__Password`) sin tocar el código — ver la sección de deploy más abajo.

## Demo online

> https://gestion-turnos-drju.onrender.com

Nota: al estar en el plan gratuito de Render, el servicio se "duerme" tras un rato de inactividad — la primera carga después de eso puede tardar unos segundos mientras arranca de nuevo.

## Deploy en Render

Render no tiene un runtime nativo para .NET (sí para Node, Python, Ruby, Go, Rust o Elixir), así que el proyecto se deploya con **Docker**: el repo incluye un `Dockerfile` en `backend/GestionTurnos` y Render lo detecta y lo usa automáticamente.

1. Creá una cuenta en Render y conectá tu repositorio de GitHub.
2. Creá un **Web Service** nuevo apuntando a este repo.
   - **Root Directory:** `backend/GestionTurnos`
   - **Runtime:** Docker (Render lo detecta solo al encontrar el `Dockerfile` en esa carpeta; no hace falta Build/Start Command manuales).
3. Agregá estas variables de entorno en la configuración del servicio:
   - `AdminSeed__Email` — email del admin de prueba para ese entorno (no reutilices el de desarrollo local).
   - `AdminSeed__Password` — contraseña del admin de prueba para ese entorno.
   - `ConnectionStrings__DefaultConnection` = `Data Source=gestionturnos.db` (ruta simple, sin las carpetas relativas que solo tienen sentido en desarrollo local).
4. Hacé el deploy. La aplicación aplica las migraciones y crea el usuario admin sola al arrancar — no hace falta ningún paso manual adicional.

**Nota sobre persistencia:** en el plan gratuito de Render el disco no es 100% persistente — si el servicio se reinicia por inactividad, la base SQLite vuelve a crearse vacía. Para esta demo de portfolio no es un problema, pero no es el comportamiento esperado de un sistema en producción con datos reales.

## Capturas de pantalla

_(Agregar las imágenes en una carpeta `docs/screenshots/` y enlazarlas acá)_

- [ ] Listado de Operadores
- [ ] Formulario de creación/edición de un Operador
- [ ] Listado de Tipos de Turno
- [ ] Pantalla de login
- [ ] Vista semanal de asignación de turnos (la pantalla principal del sistema)

## Documentación técnica

Las decisiones de diseño de cada parte del proyecto (por qué SQLite, por qué Identity, qué conceptos nuevos aparecen, errores comunes a evitar) están documentadas en [`GUIA_DE_ESTUDIO.md`](./GUIA_DE_ESTUDIO.md).
