USE db_club_deportivo;

-- ============================================
-- STORED PROCEDURES - VISITANTES (CU-02)
-- ============================================

DROP PROCEDURE IF EXISTS sp_crear_visitante;

DELIMITER $$

CREATE PROCEDURE sp_crear_visitante(
    IN p_dni VARCHAR(20),
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_actividad_id INT,
    IN p_pago_diario_monto DECIMAL(10,2),
    OUT p_visitante_id INT
)
BEGIN
    DECLARE v_cupo INT DEFAULT 0;
    DECLARE v_ocupados INT DEFAULT 0;

    SET p_visitante_id = 0;

    SELECT cupo_maximo INTO v_cupo
    FROM actividades
    WHERE id_actividad = p_actividad_id AND activa = TRUE
    LIMIT 1;

    IF v_cupo > 0 THEN
        SELECT COUNT(*) INTO v_ocupados
        FROM visitantes
        WHERE actividad_id = p_actividad_id
          AND DATE(fecha_ingreso) = CURDATE();

        IF v_ocupados < v_cupo THEN
            INSERT INTO visitantes (dni, nombre, apellido, telefono, actividad_id, fecha_ingreso, pago_diario_monto)
            VALUES (p_dni, p_nombre, p_apellido, p_telefono, p_actividad_id, NOW(), p_pago_diario_monto);

            SET p_visitante_id = LAST_INSERT_ID();
        END IF;
    END IF;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_visitante_por_id;

DELIMITER $$

CREATE PROCEDURE sp_obtener_visitante_por_id(
    IN p_id_visitante INT
)
BEGIN
    SELECT
        v.id_visitante,
        v.dni,
        v.nombre,
        v.apellido,
        v.telefono,
        v.actividad_id,
        a.nombre AS actividad,
        v.fecha_ingreso,
        v.pago_diario_monto
    FROM visitantes v
    INNER JOIN actividades a ON v.actividad_id = a.id_actividad
    WHERE v.id_visitante = p_id_visitante
    LIMIT 1;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_visitantes;

DELIMITER $$

CREATE PROCEDURE sp_obtener_visitantes()
BEGIN
    SELECT
        v.id_visitante,
        v.dni,
        v.nombre,
        v.apellido,
        v.telefono,
        v.actividad_id,
        a.nombre AS actividad,
        v.fecha_ingreso,
        v.pago_diario_monto
    FROM visitantes v
    INNER JOIN actividades a ON v.actividad_id = a.id_actividad
    ORDER BY v.fecha_ingreso DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_obtener_visitantes_listado;

DELIMITER $$

CREATE PROCEDURE sp_obtener_visitantes_listado()
BEGIN
    SELECT
        v.id_visitante,
        v.dni,
        v.nombre,
        v.apellido,
        v.telefono,
        v.actividad_id,
        a.nombre AS actividad,
        v.fecha_ingreso,
        COALESCE(
            (
                SELECT p.monto
                FROM pagos p
                WHERE p.visitante_id = v.id_visitante
                ORDER BY p.fecha_pago DESC
                LIMIT 1
            ),
            v.pago_diario_monto
        ) AS monto,
        (
            SELECT p.medio_pago
            FROM pagos p
            WHERE p.visitante_id = v.id_visitante
            ORDER BY p.fecha_pago DESC
            LIMIT 1
        ) AS medio_pago,
        (
            SELECT COUNT(*) > 0
            FROM pagos p
            WHERE p.visitante_id = v.id_visitante
        ) AS tiene_pago
    FROM visitantes v
    INNER JOIN actividades a ON v.actividad_id = a.id_actividad
    ORDER BY v.fecha_ingreso DESC;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_actualizar_visitante;

DELIMITER $$

CREATE PROCEDURE sp_actualizar_visitante(
    IN p_id_visitante INT,
    IN p_dni VARCHAR(20),
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(50),
    IN p_actividad_id INT,
    IN p_pago_diario_monto DECIMAL(10,2),
    OUT p_actualizado BOOLEAN
)
BEGIN
    DECLARE v_cupo INT DEFAULT 0;
    DECLARE v_ocupados INT DEFAULT 0;

    SET p_actualizado = FALSE;

    SELECT cupo_maximo INTO v_cupo
    FROM actividades
    WHERE id_actividad = p_actividad_id AND activa = TRUE
    LIMIT 1;

    IF v_cupo > 0 THEN
        SELECT COUNT(*) INTO v_ocupados
        FROM visitantes
        WHERE actividad_id = p_actividad_id
          AND DATE(fecha_ingreso) = CURDATE()
          AND id_visitante <> p_id_visitante;

        IF v_ocupados < v_cupo THEN
            UPDATE visitantes
            SET
                dni = p_dni,
                nombre = p_nombre,
                apellido = p_apellido,
                telefono = p_telefono,
                actividad_id = p_actividad_id,
                pago_diario_monto = p_pago_diario_monto
            WHERE id_visitante = p_id_visitante;

            SET p_actualizado = (ROW_COUNT() >= 1);
        END IF;
    END IF;
END$$

DELIMITER ;

DROP PROCEDURE IF EXISTS sp_eliminar_visitante;

DELIMITER $$

CREATE PROCEDURE sp_eliminar_visitante(
    IN p_id_visitante INT,
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DECLARE v_cant_pagos INT DEFAULT 0;

    SELECT COUNT(*) INTO v_cant_pagos
    FROM pagos
    WHERE visitante_id = p_id_visitante;

    IF v_cant_pagos > 0 THEN
        DELETE FROM pagos WHERE visitante_id = p_id_visitante;
    END IF;

    DELETE FROM visitantes WHERE id_visitante = p_id_visitante;

    IF ROW_COUNT() >= 1 THEN
        IF v_cant_pagos > 0 THEN
            SET p_mensaje = CONCAT(
                'Visitante eliminado correctamente (',
                v_cant_pagos,
                ' pago(s) diario(s) asociado(s) también fueron eliminados).'
            );
        ELSE
            SET p_mensaje = 'Visitante eliminado correctamente.';
        END IF;
    ELSE
        SET p_mensaje = 'No se encontró el visitante indicado.';
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
        v.id_visitante,
        v.dni,
        v.nombre,
        v.apellido,
        v.telefono,
        v.actividad_id,
        a.nombre AS actividad,
        v.fecha_ingreso,
        v.pago_diario_monto
    FROM visitantes v
    INNER JOIN actividades a ON v.actividad_id = a.id_actividad
    WHERE v.nombre LIKE CONCAT('%', p_busqueda, '%')
        OR v.apellido LIKE CONCAT('%', p_busqueda, '%')
        OR v.dni LIKE CONCAT('%', p_busqueda, '%')
        OR a.nombre LIKE CONCAT('%', p_busqueda, '%')
    ORDER BY v.fecha_ingreso DESC;
END$$

DELIMITER ;
