USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - PAGOS
-- ============================================

DROP PROCEDURE IF EXISTS sp_registrar_pago_socio;

DELIMITER $$

CREATE PROCEDURE sp_registrar_pago_socio(
    IN p_socio_id INT,
    IN p_cuota_id INT,
    IN p_monto DECIMAL(10,2),
    IN p_medio_pago VARCHAR(50),
    IN p_concepto VARCHAR(150),
    OUT p_pago_id INT
)
BEGIN
    INSERT INTO pagos (tipo, socio_id, cuota_id, monto, fecha_pago, medio_pago, concepto)
    VALUES ('SOCIO', p_socio_id, p_cuota_id, p_monto, NOW(), p_medio_pago, p_concepto);
    
    SET p_pago_id = LAST_INSERT_ID();
    
    UPDATE cuotas SET estado = 'PAGADA', en_mora = FALSE WHERE id_cuota = p_cuota_id;
    UPDATE socios SET estado_cuota = 'AL_DIA' WHERE id_socio = p_socio_id;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_pagos_socio;

DELIMITER $$

CREATE PROCEDURE sp_obtener_pagos_socio(
    IN p_socio_id INT
)
BEGIN
    SELECT 
        id_pago,
        tipo,
        socio_id,
        cuota_id,
        visitante_id,
        monto,
        fecha_pago,
        medio_pago,
        concepto
    FROM pagos
    WHERE socio_id = p_socio_id
    ORDER BY fecha_pago DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_pagos_visitante;

DELIMITER $$

CREATE PROCEDURE sp_obtener_pagos_visitante(
    IN p_visitante_id INT
)
BEGIN
    SELECT 
        id_pago,
        tipo,
        socio_id,
        cuota_id,
        visitante_id,
        monto,
        fecha_pago,
        medio_pago,
        concepto
    FROM pagos
    WHERE visitante_id = p_visitante_id
    ORDER BY fecha_pago DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_registrar_pago_visitante;

DELIMITER $$

CREATE PROCEDURE sp_registrar_pago_visitante(
    IN p_visitante_id INT,
    IN p_monto DECIMAL(10,2),
    IN p_medio_pago VARCHAR(50),
    IN p_concepto VARCHAR(150),
    OUT p_pago_id INT
)
BEGIN
    INSERT INTO pagos (tipo, visitante_id, monto, fecha_pago, medio_pago, concepto)
    VALUES ('VISITANTE', p_visitante_id, p_monto, NOW(), p_medio_pago, p_concepto);
    
    SET p_pago_id = LAST_INSERT_ID();
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_todos_pagos;

DELIMITER $$

CREATE PROCEDURE sp_obtener_todos_pagos()
BEGIN
    SELECT 
        id_pago,
        tipo,
        socio_id,
        cuota_id,
        visitante_id,
        monto,
        fecha_pago,
        medio_pago,
        concepto
    FROM pagos
    ORDER BY fecha_pago DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_pagos_por_fecha;

DELIMITER $$

CREATE PROCEDURE sp_obtener_pagos_por_fecha(
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE
)
BEGIN
    SELECT 
        id_pago,
        tipo,
        socio_id,
        cuota_id,
        visitante_id,
        monto,
        fecha_pago,
        medio_pago,
        concepto
    FROM pagos
    WHERE DATE(fecha_pago) BETWEEN p_fecha_inicio AND p_fecha_fin
    ORDER BY fecha_pago DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_total_pagos_periodo;

DELIMITER $$

CREATE PROCEDURE sp_obtener_total_pagos_periodo(
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE
)
BEGIN
    SELECT 
        COUNT(*) as cantidad_pagos,
        SUM(monto) as monto_total,
        AVG(monto) as monto_promedio,
        MIN(monto) as monto_minimo,
        MAX(monto) as monto_maximo
    FROM pagos
    WHERE DATE(fecha_pago) BETWEEN p_fecha_inicio AND p_fecha_fin;
END$$

DELIMITER ;