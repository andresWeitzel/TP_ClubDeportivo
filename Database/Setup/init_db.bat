@echo off
setlocal EnableExtensions

rem ============================================================
rem Configuracion MySQL (debe coincidir con Data/Conexion.cs)
rem Dejar MYSQL_PASSWORD vacio si root no tiene clave.
rem Si tiene clave, asignarla aqui UNA sola vez:
rem   set "MYSQL_PASSWORD=tu_clave"
rem ============================================================
set "MYSQL_HOST=localhost"
set "MYSQL_PORT=3306"
set "MYSQL_USER=root"
set "MYSQL_PASSWORD="

set "SCRIPTS_DIR=%~dp0..\Scripts"

set "MYSQL_ARGS=-h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --default-character-set=utf8mb4 --batch"
if not "%MYSQL_PASSWORD%"=="" (
    set "MYSQL_ARGS=%MYSQL_ARGS% -p%MYSQL_PASSWORD%"
)

echo =======================================
echo Ejecutando Scripts de Base de Datos...
echo Host: %MYSQL_HOST%  Usuario: %MYSQL_USER%
echo =======================================
echo.

for %%f in ("%SCRIPTS_DIR%\*.sql") do (
    echo Ejecutando %%~nxf ...
    mysql %MYSQL_ARGS% < "%%f"
    if errorlevel 1 (
        echo.
        echo [ERROR] Fallo al ejecutar %%~nxf
        echo Verifique que MySQL este en ejecucion y las credenciales.
        pause
        exit /b 1
    )
)

echo.
echo ===============================
echo Base de datos inicializada OK.
echo ===============================
pause
