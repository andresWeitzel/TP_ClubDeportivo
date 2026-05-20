USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - ACTIVIDADES (CU-02 cupo)
-- ============================================

DROP PROCEDURE IF EXISTS sp_obtener_actividades;

DELIMITER $$

CREATE PROCEDURE sp_obtener_actividades()
BEGIN
    SELECT
        a.id_actividad,
        a.nombre,
        a.descripcion,
        a.cupo_maximo,
        a.precio_visitante,
        a.activa,
        (
            SELECT COUNT(*)
            FROM visitantes v
            WHERE v.actividad_id = a.id_actividad
              AND DATE(v.fecha_ingreso) = CURDATE()
        ) AS ocupados_hoy
    FROM actividades a
    WHERE a.activa = TRUE
    ORDER BY a.nombre;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_actividad_por_id;

DELIMITER $$

CREATE PROCEDURE sp_obtener_actividad_por_id(
    IN p_id_actividad INT
)
BEGIN
    SELECT
        a.id_actividad,
        a.nombre,
        a.descripcion,
        a.cupo_maximo,
        a.precio_visitante,
        a.activa,
        (
            SELECT COUNT(*)
            FROM visitantes v
            WHERE v.actividad_id = a.id_actividad
              AND DATE(v.fecha_ingreso) = CURDATE()
        ) AS ocupados_hoy
    FROM actividades a
    WHERE a.id_actividad = p_id_actividad
    LIMIT 1;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_verificar_cupo_actividad;

DELIMITER $$

CREATE PROCEDURE sp_verificar_cupo_actividad(
    IN p_actividad_id INT,
    IN p_excluir_visitante_id INT,
    OUT p_hay_cupo BOOLEAN,
    OUT p_ocupados INT,
    OUT p_cupo_maximo INT,
    OUT p_nombre_actividad VARCHAR(100)
)
BEGIN
    SET p_hay_cupo = FALSE;
    SET p_ocupados = 0;
    SET p_cupo_maximo = 0;
    SET p_nombre_actividad = '';

    SELECT a.cupo_maximo, a.nombre
    INTO p_cupo_maximo, p_nombre_actividad
    FROM actividades a
    WHERE a.id_actividad = p_actividad_id AND a.activa = TRUE
    LIMIT 1;

    IF p_cupo_maximo > 0 THEN
        SELECT COUNT(*)
        INTO p_ocupados
        FROM visitantes v
        WHERE v.actividad_id = p_actividad_id
          AND DATE(v.fecha_ingreso) = CURDATE()
          AND (p_excluir_visitante_id IS NULL OR v.id_visitante <> p_excluir_visitante_id);

        SET p_hay_cupo = (p_ocupados < p_cupo_maximo);
    END IF;
END$$

DELIMITER ;
