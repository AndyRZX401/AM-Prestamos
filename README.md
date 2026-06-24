# AM Prestamos (AM Financial System)

![AM Prestamos Logo](screenshots/logo_placeholder.png)

**AM Prestamos** es un sistema integral de escritorio para la gestión de préstamos personales y grupos de ahorro (S.A.M.). Está diseñado con una arquitectura robusta en **.NET 10 (WPF)** y bases de datos **SQL Server** mediante **Entity Framework Core**.

## 🚀 Características Principales

*   **Autenticación Segura:** Sistema de inicio de sesión con encriptación SHA256 y mecanismo de recuperación con llave maestra.
*   **Dashboard Dinámico:** Interfaz de usuario premium con animaciones, diseño neumórfico y modo oscuro.
*   **Módulo de S.A.M. (Sistema de Ahorro Mutuo):** Gestión de clientes, historiales, cobros semanales y visualización de estados inactivos.
*   **Módulo de Préstamos:** Creación de préstamos, cálculo automático de cuotas, seguimiento de mora y pagos.
*   **Generación de Documentos:** Creación de Contratos de Préstamo imprimibles y Recibos/Tickets de Pago detallados.
*   **Protección Anti-fraude:** Sincronización de hora en tiempo real mediante API externa para evitar manipulaciones de fechas locales.

## 🛠️ Tecnologías Utilizadas

*   **Lenguaje:** C# 14 / .NET 10
*   **Interfaz de Usuario:** WPF (Windows Presentation Foundation) con XAML puro (sin librerías de terceros pesadas).
*   **Base de Datos:** Microsoft SQL Server Express.
*   **ORM:** Entity Framework Core 10.0.
*   **Seguridad:** Hashing SHA256 para contraseñas.

## 📋 Requisitos del Sistema

*   Sistema Operativo: Windows 10 o Windows 11 (64-bits).
*   [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/) instalado.
*   [SQL Server Express](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) (o superior) ejecutándose en `.\SQLEXPRESS`.

## ⚙️ Instalación y Configuración

1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/tu-usuario/am-prestamos.git
    cd am-prestamos
    ```

2.  **Configurar Base de Datos:**
    *   Asegúrate de que tu instancia local de SQL Server esté corriendo en `.\SQLEXPRESS`.
    *   No es necesario ejecutar scripts manuales: el sistema utiliza migraciones automáticas (`context.Database.MigrateAsync()`) al iniciar la aplicación para crear y actualizar la estructura de la base de datos `ElPretamitaDB`.
    *   *Opcional:* Si deseas recrear la base de datos manualmente, puedes usar el script de inicialización ubicado en `database/Init_Database.sql`.

3.  **Compilar y Ejecutar:**
    *   Abre la solución `AMPrestamos.sln` en Visual Studio 2026+.
    *   Establece el proyecto como inicio y presiona `F5`.

## 🔐 Credenciales por Defecto

Al crearse la base de datos por primera vez, se genera un usuario administrador por defecto:
*   **Usuario:** `admin`
*   **Contraseña:** `admin1234`
*(Se recomienda cambiar esta contraseña después del primer inicio de sesión).*

## 📂 Estructura del Proyecto

*   `Assets/`: Imágenes, iconos y recursos visuales.
*   `DataBase/`: Configuración del contexto de EF Core y Modelos de Entidad.
*   `Helpers/`: Clases de utilidad (ej. `SecurityHelper`).
*   `Services/`: Servicios externos (ej. `TimeService`).
*   `Views/`: Ventanas y controles de usuario XAML divididos por módulos (`Login`, `Dashboard`, `Prestamos`, `Sam`, `Compartido`).
*   `database/`: Scripts de base de datos.
*   `docs/`: Documentación adicional.
*   `releases/`: Ejecutables finales listos para distribuir.

## 📄 Licencia y Derechos

Copyright © 2026 AM Index. Todos los derechos reservados.

---
**Desarrollado para la administración eficiente y segura del capital financiero.**
