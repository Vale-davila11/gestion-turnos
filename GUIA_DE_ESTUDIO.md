# Guía de estudio — Sistema de Gestión de Turnos

Este documento acompaña al código del proyecto. Está pensado para que, además de tener el sistema funcionando, puedas explicar y defender cada decisión técnica en una entrevista o frente a un cliente.

## Índice

1. [Paso 1 — Modelos y base de datos](#paso-1--modelos-y-base-de-datos)

---

## Paso 1 — Modelos y base de datos

### Qué se hizo

- Se reconvirtió el proyecto `backend/GestionTurnos`, que originalmente estaba armado como una Web API pura apuntando a PostgreSQL, en un proyecto **ASP.NET Core MVC**.
- Se agregó **Entity Framework Core** con el proveedor de **SQLite**.
- Se definieron tres modelos (clases C#) que representan las tablas del sistema:
  - `Operador`: Id, Nombre, Contacto, Rol.
  - `TipoTurno`: Id, Nombre, HoraInicio, HoraFin (ej: Mañana 06:00–14:00).
  - `AsignacionTurno`: Id, Fecha, OperadorId, TipoTurnoId — es la tabla que conecta "qué operador cubre qué tipo de turno, qué día".
- Se creó `ApplicationDbContext`, la clase que EF Core usa para hablar con la base de datos.
- Se generó la primera **migración** (`Inicial`) y se aplicó, creando el archivo `database/gestionturnos.db` con las tres tablas.

### Para qué sirve dentro del sistema completo

Esta es la base de todo lo demás. Los CRUD del Paso 2, el login del Paso 3 y la vista semanal del Paso 4 van a leer y escribir sobre estas mismas tres tablas. Si el modelo de datos está mal pensado acá, se arrastra el problema a todo el resto.

### Por qué se tomaron estas decisiones técnicas

**¿Por qué SQLite y no PostgreSQL o SQL Server?**
SQLite guarda toda la base en un único archivo (`gestionturnos.db`), sin necesidad de instalar ni levantar un motor de base de datos aparte (un servicio corriendo en segundo plano, usuario, contraseña, puerto, etc.). Para un proyecto de portfolio esto es clave: cualquiera que clone el repo puede correrlo con `dotnet run` sin instalar nada más.

*Cuándo convendría otra opción:* en un sistema real en producción, con varios usuarios escribiendo al mismo tiempo y necesidad de alta disponibilidad, se preferiría PostgreSQL o SQL Server — motores pensados para eso. SQLite no está pensado para mucha concurrencia de escritura.

**¿Por qué Entity Framework Core (ORM) y no SQL directo (como el `crear-db.sql` que ya existía)?**
Con EF Core, el esquema de la base se define como clases C# (los modelos) y el propio EF Core genera el SQL necesario. La ventaja principal para este proyecto es el sistema de **migraciones**: cada cambio al modelo queda versionado y se puede aplicar o revertir con un comando, en vez de editar SQL a mano y tener que acordarte de correrlo en cada entorno.

*Cuándo convendría SQL directo:* cuando se necesita control fino sobre consultas muy específicas de rendimiento, o en equipos donde ya existe una base de datos gestionada por DBAs con sus propias convenciones (como parece haber sido la idea original del proyecto, ver `database/crear-db.sql`).

**¿Por qué esta estructura de carpetas (`Models/`, `Data/`)?**
Es la convención estándar de ASP.NET Core MVC: `Models/` para las clases de dominio, `Data/` para todo lo relacionado a la persistencia (el `DbContext`). Más adelante se suman `Controllers/` y `Views/`. Seguir la convención hace que cualquier otro desarrollador .NET entienda el proyecto sin explicación adicional.

### Conceptos nuevos

- **DbContext**: es la clase que representa "la sesión con la base de datos". Expone una propiedad `DbSet<T>` por cada tabla (ej: `DbSet<Operador> Operadores`), y a través de ella se hacen las consultas y los cambios. Internamente sabe cómo traducir eso a SQL para el motor configurado (en este caso, SQLite).
- **Migración**: es un archivo C# generado automáticamente que describe "cómo pasar la base de datos de un estado al siguiente" (ej: crear la tabla Operadores). Cada vez que cambiás un modelo, generás una migración nueva. Esto permite tener un historial versionado del esquema, igual que git versiona el código.
- **Code-First**: el enfoque donde el código (las clases C#) es la fuente de verdad, y la base de datos se genera a partir de él (en vez de al revés, que sería "Database-First": partir de una base ya existente y generar las clases desde ahí — el enfoque que sugería `crear-db.sql`).
- **ICollection<T> en un modelo** (ej: `Operador.Asignaciones`): esto se llama una "propiedad de navegación". Le permite a EF Core entender que un `Operador` puede tener muchas `AsignacionTurno`, y te deja escribir código como `operador.Asignaciones` en vez de hacer un JOIN manual.

### Errores comunes / trampas en esta parte

- **Olvidarse de correr `dotnet ef database update` después de agregar una migración.** Generar la migración (`dotnet ef migrations add`) solo crea el archivo C# con los cambios; hay que aplicarla (`database update`) para que efectivamente se modifique el archivo `.db`.
- **Commitear el archivo `.db`.** El archivo de la base de datos SQLite es un binario generado, no código fuente — por eso está en `.gitignore`. Lo que sí se commitea son las migraciones (son código C# reproducible).
- **Poner credenciales reales en `appsettings.json`.** Este archivo se sube al repositorio. Cualquier contraseña o secreto ahí queda expuesta públicamente en GitHub. Con SQLite este problema desaparece para la conexión a la base (no hay usuario/contraseña), pero el hábito hay que mantenerlo para cualquier otro secreto futuro (ej: claves de API).

---

## Próximos pasos si quisiera ampliar este proyecto

*(se completa al final del proyecto)*
