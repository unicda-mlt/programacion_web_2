# Punto Académico UNICDA - API

- [Acceso a la API](#url-acceso-a-la-api)
- [Arquitectura por Capas](#arquitectura-por-capas)
   - [Resumen](#resumen)
   - [Diagrama de Arquitectura](#diagrama-de-arquitectura)
   - [Lógica de Capas](#lógica-de-capas)
      - [1. Capa de Dominio](#1-capa-de-dominio)
      - [2. Capa de Datos](#2-capa-de-datos)
      - [3. Capa de Negocio](#3-capa-de-negocio)
      - [4. Capa API](#4-capa-api)
- [Migraciones a Base de Datos](#migraciones-a-base-de-datos)
   - [Crear una Nueva Migración](#crear-una-nueva-migración)
   - [Aplicar Migraciones a la Base de Datos](#aplicar-migraciones-a-la-base-de-datos)

---

<a name="url-acceso-a-la-api"></a>
## URL Acceso a la API

- Desarrollo:
   - http: [http://localhost:5138](http://localhost:5138)
   - https: [https://localhost:7192](https://localhost:7192)
   - Swagger http: [http://localhost:5138/swagger/index.html](http://localhost:5138/swagger/index.html)
   - Swagger https: [https://localhost:7192/swagger/index.html](https://localhost:7192/swagger/index.html)

El archivo de configuración de acceso a la API se encuentra en `API/Properties/launchSettings.json`.

---

<a name="arquitectura-por-capas"></a>
## Arquitectura por Capas
<a name="resumen"></a>
### Resumen

Esta solución utiliza una arquitectura limpia y escalable basada en la separación de responsabilidades:

* **Capa API** (`API`): Maneja solicitudes y respuestas HTTP. Contiene únicamente los controladores.
* **Capa de Negocio** (`Business`): Implementa las reglas y servicios del negocio.
* **Capa de Datos** (`Data`): Gestiona el acceso a los datos, los repositorios y el DbContext de EF Core.
* **Capa de Dominio** (`Domain`): Contiene los modelos/entidades de dominio compartidos por las demás capas.

<a name="diagrama-de-arquitectura"></a>
### Diagrama de Arquitectura

```
[Controladores API] → [Servicios de Negocio] → [Repositorios de Datos] → [Base de Datos]
         |                    |                         |                
   (Usa DTOs)        (Lógica de negocio)      (Entity Framework)
```

<a name="lógica-de-capas"></a>
### Lógica de Capas

<a name="capa-de-dominio"></a>
#### 1. **Capa de Dominio**

* Solo contiene clases POCO (por ejemplo, `Usuario`, `Rol`), sin dependencias hacia otras capas.
* Es compartida por las capas de Datos y de Negocio.

<aname="capa-de-datos"></a>
#### 2. **Capa de Datos**

* Contiene:

  * `AppDbContext` (contexto de Entity Framework Core)
  * Interfaces e implementaciones de repositorios (por ejemplo, `IUsuarioRepository`, `UsuarioRepository`)
* Depende del Dominio, pero **no** de Negocio ni API.

<a name="capa-de-negocio"></a>
#### 3. **Capa de Negocio**

* Contiene:

  * Interfaces e implementaciones de servicios (por ejemplo, `IUsuarioService`, `UsuarioService`)
  * Llama a la capa de Datos a través de interfaces de repositorio.
* Depende de Dominio y Datos, pero **no** de API.

<a name="capa-api"></a>
#### 4. **Capa API**

* Solo contiene controladores (por ejemplo, `UsuariosController`).
* Inyecta y utiliza servicios de la capa de Negocio.

---

<a name="migraciones-a-base-de-datos"></a>
## Migraciones a Base de Datos

Para actualizar el esquema de tu base de datos, utiliza migraciones de Entity Framework Core.

**Importante:** Ejecuta los comandos de migración desde la capa API, ya que ahí está la configuración y cadena de conexión.

<a name="crear-una-nueva-migracion"></a>
### **Crear una Nueva Migración**

**1.** Abre la Consola del Administrador de Paquetes en Visual Studio
   (`Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes`)
   (`Tools > NuGet Package Manager > Package Manager Console`).

**2.** Selecciona Proyecto Predeterminado (`Default Project`) como `API`, desplegable en la parte superior de la consola.

**3.** Ejecuta:

   ```powershell
   Add-Migration <NombreMigracion> -StartupProject API -Project Data
   ```

   * Reemplaza `<NombreMigracion>` por el nombre que desees.

<a name="aplicar-la-migracion-a-la-base-de-datos"></a>
### **Aplicar Migraciones a la Base de Datos**

En la misma consola, ejecuta:

```powershell
Update-Database -StartupProject API -Project Data
```

## **Rollback de Migraciones**
- Si necesitas revertir hasta una migración específica, puedes usar:

```powershell
Update-Database -Migration <NombreMigracion> -StartupProject API -Project Data
```

- Para eliminar la última migración creada y restaurar el `ModelSnapshot`, puedes usar:

```powershell
Remove-Migration -StartupProject API -Project Data
```