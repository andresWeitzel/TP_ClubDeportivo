
# Documento de Análisis y Alcance

**Fuente oficial (versión 1.0 — Abril 2026):**  
[`doc/definiciones_club_deportivo/analisis_club_deportivo.docx`](doc/definiciones_club_deportivo/analisis_club_deportivo.docx)

Incluye diagrama de clases, modelo entidad-relación y casos de uso UML 2.0. El resumen siguiente condensa el alcance funcional del TP; para el detalle completo (tablas, flujos y excepciones), consultar el documento Word.

## 1. Introducción

El sistema informatiza las operaciones administrativas, deportivas y de salud de un club con actividades para **socios** (acceso continuo con carnet, ficha médica y cuota mensual) y **visitantes** (ingreso diario con pago). Este análisis es la base de diseño y desarrollo del proyecto.

## 2. Alcance del sistema

| Módulo | Contenido |
|--------|-----------|
| **Gestión de usuarios** | Socios (carnet, ficha médica, estado de cuota) y visitantes (pago diario, sin carnet). |
| **Cuotas y pagos** | Cuotas mensuales con vencimiento; mora suspende acceso hasta regularizar; pagos de visitantes. |
| **Personal (profesores)** | Horarios, especialidades, asistencia diaria, rutinas personalizadas, liquidación mensual (último día hábil). |
| **Nutrición** | Turnos semanales, fichas médicas y carga de actividad física permitida. |
| **Reportes y control** | Cuotas por vencer / vencidas, asistencia de profesores, seguimiento general. |

## 3. Requerimientos funcionales

| ID | Descripción | Módulo |
|----|-------------|--------|
| RF-01 | Registrar socios con datos personales, foto y carnet | Usuarios |
| RF-02 | Registrar visitantes con pago diario | Usuarios |
| RF-03 | Emitir y reimprimir carnets de socio | Usuarios |
| RF-04 | Registrar pago de cuota mensual con vencimiento | Cuotas |
| RF-05 | Bloquear acceso a socio con cuota vencida (mora) | Cuotas |
| RF-06 | Registrar pago de mora y reactivar acceso | Cuotas |
| RF-07 | Registrar pago diario de visitante | Pagos |
| RF-08 | ABM de profesores con especialidad y horarios | Personal |
| RF-09 | Registrar asistencia diaria de profesores | Personal |
| RF-10 | Confeccionar rutinas personalizadas por alumno | Personal |
| RF-11 | Liquidar haberes mensuales (último día hábil) | Personal |
| RF-12 | Gestionar turnos semanales de nutrición | Nutrición |
| RF-13 | Administrar fichas médicas de socios | Nutrición |
| RF-14 | Definir carga de actividad física permitida | Nutrición |
| RF-15 | Listado diario de cuotas próximas a vencer | Reportes |
| RF-16 | Listado de socios en mora | Reportes |
| RF-17 | Reporte de asistencia de profesores | Reportes |

## 4. Modelo conceptual (resumen)

**Clases principales:** `Persona` → `Socio` / `Visitante`; `Cuota`, `Pago`, `Carnet`, `FichaMedica`, `Profesor`, `HorarioClase`, `Asistencia`, `Rutina`, `TurnoNutricion`, `Liquidacion`, `Actividad`.

**Persistencia (ER):** tablas `socios`, `visitantes`, `actividades`, `cuotas`, `pagos`, `profesores`, `horarios_actividad`, `asistencias`, `rutinas`, `fichas_medicas`, `turnos_nutricion`, `liquidaciones`, `carnets` (ver `Database/Scripts/01_DDL.sql`).

## 5. Casos de uso implementados / planificados

| CU | Nombre | Actor | Pantalla / módulo en este repo |
|----|--------|-------|-------------------------------|
| CU-01 | Registrar socio | Administrador | `FormSocios` |
| CU-02 | Registrar visitante | Administrador | `FormVisitantes` |
| RF-03 | Emitir / renovar carnet | Administrador | `FormCarnets` |
| CU-03 | Cobrar cuota mensual | Administrador | `FormCobroCuota` |
| CU-04 | Controlar vencimiento de cuotas | Sistema | `FormReportes` + `sp_controlar_vencimiento_cuotas` |
| CU-05 | Firmar asistencia | Profesor | Pendiente (datos en BD) |
| CU-06 | Confeccionar rutina | Profesor | Pendiente (datos en BD) |
| CU-07 | Gestionar turno de nutrición | Admin / Nutricionista | Pendiente (datos en BD) |
| CU-08 | Liquidar haberes | Administrador | `FormLiquidarHaberes` |
| CU-09 | Generar reportes | Administrador / Empleado | `FormReportes` (RF-15, RF-16; RF-17 solo Administrador) |

Detalle de flujos, precondiciones y excepciones (E1, etc.) en el [documento Word](doc/definiciones_club_deportivo/analisis_club_deportivo.docx).

### Control de acceso por rol

La UI filtra menú, panel principal y formularios según `Permisos.cs` y `Sesion.TieneRol`.

| Módulo | Administrador | Empleado | Profesor | Nutricionista | Socio / Visitante |
|--------|:-------------:|:--------:|:--------:|:-------------:|:-----------------:|
| Socios (CU-01) | ✓ | ✓ | — | — | — |
| Visitantes (CU-02) | ✓ | ✓ | — | — | — |
| Cobrar cuota (CU-03) | ✓ | ✓ | — | — | — |
| Carnets (RF-03) | ✓ | ✓ | — | — | — |
| Firmar asistencia (CU-05) | ✓ | — | ✓ | — | — |
| Rutinas (CU-06) | ✓ | — | ✓ | — | — |
| Turnos nutrición (CU-07) | ✓ | — | — | ✓ | — |
| Reportes cuotas (CU-09) | ✓ | ✓ | — | — | — |
| Reporte asistencia RF-17 | ✓ | — | — | — | — |
| Liquidar haberes (CU-08) | ✓ | — | — | — | — |

**Socio** y **Visitante** no ingresan a la app de gestión (solo personal interno). Usuarios de prueba en `02_DML.sql`: `admin`, `empleado1`, `juan_prof`, `maria_nutri`, etc.

## 6. Glosario

| Término | Definición |
|---------|------------|
| Socio | Inscripto con carnet, cuota mensual y acceso continuo. |
| Visitante | Asistencia ocasional con pago diario. |
| Cuota | Cargo mensual para mantener el acceso activo. |
| Mora | Cuota vencida sin pago; acceso suspendido. |
| Carnet | Identificación del socio. |
| Rutina | Plan de ejercicios asignado por un profesor. |
| Ficha médica | Datos de salud y restricciones del socio. |
| Liquidación | Cálculo y pago de haberes a profesores. |

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


# Objetivo de esta configuración

Permitir:
- conexión entre C# y MySQL
- utilización de MySqlConnection
- ejecución de Store Procedures
- implementación de DAOs
- desarrollo del login del sistema


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
