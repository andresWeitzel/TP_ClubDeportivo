USE db_club_deportivo;

CREATE TABLE roles (
    RolUsu INT PRIMARY KEY,
    NomRol VARCHAR(30)
);

CREATE TABLE usuario (
    CodUsu INT PRIMARY KEY AUTO_INCREMENT,
    NombreUsu VARCHAR(20),
    PassUsu VARCHAR(15),
    RolUsu INT,
    Activo BOOLEAN DEFAULT TRUE,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_usuario FOREIGN KEY (RolUsu) REFERENCES roles(RolUsu)
);

CREATE TABLE socios (
    id_socio INT PRIMARY KEY AUTO_INCREMENT,
    dni VARCHAR(20) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50),
    direccion VARCHAR(150),
    estado_cuota VARCHAR(30),
    fecha_alta DATETIME DEFAULT CURRENT_TIMESTAMP
);