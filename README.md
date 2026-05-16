# TP Club Deportivo

## Descripción general

Proyecto integrador desarrollado en C# utilizando Windows Forms y MySQL, orientado a la gestión de un club deportivo.

El sistema permite administrar:
- socios
- usuarios
- actividades
- pagos
- cuotas
- carnets
- turnos
- asistencia de profesores

El proyecto se encuentra organizado en diferentes fases de análisis, modelado e implementación.

---

# Tecnologías utilizadas

- C#
- .NET
- Windows Forms
- MySQL
- DBeaver
- Visual Studio Code
- Git

---

# Base de datos

## Motor utilizado

MySQL

## Herramienta de administración

DBeaver

Se utiliza para:
- ejecutar scripts SQL
- visualizar tablas
- probar Store Procedures
- administrar la base de datos

---

# Nombre de la base de datos

```sql
db_club_deportivo
```

---

# Scripts SQL

Los scripts SQL se encuentran organizados según su responsabilidad.

## DDL

Contiene:
- creación de base de datos
- creación de tablas
- relaciones

Archivos:
- 01_create_database.sql
- 02_create_tables.sql

---

## DML

Contiene:
- inserción de datos
- modificaciones
- carga inicial

Archivos:
- 03_insert_usuarios.sql

---

## SP (Stored Procedures)

Contiene:
- procedimientos almacenados
- lógica reutilizable SQL

Archivos:
- 04_sp_login.sql
- 05_sp_nuevo_socio.sql

---

# Orden de ejecución de scripts

1. 01_create_database.sql
2. 02_create_tables.sql
3. 03_insert_usuarios.sql
4. 04_sp_login.sql
5. 05_sp_nuevo_socio.sql

---

# Configuración del framework .NET

## Framework inicial

El proyecto fue creado inicialmente utilizando:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
```

---

## Problema detectado

Al intentar instalar el paquete de conexión MySQL se presentaron incompatibilidades con algunas dependencias del framework .NET 10.

Error detectado:

```text
No se puede resolver 'Microsoft.NETCore.App.Host.win-x64'
```

---

## Solución aplicada

Se decidió migrar el proyecto a una versión LTS (Long Term Support) más estable y ampliamente compatible.

Archivo modificado:

```text
TP_ClubDeportivo.csproj
```

Cambio realizado:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
```

por:

```xml
<TargetFramework>net8.0-windows</TargetFramework>
```

---

# Instalación de .NET 8 SDK

En caso de no tener instalado el SDK de .NET 8 se debe ejecutar:

```bash
winget install Microsoft.DotNet.SDK.8
```

---

# Verificación de SDK instalados

Luego de instalar .NET 8 se recomienda verificar:

```bash
dotnet --list-sdks
```

Debe aparecer una versión similar a:

```text
8.0.xxx
```

---

# Restauración del proyecto

Luego de modificar el framework se ejecuta:

```bash
dotnet restore
```

Este comando:
- restaura dependencias
- actualiza paquetes
- recompone la solución
- valida compatibilidad del framework

---

# Configuración de NuGet

Durante la instalación de paquetes se detectó que no existían fuentes configuradas en NuGet.

Verificación realizada:

```bash
dotnet nuget list source
```

Resultado detectado:

```text
No se encontró ningún origen.
```

---

# Agregado del repositorio oficial de NuGet

Se agregó manualmente el repositorio oficial utilizando:

```bash
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

---

# Verificación de fuentes configuradas

Luego del agregado se recomienda verificar nuevamente:

```bash
dotnet nuget list source
```

Debe aparecer una salida similar a:

```text
nuget.org
https://api.nuget.org/v3/index.json
```

---

# Instalación del paquete MySQL para C#

Para permitir la conexión entre C# y MySQL se instala:

```bash
dotnet add package MySqlConnector --version 2.3.7
```

---

# Motivo de uso de MySqlConnector

Se utiliza `MySqlConnector` debido a:
- mayor compatibilidad con .NET moderno
- mejor estabilidad
- instalación más simple
- soporte activo
- mejor integración con WinForms

---

# Objetivo de esta configuración

Permitir:
- conexión entre C# y MySQL
- utilización de MySqlConnection
- ejecución de Store Procedures
- implementación de DAOs
- desarrollo del login del sistema

---

---

# Configuración de MariaDB / MySQL

## Motor utilizado

El proyecto utiliza MariaDB como motor de base de datos.

MariaDB mantiene compatibilidad con MySQL, por lo que:
- DBeaver funciona normalmente
- MySqlConnector funciona correctamente
- los scripts SQL son compatibles
- los Stored Procedures pueden ejecutarse sin modificaciones

---

# Verificación de instalación del motor SQL

Se recomienda verificar desde terminal:

```bash
mysql --version
```

Salida esperada:

```text
mysql.exe from 11.x-MariaDB
```

---

# Configuración de variable de entorno PATH

Para poder ejecutar scripts automáticos desde terminal o archivos `.bat`, es necesario que MariaDB/MySQL se encuentre agregado al PATH de Windows.

Ruta utilizada:

```text
C:\Program Files\MariaDB 11.6\bin
```

---

# Configuración de PATH en Windows

Ruta:

```text
Configuración avanzada del sistema
→ Variables de entorno
→ Path
→ Nuevo
```

Agregar:

```text
C:\Program Files\MariaDB 11.6\bin
```

---

# Verificación posterior al PATH

Luego de configurar el PATH:
- cerrar VS Code
- cerrar terminales abiertas
- abrir nuevamente la terminal

y verificar:

```bash
mysql --version
```

---

# Automatización de scripts SQL

El proyecto incluye un script de automatización para inicializar la base de datos sin necesidad de utilizar DBeaver manualmente.

Archivo:

```text
setup/init_db.bat
```

---

# Funcionalidad de init_db.bat

El archivo:
- recorre automáticamente todos los `.sql`
- ejecuta los scripts en orden numérico
- inicializa la base de datos
- crea tablas
- inserta datos iniciales
- crea Stored Procedures

---

# Estructura utilizada

```text
ScriptsSQL/
│
├── 01_create_database.sql
├── 02_create_tables.sql
├── 03_insert_usuarios.sql
├── 04_sp_login.sql
└── 05_sp_nuevo_socio.sql
```

---

# Ejecución del script automático

Desde terminal:

```bash
./init_db.bat
```

o ejecutando doble click sobre:

```text
setup/init_db.bat
```

---

# Requisito para la ejecución automática

Es necesario:
- tener MariaDB/MySQL instalado
- tener `mysql.exe` disponible en PATH
- conocer el usuario y contraseña del motor SQL

---

# Beneficios de esta automatización

Permite:
- evitar ejecución manual en DBeaver
- simplificar instalación del entorno
- facilitar configuración entre integrantes del grupo
- recrear rápidamente la base de datos
- mantener scripts SQL versionados dentro del proyecto

---

# Conexión a la base de datos

La conexión se centralizará en:

```text
Data/Conexion.cs
```

Esta clase será utilizada posteriormente por:
- UsuarioDAO
- SocioDAO
- Login
- formularios