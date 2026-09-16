# gestion-turnos

Proyecto personal: sistema web de gestión de turnos para un centro de monitoreo, pensado como pieza de portfolio para buscar trabajo freelance/remoto como desarrollador.

Contexto del repo: la estructura de carpetas `backend/GestionTurnos` y `database` puede existir pero estar vacía o sin implementación real. Explorar primero lo que hay antes de crear nada nuevo.

## Objetivo

Un MVP funcional, sin sobre-ingeniería, con este alcance exacto (no agregar funcionalidades fuera de esta lista):

1. CRUD de Operadores (nombre, contacto, rol)
2. CRUD de Tipos de Turno (nombre, hora inicio, hora fin — ej: Mañana 06:00-14:00, Tarde 14:00-22:00, Noche 22:00-06:00)
3. Login con autenticación (un usuario admin de prueba es suficiente, no hace falta manejo de roles complejo)
4. Vista de asignación de turnos: una tabla/calendario semanal donde se asigna qué operador cubre cada turno cada día

## Stack requerido

- ASP.NET Core MVC (Razor Views), no Web API pura — tiene que verse y funcionar visualmente en el navegador
- Entity Framework Core con SQLite (no SQL Server, para no depender de un motor externo instalado)
- ASP.NET Core Identity para el login (scaffolding estándar, no reinventar autenticación)
- Bootstrap para estilos (el que viene con las plantillas de .NET alcanza, no hace falta nada más elaborado)

## Cómo trabajar en este proyecto

- Ir paso a paso: primero modelos y base de datos, después cada CRUD, después el login, y al final la vista de asignación semanal (la parte más compleja).
- Antes de cada paso grande, explicar brevemente qué se va a hacer y por qué, en términos simples (el dueño del proyecto está aprendiendo, no es senior).
- Nombres de variables y entidades en español (Operador, TipoTurno, AsignacionTurno) — convención del dominio, mantener coherencia.
- Commits chicos y descriptivos a medida que se avanza, no un commit gigante al final.
- Si una decisión de diseño no está clara en este documento, preguntar antes de asumir y seguir de largo.

## Reglas de trabajo obligatorias

- Antes de escribir o modificar cualquier archivo de código, explicar brevemente qué se va a hacer y esperar confirmación explícita ("dale", "ok", "confirmado" o similar) antes de escribirlo. No asumir autorización por defecto, incluso si ya se venía hablando del plan general.
- Nunca hacer `git commit` sin preguntar antes. Cuando parezca buen momento para commitear, indicar qué archivos cambiaron y proponer un mensaje de commit, y esperar confirmación antes de ejecutarlo.
- Si hay que tomar una decisión de diseño que no esté clara en las instrucciones, preguntar antes de seguir en vez de asumir.
- Priorizar que se entienda cada parte del código por sobre la velocidad de entrega. Si se va a implementar algo con una librería, patrón o concepto no mencionado explícitamente, avisar primero.

## Documento de estudio (GUIA_DE_ESTUDIO.md)

En paralelo al código, mantener `GUIA_DE_ESTUDIO.md` documentando, por cada parte construida:

- Qué se hizo y para qué sirve dentro del sistema completo
- Por qué se tomó esa decisión técnica en particular (ej: por qué SQLite y no otra base, por qué Identity y no autenticación manual, por qué esa estructura de carpetas)
- Qué alternativas existían para resolver lo mismo, y en qué casos convendría usar cada una en vez de la elegida acá
- Conceptos nuevos que aparecen (ej: qué es un DbContext, qué es una migración de EF Core, qué es un ViewModel) explicados en términos simples
- Errores comunes o "trampas" que alguien nuevo podría cometer en esa parte

Este documento es tan importante como el código: el objetivo es poder explicar y defender cada decisión del proyecto en una entrevista o frente a un cliente, no solo tener algo que funciona sin entenderlo del todo.

## Al terminar el MVP

