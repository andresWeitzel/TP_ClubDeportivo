USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - VISITANTES
-- ============================================

DROP PROCEDURE IF EXISTS sp_crear_visitante;

DELIMITER $$

CREATE PROCEDURE sp_crear_visitante(
    IN p_dni VARCHAR(20),
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_actividad VARCHAR(100),
    IN p_pago_diario_monto DECIMAL(10,2),
    OUT p_visitante_id INT
)
BEGIN
    INSERT INTO visitantes (dni, nombre, apellido, telefono, actividad, fecha_ingreso, pago_diario_monto)
    VALUES (p_dni, p_nombre, p_apellido, p_telefono, p_actividad, NOW(), p_pago_diario_monto);
    
    SET p_visitante_id = LAST_INSERT_ID();
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_visitante_por_id;

DELIMITER $$

CREATE PROCEDURE sp_obtener_visitante_por_id(
    IN p_id_visitante INT
)
BEGIN
    SELECT 
        id_visitante,
        dni,
        nombre,
        apellido,
        telefono,
        actividad,
        fecha_ingreso,
        pago_diario_monto
    FROM visitantes
    WHERE id_visitante = p_id_visitante
    LIMIT 1;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_visitantes;

DELIMITER $$

CREATE PROCEDURE sp_obtener_visitantes()
BEGIN
    SELECT 
        id_visitante,
        dni,
        nombre,
        apellido,
        telefono,
        actividad,
        fecha_ingreso,
        pago_diario_monto
    FROM visitantes
    ORDER BY fecha_ingreso DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_visitante;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_visitante(
    IN p_id_visitante INT,
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_actividad VARCHAR(100),
    IN p_pago_diario_monto DECIMAL(10,2)
)
BEGIN
    UPDATE visitantes
    SET 
        nombre = p_nombre,
        apellido = p_apellido,
        telefono = p_telefono,
        actividad = p_actividad,
        pago_diario_monto = p_pago_diario_monto
    WHERE id_visitante = p_id_visitante;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_eliminar_visitante;

DELIMITER $$

CREATE PROCEDURE sp_eliminar_visitante(
    IN p_id_visitante INT,
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DECLARE v_tiene_pagos INT;
    
    -- Validar si tiene pagos registrados
    SELECT COUNT(*) INTO v_tiene_pagos 
    FROM pagos WHERE visitante_id = p_id_visitante;
    
    IF v_tiene_pagos > 0 THEN
        SET p_mensaje = 'No se puede eliminar. El visitante tiene pagos registrados.';
    ELSE
        DELETE FROM visitantes WHERE id_visitante = p_id_visitante;
        SET p_mensaje = 'Visitante eliminado correctamente.';
    END IF;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_buscar_visitantes;

DELIMITER $$

CREATE PROCEDURE sp_buscar_visitantes(
    IN p_busqueda VARCHAR(100)
)
BEGIN
    SELECT 
        id_visitante,
        dni,
        nombre,
        apellido,
        telefono,
        actividad,
        fecha_ingreso,
        pago_diario_monto
    FROM visitantes
    WHERE nombre LIKE CONCAT('%', p_busqueda, '%')
        OR apellido LIKE CONCAT('%', p_busqueda, '%')
        OR dni LIKE CONCAT('%', p_busqueda, '%')
        OR actividad LIKE CONCAT('%', p_busqueda, '%')
    ORDER BY fecha_ingreso DESC;
END$$

DELIMITER ;
