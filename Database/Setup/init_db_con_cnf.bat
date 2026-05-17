@echo off
setlocal EnableExtensions

set "CNF_FILE=%~dp0mysql-client.cnf"
set "SCRIPTS_DIR=%~dp0..\Scripts"

if not exist "%CNF_FILE%" (
    echo No se encontro mysql-client.cnf
    echo Copie mysql-client.cnf.example a mysql-client.cnf y edite user/password.
    pause
    exit /b 1
)

echo =======================================
echo Ejecutando Scripts (mysql-client.cnf)...
echo =======================================
echo.

for %%f in ("%SCRIPTS_DIR%\*.sql") do (
    echo Ejecutando %%~nxf ...
    mysql --defaults-extra-file="%CNF_FILE%" --default-character-set=utf8mb4 --batch < "%%f"
    if errorlevel 1 (
        echo [ERROR] Fallo en %%~nxf
        pause
        exit /b 1
    )
)

echo.
echo Base de datos inicializada OK.
pause
