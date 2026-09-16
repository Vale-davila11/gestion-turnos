# Guía de estudio — Sistema de Gestión de Turnos

Este documento acompaña al código del proyecto. Está pensado para que, además de tener el sistema funcionando, puedas explicar y defender cada decisión técnica en una entrevista o frente a un cliente.

## Índice

1. [Paso 1 — Modelos y base de datos](#paso-1--modelos-y-base-de-datos)
2. [Paso 2 — Base MVC y CRUD de Operadores](#paso-2--base-mvc-y-crud-de-operadores)
3. [Paso 3 — CRUD de Tipos de Turno](#paso-3--crud-de-tipos-de-turno)
4. [Paso 4 — Login con ASP.NET Core Identity](#paso-4--login-con-aspnet-core-identity)
5. [Paso 5 — Vista de Asignación Semanal](#paso-5--vista-de-asignación-semanal)

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

## Paso 2 — Base MVC y CRUD de Operadores

### Qué se hizo

- El proyecto tenía los modelos y la base de datos (Paso 1), pero le faltaba toda la estructura visual: no existía `Views/`, `wwwroot/` (con Bootstrap) ni un `HomeController`. Se generó esa base con la plantilla oficial `dotnet new mvc` y se copiaron al proyecto los archivos estándar (`_Layout.cshtml`, `_ViewImports.cshtml`, `_ViewStart.cshtml`, Bootstrap/jQuery en `wwwroot/`, y un `HomeController` mínimo), adaptando el namespace a `GestionTurnos`.
- Se escribió a mano `OperadoresController.cs` con las 5 acciones estándar de un CRUD: `Index` (listar), `Details` (ver uno), `Create` (GET muestra el formulario, POST lo guarda), `Edit` (GET/POST) y `Delete` (GET pide confirmación, POST borra).
- Se crearon las vistas Razor correspondientes en `Views/Operadores/` (`Index`, `Details`, `Create`, `Edit`, `Delete.cshtml`), usando Tag Helpers de ASP.NET Core (`asp-for`, `asp-action`, etc.) en vez de HTML plano.
- Se agregó un link "Operadores" en el menú de navegación del layout.
- Se probó el flujo completo (crear, listar, ver detalle, editar, eliminar) corriendo la app y haciendo pedidos HTTP reales a cada acción antes de dar el paso por terminado.

### Para qué sirve dentro del sistema completo

Este CRUD es el primero de los dos que pide el MVP (el segundo es Tipos de Turno) y sirve como plantilla: el de Tipos de Turno se va a escribir siguiendo exactamente la misma estructura. Además, la base MVC (layout, Bootstrap, Home) que se armó acá es la que van a usar todas las pantallas que faltan, incluida la vista semanal de asignación.

### Por qué se tomaron estas decisiones técnicas

**¿Por qué escribir el Controller y las Views a mano en vez de usar el scaffolding automático de `dotnet aspnet-codegenerator`?**
Se intentó usar el generador automático primero, pero tiene un bug conocido: pide instalar el paquete `Microsoft.EntityFrameworkCore.SqlServer` aunque el proyecto use SQLite. Agregar ese paquete solo para destrabar la herramienta hubiera dejado una dependencia sin sentido en el `.csproj`. Como además el objetivo de este proyecto es entender cada parte del código (no solo tenerlo funcionando), escribirlo a mano tiene la ventaja de que no hay "magia" generada automáticamente que haya que descifrar después.

*Cuándo convendría el scaffolding:* en un proyecto grande, con muchas entidades repetitivas y donde ya se entiende bien el patrón CRUD, el scaffolding ahorra tiempo. Ahí sí vale la pena resolver el problema del paquete de SQL Server (por ejemplo, agregándolo solo como dependencia de desarrollo).

**¿Por qué el atributo `[Bind("Nombre,Contacto,Rol")]` en el `Create`, en vez de recibir el objeto `Operador` completo?**
Esto se llama protección contra **over-posting**: si el action recibiera el modelo completo sin restricciones, alguien podría mandar por HTTP un campo que no debería poder editar (por ejemplo, si el modelo tuviera un campo `EsAdmin`, un atacante podría agregarlo al formulario enviado aunque no exista un `<input>` para eso en la vista). Con `[Bind]` se especifica una lista blanca de qué propiedades se aceptan del formulario.

*Cuándo no haría falta:* si se usaran ViewModels específicos por acción (una clase separada solo con los campos del formulario) en vez de bindear directamente la entidad. Es una alternativa más prolija en proyectos grandes, pero para este MVP el `[Bind]` alcanza sin agregar una clase más.

**¿Por qué `[ValidateAntiForgeryToken]` en cada POST?**
Protege contra ataques CSRF (Cross-Site Request Forgery): sin esto, una página maliciosa externa podría enviar un formulario escondido al servidor haciéndose pasar por el usuario logueado. ASP.NET Core genera automáticamente un token oculto en cada `<form>` (vía el Tag Helper) y este atributo verifica que el token recibido coincida con el esperado.

**¿Por qué generar primero un proyecto temporal con `dotnet new mvc` en vez de escribir el layout y el Bootstrap a mano?**
El prompt original pedía específicamente "el Bootstrap que viene con las plantillas de .NET, no hace falta nada más elaborado" — la forma de tener exactamente eso es tomarlo de la plantilla oficial. Escribirlo a mano hubiera significado reinventar (peor) algo que Microsoft ya resuelve bien.

### Conceptos nuevos

- **Controller / Action**: un Controller (`OperadoresController`) es una clase que agrupa operaciones relacionadas a una entidad. Cada método público (`Index`, `Create`, etc.) es una "Action" que responde a una URL (ej: `/Operadores/Create`).
- **Razor View**: un archivo `.cshtml` que mezcla HTML con código C# (usando `@`). Es lo que arma la página que ve el usuario a partir de los datos que le pasa el Controller.
- **Tag Helper**: atributos especiales en el HTML (`asp-for`, `asp-action`, `asp-route-id`) que ASP.NET Core procesa en el servidor para generar el HTML final (URLs correctas, nombres de campos ligados al modelo, etc.), en vez de escribirlos a mano y arriesgarse a errores de tipeo.
- **ModelState**: un objeto que junta los resultados de validar los datos que llegaron del formulario contra las reglas del modelo (ej: `[Required]`, `[StringLength]` en `Operador.cs`). `ModelState.IsValid` dice si todo pasó esas validaciones.
- **Patrón GET/POST separado (`Create`, `Edit`, `Delete`)**: por convención, el método GET de una acción solo *muestra* el formulario o la pantalla de confirmación; el POST es el que efectivamente *hace* el cambio. Esto evita que, por ejemplo, refrescar la página o que un buscador indexe la URL borre datos por accidente.

### Errores comunes / trampas en esta parte

- **Bindear la entidad completa sin `[Bind]` ni ViewModel.** Deja la puerta abierta a over-posting (ver arriba).
- **Olvidarse `[ValidateAntiForgeryToken]` en un POST.** Compila y funciona igual en desarrollo, pero deja el endpoint vulnerable a CSRF.
- **No revisar `ModelState.IsValid` antes de guardar.** Si se guarda directo sin chequearlo, se pueden persistir datos que no cumplen las validaciones del modelo.
- **Correr la app sin haber aplicado la migración (`dotnet ef database update`) en una PC nueva.** El archivo `.db` no se versiona (está en `.gitignore`), así que cada máquina nueva necesita este paso — si no, las páginas que consultan la base tiran "no such table".

---

## Paso 3 — CRUD de Tipos de Turno

### Qué se hizo

- Se escribió `TiposTurnoController.cs`, siguiendo exactamente la misma estructura que `OperadoresController` del Paso 2 (Index, Details, Create, Edit, Delete).
- Se crearon las vistas Razor en `Views/TiposTurno/`.
- Se agregó el link "Tipos de Turno" al menú del layout.
- Se decidió que la carga de horario fuera un CRUD simple y libre: dos campos de hora (`HoraInicio`, `HoraFin`) donde se puede tipear cualquier horario, sin turnos predefinidos ni botones de acceso rápido. Los ejemplos del prompt original (Mañana 06-14, Tarde 14-22, Noche 22-06) son solo eso — ejemplos de qué datos se pueden cargar, no valores fijos del sistema.
- Se probó el flujo completo (crear, editar, eliminar) antes de dar el paso por terminado.

### Para qué sirve dentro del sistema completo

Junto con Operadores, esto completa los dos catálogos base que necesita el sistema. La vista de asignación semanal (Paso 5) va a combinar ambos: qué Operador cubre qué TipoTurno, en qué día.

### Por qué se tomaron estas decisiones técnicas

**¿Por qué `TimeOnly` en el modelo (`HoraInicio`, `HoraFin`) y no `DateTime` o `string`?**
`TimeOnly` es un tipo de .NET pensado específicamente para representar una hora del día sin fecha (a diferencia de `DateTime`, que siempre incluye fecha, u obligaría a inventar una fecha "dummy" sin sentido). El Tag Helper `asp-for` de ASP.NET Core lo reconoce automáticamente y genera un `<input type="time">`, que en el navegador ya trae un selector de hora nativo.

**¿Por qué no restringir a turnos predefinidos (Mañana/Tarde/Noche fijos)?**
El prompt pedía explícitamente un CRUD de Tipos de Turno con nombre y horario editable — si estuvieran fijos no habría nada que crear, editar o eliminar. Los tres ejemplos sirven para probar el sistema, pero cualquier centro de monitoreo real podría necesitar un cuarto turno, o cambiar los horarios de los existentes, y el CRUD ya permite eso sin tocar código.

### Conceptos nuevos

- **TimeOnly**: tipo de .NET (desde .NET 6) para representar solamente una hora del día (ej: `14:30`), sin componente de fecha. Complementa a `DateOnly`, que es solo fecha sin hora.

### Errores comunes / trampas en esta parte

- **Comparar `HoraInicio` y `HoraFin` asumiendo que la fin siempre es mayor a la de inicio.** El turno Noche (22:00–06:00) cruza la medianoche, así que una validación tipo "HoraFin > HoraInicio" rechazaría un turno nocturno válido. Por eso no se agregó esa validación en este paso — hay que tenerlo en cuenta si más adelante se agrega alguna regla de horario.

---

## Paso 4 — Login con ASP.NET Core Identity

### Qué se hizo

- Se agregó el paquete `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
- `ApplicationDbContext` pasó a heredar de `IdentityDbContext<IdentityUser>` en vez de `DbContext`. Esto agrega automáticamente las tablas de Identity (`AspNetUsers`, `AspNetRoles`, etc.) al modelo.
- Se generó y aplicó una nueva migración (`AgregarIdentity`) para crear esas tablas.
- Se configuró Identity en `Program.cs`: `AddIdentity<IdentityUser, IdentityRole>()`, autenticación por cookies, y un `SeedAdminAsync` que crea un usuario admin de prueba al iniciar la aplicación si todavía no existe (`admin@gestionturnos.local`).
- Se escribió a mano un `AccountController` con dos acciones: `Login` (GET muestra el formulario, POST valida credenciales con `SignInManager`) y `Logout`. No se generaron las páginas por defecto de Identity (Registro, Recuperar contraseña, Confirmación de email, etc.) porque el alcance del proyecto solo pide un admin de prueba, sin gestión de cuentas.
- Se agregó `[Authorize]` a `OperadoresController` y `TiposTurnoController`, así que ahora hace falta estar logueado para gestionarlos.
- Se probó el flujo completo: acceso bloqueado sin sesión, redirección a Login con `ReturnUrl`, login con credenciales correctas y también incorrectas, acceso permitido ya logueado, y logout.

### Para qué sirve dentro del sistema completo

Protege los CRUD de Operadores y Tipos de Turno (y en el futuro, la vista de asignación) para que no cualquiera pueda modificar los datos del sistema sin loguearse.

### Por qué se tomaron estas decisiones técnicas

**¿Por qué ASP.NET Core Identity y no un login hecho a mano (verificar usuario/contraseña contra una tabla propia)?**
Identity ya resuelve, de forma probada y segura, cosas que son fáciles de hacer mal si se escriben a mano: el hasheo de contraseñas (nunca se guardan en texto plano), la protección contra fuerza bruta, la generación y validación de la cookie de sesión, y la integración con `[Authorize]`. Reinventar esto no aporta nada al proyecto y sí agrega riesgo de un login inseguro.

*Cuándo se justificaría un login manual:* prácticamente nunca para un proyecto nuevo — Identity es el estándar de la plataforma. Se ve código de login manual sobre todo en proyectos legacy que no lo tenían disponible.

**¿Por qué escribir el `AccountController` a mano en vez de usar el scaffolding completo de Identity (`dotnet new mvc -au Individual`)?**
El scaffolding completo trae Registro, Recuperación de contraseña, Confirmación de email por correo, autenticación en dos pasos, login con proveedores externos (Google, Microsoft), y páginas de administración de la propia cuenta. El prompt original pide explícitamente "un admin de prueba es suficiente, no hace falta manejo de roles complejo" — agregar todo eso sería sobre-ingeniería para este MVP. Lo que sí se usó del scaffolding es la parte que importa: las clases `UserManager`/`SignInManager` de Identity, que son las mismas que usan las páginas generadas automáticamente.

**¿Por qué el usuario admin se crea con código (`SeedAdminAsync`) en vez de insertarlo a mano en la base?**
Así cualquiera que clone el repo y corra `dotnet ef database update` seguido de `dotnet run` tiene el usuario de prueba disponible sin pasos manuales extra, incluso en una base nueva y vacía. Es la misma lógica de las migraciones: reproducible en cualquier máquina.

**¿Por qué el admin de prueba usa un email inventado (`admin@gestionturnos.local`) y no un email real?**
El código que crea este usuario queda en el repositorio, que es público. Poner un email real y una contraseña débil ahí los expone permanentemente en el historial de git. `admin@gestionturnos.local` es un dominio reservado para pruebas/documentación (no resuelve a nada real).

### Conceptos nuevos

- **IdentityDbContext**: una versión de `DbContext` que ya trae predefinidas las tablas necesarias para usuarios, roles, y sus relaciones. Al heredar de él (en vez de `DbContext` a secas), el propio `ApplicationDbContext` termina teniendo tanto las tablas del dominio (Operadores, TiposTurno) como las de autenticación, todo en la misma base SQLite.
- **UserManager / SignInManager**: dos clases de Identity con responsabilidades distintas. `UserManager<TUser>` maneja el CRUD de usuarios (crearlos, buscarlos, cambiar contraseñas). `SignInManager<TUser>` maneja el proceso de login/logout en sí (validar credenciales, escribir la cookie de sesión).
- **`[Authorize]`**: atributo que se pone sobre un Controller o una Action para exigir que el usuario esté autenticado antes de poder ejecutarla. Si no lo está, ASP.NET Core lo redirige automáticamente a la ruta configurada en `LoginPath` (en este proyecto, `/Account/Login`), agregando un `ReturnUrl` para volver a la página que quería ver después de loguearse.
- **Cookie de autenticación**: después de un login exitoso, el servidor manda al navegador una cookie firmada que identifica la sesión. En cada pedido siguiente, el navegador la reenvía automáticamente y el middleware de autenticación (`app.UseAuthentication()`) la valida antes de que la request llegue al Controller.

### Errores comunes / trampas en esta parte

- **Olvidar `base.OnModelCreating(modelBuilder)` al sobreescribir `OnModelCreating` en un `IdentityDbContext`.** Si no se llama al método de la clase base, EF Core no configura las tablas de Identity correctamente.
- **Poner `app.UseAuthorization()` antes de `app.UseAuthentication()`.** El orden importa: primero hay que identificar quién es el usuario (Authentication), y después decidir si tiene permiso (Authorization). Al revés, `[Authorize]` no tiene la información del usuario todavía.
- **Commitear credenciales reales de prueba.** Por eso se usó un email inventado — ver la sección de decisiones técnicas arriba.
- **Cambiar la contraseña del admin de prueba en un solo lugar y no en el otro.** La contraseña vive únicamente en `SeedAdminAsync` (`Program.cs`); si se cambia ahí después de que el usuario ya existe en la base, no tiene efecto porque `SeedAdminAsync` solo crea el usuario si todavía no existe — hay que borrarlo de la tabla `AspNetUsers` (o borrar el archivo `.db` y volver a migrar) para que tome la contraseña nueva.

---

## Paso 5 — Vista de Asignación Semanal

### Qué se hizo

- Se agregó `AsignacionesController` (protegido con `[Authorize]`, igual que los otros dos) con dos acciones: `Index` (muestra la grilla de una semana) y `Guardar` (procesa el formulario completo de la grilla).
- Se creó `AsignacionSemanalViewModel`, que junta todo lo que necesita la vista: el lunes de la semana mostrada, la lista de los 7 días, los Tipos de Turno existentes, los Operadores existentes, y una lista plana de "celdas" (una por cada combinación Tipo de Turno × Día).
- La vista `Views/Asignaciones/Index.cshtml` dibuja una tabla: filas = Tipos de Turno, columnas = los 7 días de la semana, y cada celda es un `<select>` con los Operadores (más la opción "Sin asignar"). Arriba tiene links para ir a la semana anterior/siguiente.
- Al tocar "Guardar asignaciones", se manda todo el formulario de una sola vez. El servidor compara, para cada celda, contra lo que ya había en la base esa semana: si se seleccionó un operador y no había nada, crea el registro; si había un operador distinto, lo actualiza; si se dejó "Sin asignar" y antes había algo, lo borra.
- Se decidió (charlado antes de programar) que un mismo operador pueda quedar asignado a más de un Tipo de Turno el mismo día — no hay ninguna restricción que lo bloquee.
- Se probaron los tres caminos (crear, actualizar, y vaciar una celda) haciendo pedidos HTTP reales contra el servidor corriendo, además de la navegación entre semanas.

### Para qué sirve dentro del sistema completo

Es la pantalla principal del sistema — la razón por la que existe todo lo demás. Los Operadores y Tipos de Turno de los pasos anteriores son los "ingredientes"; esta vista es donde se combinan semana a semana.

### Por qué se tomaron estas decisiones técnicas

**¿Por qué un único botón "Guardar" para toda la grilla, en vez de que cada celda se guarde sola (por ejemplo, con JavaScript al cambiar el `<select>`)?**
Guardar todo junto es mucho más simple de implementar y de entender — un solo POST, un solo método en el Controller, sin necesitar JavaScript ni llamadas AJAX. La desventaja es que si alguien cambia 10 celdas y se corta la conexión antes de guardar, se pierden los 10 cambios (con guardado celda por celda, se habrían guardado los que ya se alcanzaron a mandar). Para el tamaño de este MVP (una grilla de una semana, algunos Tipos de Turno) esa desventaja no pesa tanto como la simplicidad ganada.

*Cuándo convendría guardar celda por celda:* en una grilla mucho más grande (por ejemplo, un mes completo con muchos operadores), donde perder todos los cambios de una sesión larga de edición sería más costoso.

**¿Por qué comparar contra la base y decidir crear/actualizar/borrar, en vez de borrar todas las asignaciones de la semana y volver a crearlas de cero?**
Borrar y recrear todo es más simple de programar, pero tiene una desventaja real: cada fila borrada y recreada obtiene un `Id` nuevo. Si en el futuro otra parte del sistema necesitara referenciar una asignación puntual por su Id (por ejemplo, un historial de cambios), borrar y recrear rompería esas referencias sin necesidad. Comparar y actualizar solo lo que cambió mantiene los `Id` estables para las celdas que no se tocaron.

**¿Por qué la lista `Celdas` es "plana" (una lista de objetos con TipoTurnoId + Fecha + OperadorId) en vez de una matriz de dos dimensiones?**
El *model binding* de ASP.NET Core (el mecanismo que convierte los campos de un formulario HTML en objetos C#) entiende de forma nativa listas indexadas como `celdas[0].TipoTurnoId`, `celdas[1].TipoTurnoId`, etc. Una matriz de dos dimensiones no tiene esa misma convención estándar, así que hubiera requerido código manual para reconstruirla — la lista plana es lo que el framework ya sabe hacer solo.

### Conceptos nuevos

- **ViewModel para una pantalla compleja**: a diferencia de los CRUD anteriores (donde la vista podía trabajar directo sobre `Operador` o `TipoTurno`), esta pantalla no representa una sola entidad sino una combinación de varias cosas a la vez. Por eso se armó una clase (`AsignacionSemanalViewModel`) pensada específicamente para lo que la vista necesita mostrar, sin relación directa con ninguna tabla de la base.
- **Model binding de listas indexadas**: cuando un formulario tiene campos con nombres como `celdas[0].Fecha`, `celdas[1].Fecha`, ASP.NET Core los junta automáticamente en una `List<T>` en el parámetro del método del Controller, siempre que los índices sean consecutivos empezando en 0.
- **DateOnly y el cálculo del lunes de una semana**: para pasar de "una fecha cualquiera" a "el lunes de esa semana", se usa la diferencia entre el `DayOfWeek` de la fecha y `DayOfWeek.Monday`, con un ajuste (`% 7`) para que funcione también cuando la fecha cae en domingo (que en .NET es el día 0, antes del lunes).

### Errores comunes / trampas en esta parte

- **Poner código C# suelto dentro de los atributos de una etiqueta HTML dentro de un `<select>`/`<option>` en Razor.** Da el error `RZ1031`. La solución fue usar un `if`/`else` completo para decidir si el `<option>` lleva o no el atributo `selected`, en vez de intentar meter una expresión condicional directamente en el atributo.
- **Usar el formato de fecha por defecto (`DateOnly.ToString()`) al armar una URL de redirección.** El formato por defecto depende de la configuración regional del servidor (podía salir `09/07/2026` en vez de `2026-09-07`), lo que es ambiguo entre día y mes según el idioma configurado. Hay que formatear siempre explícitamente con `.ToString("yyyy-MM-dd")` para que sea el mismo formato sin importar dónde corra la aplicación.
- **Al probar formularios con `curl` en una página que ya tiene otro formulario (como el de "Cerrar sesión" en el layout), extraer el token antiforgery equivocado.** Cada `<form>` de la página tiene su propio token oculto; si hay más de un formulario, hay que asegurarse de tomar el que corresponde al formulario que se está enviando.

---

## Próximos pasos si quisiera ampliar este proyecto

Estas son ideas de funcionalidades que quedaron **fuera del alcance a propósito** del MVP, para no sobre-construir algo que el prompt original no pedía. Quedan anotadas como posibles próximos pasos:

- **Roles y permisos diferenciados.** Hoy hay un solo tipo de usuario (admin). Se podría agregar un rol de "supervisor" que solo pueda ver la vista semanal sin editarla, o un rol por Operador para que cada uno vea únicamente sus propios turnos.
- **Registro de usuarios y recuperación de contraseña.** Identity ya lo soporta (páginas de Registro, "Olvidé mi contraseña", confirmación de email); no se implementó porque el prompt pedía solo un admin de prueba.
- **Notificaciones.** Avisar a un Operador (por email o alguna otra vía) cuando se le asigna o cambia un turno.
- **Historial de cambios en las asignaciones.** Guardar quién cambió qué asignación y cuándo, útil para auditar cambios de último momento.
- **Restricciones de negocio sobre las asignaciones.** Por ejemplo, impedir que un Operador quede asignado a dos turnos que se superponen en horario el mismo día (se decidió explícitamente no agregar esto en el Paso 5, para mantener la carga flexible).
- **Vista mensual o rango de fechas configurable.** Hoy la grilla es semana por semana; se podría extender a mostrar un mes completo o un rango arbitrario.
- **Exportar la asignación semanal a PDF o Excel**, para imprimir o compartir fuera del sistema.
- **Reemplazar SQLite por PostgreSQL o SQL Server** si el proyecto pasara a un entorno con múltiples usuarios escribiendo al mismo tiempo (ver la comparación en el Paso 1).
- **Actualizar las dependencias con vulnerabilidades conocidas** (`Microsoft.OpenApi`, `SQLitePCLRaw.lib.e_sqlite3`, señaladas por NuGet durante el desarrollo) a versiones más nuevas cuando estén disponibles.
