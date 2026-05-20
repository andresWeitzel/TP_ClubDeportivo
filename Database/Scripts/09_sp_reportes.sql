USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - REPORTES
-- ============================================

DROP PROCEDURE IF EXISTS sp_cuotas_por_vencer;

DELIMITER $$

CREATE PROCEDURE sp_cuotas_por_vencer(
    IN p_dias INT
)
BEGIN
    SELECT 
        s.id_socio,
        s.dni,
        s.nombre,
        s.apellido,
        s.estado_cuota,
        c.id_cuota,
        c.monto,
        c.fecha_vencimiento,
        DATEDIFF(c.fecha_vencimiento, CURDATE()) as dias_para_vencer
    FROM socios s
    INNER JOIN cuotas c ON s.id_socio = c.socio_id
    WHERE c.fecha_vencimiento BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL p_dias DAY)
    AND c.estado != 'PAGADA'
    ORDER BY c.fecha_vencimiento ASC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_cuotas_vencidas;

DELIMITER $$

CREATE PROCEDURE sp_cuotas_vencidas()
BEGIN
    SELECT 
        s.id_socio,
        s.dni,
        s.nombre,
        s.apellido,
        s.estado_cuota,
        c.id_cuota,
        c.monto,
        c.fecha_vencimiento,
        DATEDIFF(CURDATE(), c.fecha_vencimiento) as dias_vencidos
    FROM socios s
    INNER JOIN cuotas c ON s.id_socio = c.socio_id
    WHERE c.fecha_vencimiento < CURDATE()
    AND c.estado != 'PAGADA'
    ORDER BY c.fecha_vencimiento ASC;
END$$

DELIMITER ;
