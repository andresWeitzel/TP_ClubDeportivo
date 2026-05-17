USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - SOCIOS
-- ============================================

DROP PROCEDURE IF EXISTS sp_crear_socio;

DELIMITER $$

CREATE PROCEDURE sp_crear_socio(
    IN p_dni VARCHAR(20),
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_direccion VARCHAR(150),
    IN p_email VARCHAR(150),
    OUT p_socio_id INT
)
BEGIN
    INSERT INTO socios (dni, nombre, apellido, telefono, direccion, email, estado_cuota)
    VALUES (p_dni, p_nombre, p_apellido, p_telefono, p_direccion, p_email, 'AL_DIA');
    
    SET p_socio_id = LAST_INSERT_ID();
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_nuevo_socio;

DELIMITER $$

CREATE PROCEDURE sp_nuevo_socio(
    IN p_dni VARCHAR(20),
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_direccion VARCHAR(150)
)
BEGIN
    INSERT INTO socios (
        dni,
        nombre,
        apellido,
        telefono,
        direccion,
        estado_cuota
    )
    VALUES (
        p_dni,
        p_nombre,
        p_apellido,
        p_telefono,
        p_direccion,
        'AL_DIA'
    );
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_socios;

DELIMITER $$

CREATE PROCEDURE sp_obtener_socios()
BEGIN
    SELECT 
        id_socio,
        dni,
        nombre,
        apellido,
        telefono,
        direccion,
        email,
        estado_cuota,
        fecha_alta
    FROM socios
    ORDER BY nombre, apellido;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_socio_por_id;

DELIMITER $$

CREATE PROCEDURE sp_obtener_socio_por_id(
    IN p_id_socio INT
)
BEGIN
    SELECT 
        id_socio,
        dni,
        nombre,
        apellido,
        telefono,
        direccion,
        email,
        estado_cuota,
        fecha_alta
    FROM socios
    WHERE id_socio = p_id_socio;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_socio_por_dni;

DELIMITER $$

CREATE PROCEDURE sp_obtener_socio_por_dni(
    IN p_dni VARCHAR(20)
)
BEGIN
    SELECT 
        id_socio,
        dni,
        nombre,
        apellido,
        telefono,
        direccion,
        email,
        estado_cuota,
        fecha_alta
    FROM socios
    WHERE dni = p_dni;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_estado_cuota;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_estado_cuota(
    IN p_socio_id INT,
    IN p_estado VARCHAR(50)
)
BEGIN
    UPDATE socios 
    SET estado_cuota = p_estado
    WHERE id_socio = p_socio_id;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_socio;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_socio(
    IN p_socio_id INT,
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_direccion VARCHAR(150),
    IN p_email VARCHAR(150)
)
BEGIN
    UPDATE socios
    SET 
        nombre = p_nombre,
        apellido = p_apellido,
        telefono = p_telefono,
        direccion = p_direccion,
        email = p_email
    WHERE id_socio = p_socio_id;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_eliminar_socio;

DELIMITER $$

CREATE PROCEDURE sp_eliminar_socio(
    IN p_socio_id INT,
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DECLARE v_tiene_cuotas INT;
    DECLARE v_tiene_pagos INT;
    DECLARE v_tiene_carnets INT;
    
    -- Validar si tiene cuotas asociadas
    SELECT COUNT(*) INTO v_tiene_cuotas 
    FROM cuotas WHERE socio_id = p_socio_id;
    
    -- Validar si tiene pagos asociados
    SELECT COUNT(*) INTO v_tiene_pagos 
    FROM pagos WHERE socio_id = p_socio_id;
    
    -- Validar si tiene carnets asociados
    SELECT COUNT(*) INTO v_tiene_carnets 
    FROM carnets WHERE socio_id = p_socio_id;
    
    -- Si tiene registros asociados, no eliminar
    IF v_tiene_cuotas > 0 OR v_tiene_pagos > 0 OR v_tiene_carnets > 0 THEN
        SET p_mensaje = 'No se puede eliminar. El socio tiene registros asociados.';
    ELSE
        DELETE FROM socios WHERE id_socio = p_socio_id;
        SET p_mensaje = 'Socio eliminado correctamente.';
    END IF;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_buscar_socios;

DELIMITER $$

CREATE PROCEDURE sp_buscar_socios(
    IN p_busqueda VARCHAR(100)
)
BEGIN
    SELECT 
        id_socio,
        dni,
        nombre,
        apellido,
        telefono,
        direccion,
        email,
        estado_cuota,
        fecha_alta
    FROM socios
    WHERE nombre LIKE CONCAT('%', p_busqueda, '%')
        OR apellido LIKE CONCAT('%', p_busqueda, '%')
        OR dni LIKE CONCAT('%', p_busqueda, '%')
        OR email LIKE CONCAT('%', p_busqueda, '%')
    ORDER BY nombre, apellido;
END$$

DELIMITER ;