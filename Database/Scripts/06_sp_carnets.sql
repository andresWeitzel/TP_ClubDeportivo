USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - CARNETS
-- ============================================

DROP PROCEDURE IF EXISTS sp_emitir_carnet;

DELIMITER $$

CREATE PROCEDURE sp_emitir_carnet(
    IN p_socio_id INT,
    IN p_numero VARCHAR(50),
    IN p_fecha_emision DATE,
    IN p_fecha_vencimiento DATE,
    OUT p_carnet_id INT
)
BEGIN
    INSERT INTO carnets (socio_id, numero, fecha_emision, fecha_vencimiento)
    VALUES (p_socio_id, p_numero, p_fecha_emision, p_fecha_vencimiento);
    
    SET p_carnet_id = LAST_INSERT_ID();
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_carnet_por_socio;

DELIMITER $$

CREATE PROCEDURE sp_obtener_carnet_por_socio(
    IN p_socio_id INT
)
BEGIN
    SELECT 
        id_carnet,
        socio_id,
        numero,
        fecha_emision,
        fecha_vencimiento,
        foto
    FROM carnets
    WHERE socio_id = p_socio_id
    LIMIT 1;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_todos_carnets;

DELIMITER $$

CREATE PROCEDURE sp_obtener_todos_carnets()
BEGIN
    SELECT 
        id_carnet,
        socio_id,
        numero,
        fecha_emision,
        fecha_vencimiento,
        foto
    FROM carnets
    ORDER BY fecha_vencimiento DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_carnet;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_carnet(
    IN p_id_carnet INT,
    IN p_numero VARCHAR(50),
    IN p_fecha_vencimiento DATE
)
BEGIN
    UPDATE carnets
    SET 
        numero = p_numero,
        fecha_vencimiento = p_fecha_vencimiento
    WHERE id_carnet = p_id_carnet;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_renovar_carnet;

DELIMITER $$

CREATE PROCEDURE sp_renovar_carnet(
    IN p_socio_id INT,
    IN p_fecha_vencimiento DATE,
    OUT p_carnet_id INT
)
BEGIN
    -- Actualizar carnet existente
    UPDATE carnets
    SET fecha_vencimiento = p_fecha_vencimiento
    WHERE socio_id = p_socio_id;
    
    SELECT id_carnet INTO p_carnet_id
    FROM carnets
    WHERE socio_id = p_socio_id;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_eliminar_carnet;

DELIMITER $$

CREATE PROCEDURE sp_eliminar_carnet(
    IN p_id_carnet INT,
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DELETE FROM carnets WHERE id_carnet = p_id_carnet;
    SET p_mensaje = 'Carnet eliminado correctamente.';
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_carnets_proximos_vencer;

DELIMITER $$

CREATE PROCEDURE sp_obtener_carnets_proximos_vencer(
    IN p_dias INT
)
BEGIN
    SELECT 
        c.id_carnet,
        c.socio_id,
        c.numero,
        c.fecha_emision,
        c.fecha_vencimiento,
        s.nombre,
        s.apellido,
        DATEDIFF(c.fecha_vencimiento, CURDATE()) as dias_para_vencer
    FROM carnets c
    INNER JOIN socios s ON c.socio_id = s.id_socio
    WHERE c.fecha_vencimiento BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL p_dias DAY)
    ORDER BY c.fecha_vencimiento ASC;
END$$

DELIMITER ;