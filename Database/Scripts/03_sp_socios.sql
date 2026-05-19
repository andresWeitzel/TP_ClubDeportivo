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
    DECLARE v_existe INT;
    DECLARE v_pagos INT DEFAULT 0;
    DECLARE v_cuotas INT DEFAULT 0;
    DECLARE v_carnets INT DEFAULT 0;
    DECLARE v_fichas INT DEFAULT 0;
    DECLARE v_rutinas INT DEFAULT 0;
    DECLARE v_turnos INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_mensaje = 'Error al eliminar el socio y sus registros asociados.';
    END;

    -- Verificar que el socio exista
    SELECT COUNT(*) INTO v_existe FROM socios WHERE id_socio = p_socio_id;
    IF v_existe = 0 THEN
        SET p_mensaje = 'El socio no existe.';
    ELSE
        START TRANSACTION;

        -- 1) Pagos: tanto los del socio como los de sus cuotas
        SELECT COUNT(*) INTO v_pagos
        FROM pagos
        WHERE socio_id = p_socio_id
           OR cuota_id IN (SELECT id_cuota FROM cuotas WHERE socio_id = p_socio_id);

        DELETE FROM pagos
        WHERE socio_id = p_socio_id
           OR cuota_id IN (SELECT id_cuota FROM cuotas WHERE socio_id = p_socio_id);

        -- 2) Cuotas
        SELECT COUNT(*) INTO v_cuotas FROM cuotas WHERE socio_id = p_socio_id;
        DELETE FROM cuotas WHERE socio_id = p_socio_id;

        -- 3) Carnets
        SELECT COUNT(*) INTO v_carnets FROM carnets WHERE socio_id = p_socio_id;
        DELETE FROM carnets WHERE socio_id = p_socio_id;

        -- 4) Ficha médica
        SELECT COUNT(*) INTO v_fichas FROM fichas_medicas WHERE socio_id = p_socio_id;
        DELETE FROM fichas_medicas WHERE socio_id = p_socio_id;

        -- 5) Rutinas
        SELECT COUNT(*) INTO v_rutinas FROM rutinas WHERE socio_id = p_socio_id;
        DELETE FROM rutinas WHERE socio_id = p_socio_id;

        -- 6) Turnos de nutrición
        SELECT COUNT(*) INTO v_turnos FROM turnos_nutricion WHERE socio_id = p_socio_id;
        DELETE FROM turnos_nutricion WHERE socio_id = p_socio_id;

        -- 7) Socio
        DELETE FROM socios WHERE id_socio = p_socio_id;

        COMMIT;

        SET p_mensaje = CONCAT(
            'Socio eliminado correctamente. Asociados borrados: ',
            v_pagos, ' pago(s), ',
            v_cuotas, ' cuota(s), ',
            v_carnets, ' carnet(s), ',
            v_fichas, ' ficha(s) médica(s), ',
            v_rutinas, ' rutina(s), ',
            v_turnos, ' turno(s) de nutrición.'
        );
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