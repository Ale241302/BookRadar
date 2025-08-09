📚 BookRadar

Aplicación web en ASP.NET Core MVC con Entity Framework Core que permite buscar libros por autor usando la API de Open Library y almacenar un historial de búsquedas en SQL Server.

🚀 Pasos para ejecutar el proyecto

A continuación se detallan los pasos necesarios para configurar y ejecutar el proyecto en un entorno local para evaluación:

1. Clonar el repositorio

   git clone https://github.com/Ale241302/BookRadar.git
   cd BookRadar

2. Requisitos previos

   .NET 8 SDK (https://dotnet.microsoft.com/download)

   SQL Server 2019+ (https://www.microsoft.com/sql-server)

   Visual Studio 2022

   Git

3. Configurar la base de datos

   Ejecutar el script SQL llamado BookRadar.sql incluido en la carpeta sql

4. Configurar appsettings.Development.json

   Edita el archivo en la parte de Server con el valor de tu servidor donde esta la base de datos:

   {
   "ConnectionStrings": {
   "Default": "Server=.;Database=BookRadar;Integrated Security=true;TrustServerCertificate=True"
   },
   "Logging": {
   "LogLevel": {
   "Default": "Information",
   "Microsoft.AspNetCore": "Warning"
   }
   },
   "AllowedHosts": "\*"
   }

5. Restaurar dependencias

   dotnet restore

6. Ejecutar la aplicación

   dotnet run

   La aplicación estará disponible en:
   http://localhost:puerto_habilitado

🎨 Decisiones de diseño

-El diseño de BookRadar se centró en tres pilares: usabilidad, simplicidad y adaptabilidad.

-Paleta y estética

        Se utilizan CSS Custom Properties (variables) para toda la UI:

        Marca (verdes)
        --brand-50:#eef5f1, --brand-100:#d9eae0, --brand-200:#b9d8c3,
        --brand-300:#8fc19c, --brand-400:#69ae7f,
        --brand-500:#4a9667 (principal), --brand-600:#3d7a55, --brand-700:#2f5d42, --brand-800:#224131, --brand-900:#172b22

        Acento (ámbar)
        --accent-100:#fdebd2, --accent-300:#f4c38a, --accent-500:#e49a3a

        Base de interfaz
        --page:#f8faf9, --surface:#ffffff, --text:#2a2f2b, --muted:#6b736d, --border:#e8ece9,
        --shadow:0 6px 18px rgba(23,43,34,.06), 0 2px 6px rgba(23,43,34,.04)

-Componentes destacados:

        Botón primario: .btn-brand usa --brand-500 con hover en --brand-600.

        Badges: .bg-accent con --accent-500; .bg-accent-subtle con --accent-100.

        Tablas: encabezado con --brand-50 y texto --brand-800. Hover suave en filas.

        Cards: .card-soft con --surface, radios suaves y --shadow.

        Dark Mode
        Implementado con variables CSS y toggle persistente:

        Activación por atributo: html[data-theme="dark"] redefine la escala:

        --page:#0f1512, --surface:#121a16, --text:#e7ede9, --muted:#b9c2bb, --border:#1f2a24

        Ajuste de marca para contraste: --brand-500:#3c8a65 (principal en dark), --brand-600:#5aa981, etc.

        Acento en dark: --accent-500:#d49035

        Detección inicial: si no hay preferencia guardada, se respeta prefers-color-scheme.

        Persistencia: el botón “Cambiar tema” guarda la elección en localStorage (br-theme).

-Accesibilidad:

        Hover en tablas cambia el fondo a claro y fuerza texto negro para legibilidad.

        Se ajustan controles de DataTables (inputs, paginación) al esquema dark.

        UX (Experiencia de Usuario)
        Flujo claro: principal Buscador → secundaria Historial.

        Validaciones inmediatas:

        Frontend: limpieza de caracteres en el input de autor y patrón de solo letras/espacios.

        Backend: Data Annotations.

        Acción visible: spinner y deshabilitado de botón durante la búsqueda.

        Responsive centrado en tareas: formularios y tablas priorizan lectura y acción.

        UI (Interfaz de Usuario)
        Bootstrap 5 para maquetación y componentes base.

-Tipografías:

        Inter para texto por legibilidad.

        Merriweather para títulos con carácter editorial.

        Tablas responsivas: table-responsive + DataTables (paginación, orden, búsqueda y localización ES).

        Componentes reutilizables: layout compartido para navegación coherente, navbar con marca y toggle de tema.

        Botones full-width en móvil: clase .btn-responsive (activada con @media (max-width:1080px)).

🔧 Mejoras pendientes

    Para futuras versiones, se proponen las siguientes mejoras:

    -Escalabilidad

        Paginación y filtrado en servidor para manejar grandes volúmenes de datos.

        Caching de resultados frecuentes para reducir llamadas a la API de Open Library.

        API interna para exponer las búsquedas a otros servicios.

    -Experiencia de Usuario
        Búsqueda avanzada por título, editorial o año.

        Exportar historial a CSV/Excel desde la interfaz.

    -Sistema de autenticación:

        Login para controlar el acceso y registrar qué consultas hace cada usuario.

        Registro de nuevos usuarios para que puedan usar el buscador y la API del sitio.