1. Escribir un `README.md` completo en español: qué problema resuelve el sistema, stack usado, cómo instalar dependencias y correrlo localmente, y un espacio para agregar capturas de pantalla (indicar dónde tomarlas).
2. Cerrar `GUIA_DE_ESTUDIO.md` con un índice al principio y una sección final de "próximos pasos si se quisiera ampliar este proyecto" (ideas de features que quedaron fuera del alcance a propósito).

## Estado actual

- Paso 1 completado: modelos (Operador, TipoTurno, AsignacionTurno), `ApplicationDbContext`, migración inicial y base SQLite (`database/gestionturnos.db`, generado, no versionado). Documentado en `GUIA_DE_ESTUDIO.md`.
- Paso 2 completado: base MVC (layout, Bootstrap, HomeController) + CRUD completo de Operadores (Controller + Views), probado end-to-end. Documentado en `GUIA_DE_ESTUDIO.md`.
- Paso 3 completado: CRUD completo de TipoTurno (Controller + Views), horario libre sin turnos predefinidos, probado end-to-end. Documentado en `GUIA_DE_ESTUDIO.md`.
- Paso 4 completado: login con ASP.NET Core Identity. `ApplicationDbContext` ahora hereda de `IdentityDbContext<IdentityUser>`. `AccountController` (Login/Logout) escrito a mano usando `SignInManager`. Operadores y TiposTurno protegidos con `[Authorize]`. Usuario admin de prueba sembrado en `Program.cs`: `admin@gestionturnos.local` / `Admin123!` (email inventado a propósito, no usar uno real — ver GUIA_DE_ESTUDIO.md). Probado end-to-end.
- Paso 5 completado: vista de Asignación Semanal (grilla Tipo de Turno × Día, con crear/actualizar/borrar en un solo guardado). `AsignacionesController` + `AsignacionSemanalViewModel`. Se permite que un operador tenga más de un turno el mismo día (decisión charlada, sin restricción). Probado end-to-end.
- README.md reescrito (estaba corrupto/vacío, UTF-16 sin contenido real) con problema/stack/instalación/checklist de capturas. GUIA_DE_ESTUDIO.md cerrada con la sección de "próximos pasos".
- MVP completo según el alcance del prompt original.
- Paso 6 completado: app preparada para deploy en Render (admin de prueba y connection string configurables por variable de entorno, puerto dinámico vía `PORT`, migraciones automáticas al iniciar, `UseHttpsRedirection` desactivado cuando corre en un PaaS). Documentado en GUIA_DE_ESTUDIO.md.
- Paso 7 completado: **deploy en Render vía Docker**. Render no tiene runtime nativo de .NET (a diferencia de Node/Python/Ruby/Go/Rust/Elixir), así que el primer intento como Web Service nativo falló (`dotnet: command not found`). Se agregó `backend/GestionTurnos/Dockerfile` (multi-stage: SDK de .NET 10 para build, imagen ASP.NET runtime para producción) y `.dockerignore`. Al crear el servicio en Render hubo que elegir explícitamente **Docker** como "Language" (no se auto-detectó porque el Dockerfile no está en la raíz del repo sino en `backend/GestionTurnos`, que es el Root Directory configurado). README.md actualizado (sección "Deploy en Render" y URL real en "Demo online"). Deploy probado end-to-end: **https://gestion-turnos-drju.onrender.com**.
- Pendiente (opcional, fuera del alcance original): tomar las capturas de pantalla reales, revisar las vulnerabilidades NU1903 (Microsoft.OpenApi, SQLitePCLRaw) cuando haya versiones más nuevas, y documentar el paso 7 (deploy con Docker) en GUIA_DE_ESTUDIO.md.
- Nota sobre el plan gratuito de Render: el servicio "duerme" tras inactividad; la primera carga después de eso tarda unos segundos en levantar de nuevo.
- Nota de entorno: en cada PC nueva hace falta instalar el SDK de .NET 10, y las herramientas globales `dotnet-ef` y (si se usa scaffolding) `dotnet-aspnet-codegenerator`, y correr `dotnet ef database update` antes de levantar la app (la base `.db` no está versionada).
