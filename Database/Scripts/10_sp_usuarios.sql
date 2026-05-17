USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - USUARIOS / AUTENTICACION
-- ============================================

DROP PROCEDURE IF EXISTS sp_obtener_usuario_por_username;

DELIMITER $$

CREATE PROCEDURE sp_obtener_usuario_por_username(
    IN p_username VARCHAR(50)
)
BEGIN
    SELECT
        u.CodUsu AS id_usuario,
        u.NombreUsu AS username,
        u.PassUsu AS password,
        r.NomRol AS rol,
        u.FechaRegistro AS fecha_registro
    FROM usuario u
    INNER JOIN roles r ON u.RolUsu = r.RolUsu
    WHERE u.NombreUsu = p_username
    LIMIT 1;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS IngresoLogin;

DELIMITER $$

CREATE PROCEDURE IngresoLogin(
    IN Usu VARCHAR(20),
    IN Pass VARCHAR(15)
)
BEGIN
    SELECT
        u.CodUsu AS id_usuario,
        u.NombreUsu AS username,
        u.PassUsu AS password,
        r.NomRol AS rol,
        u.FechaRegistro AS fecha_registro
    FROM usuario u
    INNER JOIN roles r ON u.RolUsu = r.RolUsu
    WHERE u.NombreUsu = Usu
      AND u.PassUsu = Pass
      AND u.Activo = 1;
END$$

DELIMITER ;
