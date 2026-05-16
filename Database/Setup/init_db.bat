@echo off

set MYSQL_USER=root

echo =======================================
echo Ejecutando Scripts de Base de Datos...
echo =======================================

for %%f in (..\Scripts\*.sql) do (
    echo Ejecutando %%f ...
    mysql -u %MYSQL_USER% -p < %%f
)

echo.
echo ===============================
echo Fin de Ejecución Scripts de Base de Datos...
echo ===============================

pause