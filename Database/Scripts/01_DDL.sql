DROP DATABASE IF EXISTS db_club_deportivo;

CREATE DATABASE db_club_deportivo;

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
    dni VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50),
    direccion VARCHAR(150),
    email VARCHAR(150),
    estado_cuota VARCHAR(30) DEFAULT 'AL_DIA',
    fecha_alta DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE actividades (
    id_actividad INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    cupo_maximo INT NOT NULL DEFAULT 20,
    precio_visitante DECIMAL(10,2) NOT NULL DEFAULT 50.00,
    activa BOOLEAN DEFAULT TRUE
);

CREATE TABLE visitantes (
    id_visitante INT PRIMARY KEY AUTO_INCREMENT,
    dni VARCHAR(20),
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100),
    telefono VARCHAR(50),
    actividad_id INT NOT NULL,
    fecha_ingreso DATETIME DEFAULT CURRENT_TIMESTAMP,
    pago_diario_monto DECIMAL(10,2) DEFAULT 0.00,
    CONSTRAINT fk_visitante_actividad FOREIGN KEY (actividad_id) REFERENCES actividades(id_actividad)
);

CREATE TABLE carnets (
    id_carnet INT PRIMARY KEY AUTO_INCREMENT,
    socio_id INT NOT NULL,
    numero VARCHAR(50) NOT NULL UNIQUE,
    fecha_emision DATE NOT NULL,
    fecha_vencimiento DATE NOT NULL,
    foto VARCHAR(255),
    CONSTRAINT fk_carnet_socio FOREIGN KEY (socio_id) REFERENCES socios(id_socio)
);

CREATE TABLE cuotas (
    id_cuota INT PRIMARY KEY AUTO_INCREMENT,
    socio_id INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    fecha_emision DATE NOT NULL,
    fecha_vencimiento DATE NOT NULL,
    estado VARCHAR(30) DEFAULT 'AL_DIA',
    en_mora BOOLEAN DEFAULT FALSE,
    CONSTRAINT fk_cuota_socio FOREIGN KEY (socio_id) REFERENCES socios(id_socio)
);

CREATE TABLE pagos (
    id_pago INT PRIMARY KEY AUTO_INCREMENT,
    tipo VARCHAR(30) NOT NULL,
    socio_id INT,
    cuota_id INT,
    visitante_id INT,
    monto DECIMAL(10,2) NOT NULL,
    fecha_pago DATETIME DEFAULT CURRENT_TIMESTAMP,
    medio_pago VARCHAR(50),
    concepto VARCHAR(150),
    CONSTRAINT fk_pago_socio FOREIGN KEY (socio_id) REFERENCES socios(id_socio),
    CONSTRAINT fk_pago_cuota FOREIGN KEY (cuota_id) REFERENCES cuotas(id_cuota),
    CONSTRAINT fk_pago_visitante FOREIGN KEY (visitante_id) REFERENCES visitantes(id_visitante)
);

CREATE TABLE profesores (
    id_profesor INT PRIMARY KEY AUTO_INCREMENT,
    dni VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50),
    email VARCHAR(150),
    especialidad VARCHAR(100),
    sueldo_base DECIMAL(12,2) DEFAULT 0.00
);

CREATE TABLE horarios_actividad (
    id_horario INT PRIMARY KEY AUTO_INCREMENT,
    profesor_id INT NOT NULL,
    actividad_id INT NOT NULL,
    dia_semana VARCHAR(30) NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    CONSTRAINT fk_horario_profesor FOREIGN KEY (profesor_id) REFERENCES profesores(id_profesor),
    CONSTRAINT fk_horario_actividad FOREIGN KEY (actividad_id) REFERENCES actividades(id_actividad)
);

CREATE TABLE asistencias (
    id_asistencia INT PRIMARY KEY AUTO_INCREMENT,
    profesor_id INT NOT NULL,
    fecha DATE NOT NULL,
    presente BOOLEAN DEFAULT TRUE,
    firma VARCHAR(150),
    CONSTRAINT fk_asistencia_profesor FOREIGN KEY (profesor_id) REFERENCES profesores(id_profesor)
);

CREATE TABLE rutinas (
    id_rutina INT PRIMARY KEY AUTO_INCREMENT,
    socio_id INT NOT NULL,
    profesor_id INT NOT NULL,
    descripcion TEXT,
    fecha_creacion DATE DEFAULT CURRENT_DATE,
    observaciones TEXT,
    CONSTRAINT fk_rutina_socio FOREIGN KEY (socio_id) REFERENCES socios(id_socio),
    CONSTRAINT fk_rutina_profesor FOREIGN KEY (profesor_id) REFERENCES profesores(id_profesor)
);

CREATE TABLE fichas_medicas (
    id_ficha INT PRIMARY KEY AUTO_INCREMENT,
    socio_id INT NOT NULL UNIQUE,
    peso DECIMAL(5,2),
    altura DECIMAL(5,2),
    alergias TEXT,
    medicacion TEXT,
    observaciones TEXT,
    carga_permitida VARCHAR(200),
    CONSTRAINT fk_ficha_socio FOREIGN KEY (socio_id) REFERENCES socios(id_socio)
);

CREATE TABLE nutricionistas (
    id_nutricionista INT PRIMARY KEY AUTO_INCREMENT,
    dni VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50),
    email VARCHAR(150),
    matricula VARCHAR(100)
);

CREATE TABLE turnos_nutricion (
    id_turno INT PRIMARY KEY AUTO_INCREMENT,
    socio_id INT NOT NULL,
    nutricionista_id INT NOT NULL,
    fecha DATE NOT NULL,
    hora TIME NOT NULL,
    estado VARCHAR(30) DEFAULT 'DISPONIBLE',
    CONSTRAINT fk_turno_socio FOREIGN KEY (socio_id) REFERENCES socios(id_socio),
    CONSTRAINT fk_turno_nutricionista FOREIGN KEY (nutricionista_id) REFERENCES nutricionistas(id_nutricionista)
);

CREATE TABLE liquidaciones (
    id_liquidacion INT PRIMARY KEY AUTO_INCREMENT,
    profesor_id INT NOT NULL,
    mes INT NOT NULL,
    anio INT NOT NULL,
    monto_bruto DECIMAL(12,2) NOT NULL,
    descuentos DECIMAL(12,2) DEFAULT 0.00,
    monto_neto DECIMAL(12,2) NOT NULL,
    fecha_pago DATE,
    estado VARCHAR(30) DEFAULT 'PENDIENTE',
    CONSTRAINT fk_liquidacion_profesor FOREIGN KEY (profesor_id) REFERENCES profesores(id_profesor)
);
