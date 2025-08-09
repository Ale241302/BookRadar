📚 BookRadar
Es una aplicación web desarrollada en ASP.NET Core MVC con Entity Framework Core que permite buscar libros por autor utilizando la API de Open Library y almacenar un historial de búsquedas en base de datos.

🚀 Pasos para ejecutar el proyecto
A continuación se detallan los pasos necesarios para configurar y ejecutar el proyecto en un entorno local para evaluación:

Clonar el repositorio
git clone https://github.com/usuario/BookRadar.git
cd BookRadar

Requisitos previos

.NET 8 SDK (https://dotnet.microsoft.com/download)

SQL Server 2019+ (https://www.microsoft.com/sql-server)

Visual Studio 2022 o Visual Studio Code

Git

Configurar la base de datos
Ejecutar el script SQL incluido en la carpeta sql/:

También puedes usar migraciones de Entity Framework Core:
dotnet ef database update

Configurar appsettings.Development.json
Copia el archivo de ejemplo y ajusta la conexión:
cp appsettings.Development.example.json appsettings.Development.json
Edita el archivo:
{
"ConnectionStrings": {
"Default": "Server=.;Database=BookRadar;Trusted_Connection=True;TrustServerCertificate=True"
},
"Logging": {
"LogLevel": {
"Default": "Information",
"Microsoft.AspNetCore": "Warning"
}
},
"AllowedHosts": "\*"
}

Restaurar dependencias
dotnet restore

Ejecutar la aplicación
dotnet run --project src/BookRadar.Web
La aplicación estará disponible en:
http://localhost:puerto_habilitado

🎨 Decisiones de diseño
El diseño de BookRadar se centró en tres pilares principales: usabilidad, simplicidad y adaptabilidad.

Colores y estética

Color principal (Brand): Azul suave (#007bff) para transmitir confianza y profesionalidad.

Color secundario: Gris claro para fondos y separadores, minimizando distracciones.

Dark Mode: Implementado para reducir fatiga visual en entornos con poca luz.

UX (Experiencia de Usuario)

Flujo lineal y claro: La página principal es el buscador; la página secundaria es el historial.

Validaciones inmediatas: Uso de validaciones en frontend con JavaScript y en backend con Data Annotations.

Botones responsivos: Ocupan el 100% del ancho en dispositivos móviles para facilitar la interacción.

UI (Interfaz de Usuario)

Bootstrap 5: Para una maquetación limpia y consistente.

Tipografías:

Inter para textos por su legibilidad.

Merriweather para títulos, dando un toque editorial.

Tablas responsivas: Uso de table-responsive para adaptarse a pantallas pequeñas.

Componentes reutilizables: Layout compartido para navegación coherente.

🔧 Mejoras pendientes
Para futuras versiones, se proponen las siguientes mejoras:

Escalabilidad

Paginación y filtrado en servidor para manejar grandes volúmenes de datos.

Caching de resultados frecuentes para reducir llamadas a la API de Open Library.

API interna para exponer las búsquedas a otros servicios.

Experiencia de Usuario

Búsqueda avanzada por título, editorial o año.

Exportar historial a CSV/Excel desde la interfaz.

Notificaciones en tiempo real cuando se agregan resultados nuevos.

Optimización técnica

Implementar Stored Procedures para consultas críticas.

Tests automatizados (unitarios y de integración) para asegurar calidad.

Dockerización del proyecto para despliegue rápido en cualquier entorno.

📜 Licencia
Este proyecto es de uso académico y no comercial. Derechos reservados © 2025.
