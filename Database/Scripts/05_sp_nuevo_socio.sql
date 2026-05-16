USE db_club_deportivo;

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

END $$

DELIMITER ;

CALL sp_nuevo_socio('38744622', 'Juan', 'Pérez', '5678', 'Calle 123');

SELECT * FROM socios;
