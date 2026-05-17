USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - FICHAS MEDICAS
-- ============================================

DROP PROCEDURE IF EXISTS sp_crear_ficha_medica;

DELIMITER $$

CREATE PROCEDURE sp_crear_ficha_medica(
    IN p_socio_id INT
)
BEGIN
    INSERT INTO fichas_medicas (socio_id) VALUES (p_socio_id);
END$$

DELIMITER ;
