USE db_club_deportivo;

INSERT INTO roles (RolUsu, NomRol) VALUES
(120, 'Administrador'),
(121, 'Empleado');

INSERT INTO usuario (NombreUsu, PassUsu, RolUsu) VALUES
('Mari2023', '123456', 120),
('admin', '1234', 120);

