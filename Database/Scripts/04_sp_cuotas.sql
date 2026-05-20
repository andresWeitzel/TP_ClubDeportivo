USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - CUOTAS
-- ============================================

DROP PROCEDURE IF EXISTS sp_crear_cuota;

DELIMITER $$

CREATE PROCEDURE sp_crear_cuota(
    IN p_socio_id INT,
    IN p_monto DECIMAL(10,2),
    IN p_fecha_vencimiento DATE,
    OUT p_cuota_id INT
)
BEGIN
    INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora)
    VALUES (p_socio_id, p_monto, CURDATE(), p_fecha_vencimiento, 'AL_DIA', FALSE);
    
    SET p_cuota_id = LAST_INSERT_ID();
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_cuotas_por_socio;

DELIMITER $$

CREATE PROCEDURE sp_obtener_cuotas_por_socio(
    IN p_socio_id INT
)
BEGIN
    SELECT 
        id_cuota,
        socio_id,
        monto,
        fecha_emision,
        fecha_vencimiento,
        estado,
        en_mora
    FROM cuotas
    WHERE socio_id = p_socio_id
    ORDER BY fecha_vencimiento DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_ultima_cuota_socio;

DELIMITER $$

CREATE PROCEDURE sp_obtener_ultima_cuota_socio(
    IN p_socio_id INT
)
BEGIN
    SELECT 
        id_cuota,
        socio_id,
        monto,
        fecha_emision,
        fecha_vencimiento,
        estado,
        en_mora
    FROM cuotas
    WHERE socio_id = p_socio_id
    ORDER BY fecha_vencimiento DESC
    LIMIT 1;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_estado_cuota_mora;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_estado_cuota_mora(
    IN p_id_cuota INT,
    IN p_estado VARCHAR(50),
    IN p_en_mora BOOLEAN
)
BEGIN
    UPDATE cuotas 
    SET estado = p_estado, en_mora = p_en_mora
    WHERE id_cuota = p_id_cuota;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_cuota;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_cuota(
    IN p_id_cuota INT,
    IN p_monto DECIMAL(10,2),
    IN p_fecha_vencimiento DATE
)
BEGIN
    UPDATE cuotas
    SET 
        monto = p_monto,
        fecha_vencimiento = p_fecha_vencimiento
    WHERE id_cuota = p_id_cuota;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_eliminar_cuota;

DELIMITER $$

CREATE PROCEDURE sp_eliminar_cuota(
    IN p_id_cuota INT,
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DECLARE v_tiene_pagos INT;
    
    -- Validar si tiene pagos registrados
    SELECT COUNT(*) INTO v_tiene_pagos 
    FROM pagos WHERE cuota_id = p_id_cuota;
    
    IF v_tiene_pagos > 0 THEN
        SET p_mensaje = 'No se puede eliminar. La cuota tiene pagos registrados.';
    ELSE
        DELETE FROM cuotas WHERE id_cuota = p_id_cuota;
        SET p_mensaje = 'Cuota eliminada correctamente.';
    END IF;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_todas_cuotas;

DELIMITER $$

CREATE PROCEDURE sp_obtener_todas_cuotas()
BEGIN
    SELECT 
        id_cuota,
        socio_id,
        monto,
        fecha_emision,
        fecha_vencimiento,
        estado,
        en_mora
    FROM cuotas
    ORDER BY fecha_vencimiento DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_controlar_vencimiento_cuotas;

DELIMITER $$

CREATE PROCEDURE sp_controlar_vencimiento_cuotas(
    OUT p_cuotas_en_mora INT,
    OUT p_socios_suspendidos INT
)
BEGIN
    -- CU-04 paso 2: cuotas vencidas impagas pasan a mora
    UPDATE cuotas
    SET estado = 'VENCIDA', en_mora = TRUE
    WHERE fecha_vencimiento < CURDATE()
      AND estado <> 'PAGADA';

    SELECT COUNT(*) INTO p_cuotas_en_mora
    FROM cuotas
    WHERE fecha_vencimiento < CURDATE()
      AND estado <> 'PAGADA';

    -- CU-04 paso 3: socio con deuda vencida queda suspendido (MORA en el sistema)
    UPDATE socios s
    SET estado_cuota = 'MORA'
    WHERE EXISTS (
        SELECT 1
        FROM cuotas c
        WHERE c.socio_id = s.id_socio
          AND c.fecha_vencimiento < CURDATE()
          AND c.estado <> 'PAGADA'
    );

    -- Socios sin deuda vencida vuelven a AL_DIA
    UPDATE socios s
    SET estado_cuota = 'AL_DIA'
    WHERE NOT EXISTS (
        SELECT 1
        FROM cuotas c
        WHERE c.socio_id = s.id_socio
          AND c.fecha_vencimiento < CURDATE()
          AND c.estado <> 'PAGADA'
    );

    SELECT COUNT(*) INTO p_socios_suspendidos
    FROM socios
    WHERE estado_cuota = 'MORA';
END$$

DELIMITER ;