USE db_club_deportivo;

-- =============================================================================
-- DATOS INICIALES — Club Deportivo
-- Ejecutar después de 01_DDL.sql y stored procedures.
-- Las fechas relativas usan CURDATE() para que reportes y mora sigan vigentes.
--
-- Matriz de cobertura:
--   Roles (6) | Usuarios por rol + inactivo
--   Socios: AL_DIA, MORA, cuota PAGADA/VENCIDA/por vencer/vigente
--   Actividades: cupo diario para visitantes (CU-02 / E1)
--   Visitantes: con y sin pago registrado
--   Carnets: vigentes y vencido
--   Pagos: SOCIO y VISITANTE, varios medios
--   Fichas médicas: todas los socios (completa, restricciones, mínima)
--   Profesores + horarios (todos los días útiles + fin de semana)
--   Asistencias: presente, ausente, con/sin firma
--   Rutinas: varios socios y profesores
--   Turnos nutrición: DISPONIBLE, CONFIRMADO, CANCELADO
--   Liquidaciones: PAGADO y PENDIENTE, varios meses
-- =============================================================================

-- ============================================
-- ROLES (todos los actores del sistema)
-- ============================================
INSERT INTO roles (RolUsu, NomRol) VALUES
(120, 'Administrador'),
(121, 'Empleado'),
(122, 'Profesor'),
(123, 'Nutricionista'),
(124, 'Socio'),
(125, 'Visitante');

-- ============================================
-- USUARIOS (login por rol + usuario inactivo)
-- ============================================
INSERT INTO usuario (NombreUsu, PassUsu, RolUsu, Activo) VALUES
('admin',       '1234',   120, TRUE),
('Mari2023',    '123456', 120, TRUE),
('empleado1',   'emp123', 121, TRUE),
('recep1',      'recep1', 121, TRUE),
('juan_prof',   'prof123',122, TRUE),
('patricia_prof','prof456',122, TRUE),
('maria_nutri', 'nutri123',123, TRUE),
('daniela_nutri','nutri456',123, TRUE),
('socio_demo',  'socio1', 124, TRUE),
('visit_demo',  'vis1',    125, TRUE),
('testeador',   'test123',121, TRUE),
('nutri_test',  'nutri789',123, TRUE),
('inactivo',    '1234',   120, FALSE);

-- ============================================
-- SOCIOS (12 perfiles de prueba)
-- ============================================
INSERT INTO socios (dni, nombre, apellido, telefono, direccion, email, estado_cuota, fecha_alta) VALUES
('12345678', 'Carlos',   'García',     '1123456789', 'Av. Principal 100',      'carlos.garcia@email.com',    'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 400 DAY)),
('87654321', 'María',    'López',      '1198765432', 'Calle 45 200',           'maria.lopez@email.com',      'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 350 DAY)),
('11111111', 'Juan',     'Martínez',   '1111111111', 'Avenida Central 500',    'juan.martinez@email.com',    'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 300 DAY)),
('22222222', 'Ana',      'Rodríguez',  '2222222222', 'Calle B 300',            'ana.rodriguez@email.com',    'MORA',   DATE_SUB(CURDATE(), INTERVAL 280 DAY)),
('33333333', 'Pedro',    'Sánchez',    '3333333333', 'Pasaje 150',             'pedro.sanchez@email.com',    'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 250 DAY)),
('44444444', 'Laura',    'Fernández',  '4444444444', 'Ruta 7 km 10',           'laura.fernandez@email.com',  'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 200 DAY)),
('55555555', 'Roberto',  'González',   '5555555555', 'Calle 88 100',           'roberto.gonzalez@email.com', 'MORA',   DATE_SUB(CURDATE(), INTERVAL 180 DAY)),
('66666666', 'Sofía',    'Torres',     '6666666666', 'Av. Libertad 250',       'sofia.torres@email.com',     'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 150 DAY)),
('77777777', 'Miguel',   'Ramírez',    '7777777777', 'Callejón del Sur',       'miguel.ramirez@email.com',   'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 60 DAY)),
('88888888', 'Claudia',  'Moreno',     '8888888888', 'Plaza Mayor 10',         'claudia.moreno@email.com',   'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 45 DAY)),
('12121212', 'Diego',    'Herrera',    '1212121212', 'Barrio Norte 55',        'diego.herrera@email.com',    'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 15 DAY)),
('13131313', 'Valentina','Acosta',     '13131313',   'Calle Sur 12',           'valentina.acosta@email.com', 'MORA',   DATE_SUB(CURDATE(), INTERVAL 90 DAY)),
('14141414', 'Bruno',    'Cruz',       '1414141414', 'Paseo Colón 720',        'bruno.cruz@email.com',       'AL_DIA', DATE_SUB(CURDATE(), INTERVAL 30 DAY)),
('15151515', 'Carla',    'Duarte',     '1515151515', 'Av. 9 de Julio 1240',    'carla.duarte@email.com',     'MORA',   DATE_SUB(CURDATE(), INTERVAL 75 DAY));

-- ============================================
-- ACTIVIDADES (cupo diario para visitantes — CU-02 / E1)
-- Crossfit cupo 1 + visitante hoy = prueba de «sin cupo»
-- ============================================
INSERT INTO actividades (nombre, descripcion, cupo_maximo, precio_visitante, activa) VALUES
('Musculación', 'Sala de musculación y máquinas',           25, 50.00, TRUE),
('Pilates',     'Pilates mat y reformer',                   15, 40.00, TRUE),
('Natación',    'Pileta y aquagym',                         12, 45.00, TRUE),
('Yoga',        'Yoga y stretching',                        20, 35.00, TRUE),
('Spinning',    'Ciclismo indoor',                          18, 55.00, TRUE),
('Crossfit',    'Entrenamiento funcional alta intensidad',   1, 60.00, TRUE);

-- ============================================
-- VISITANTES (con y sin pago en tabla pagos)
-- ============================================
INSERT INTO visitantes (dni, nombre, apellido, telefono, actividad_id, fecha_ingreso, pago_diario_monto) VALUES
('99999999', 'Andrés',  'Pérez',  '9999999999', 1, DATE_SUB(NOW(), INTERVAL 5 DAY),  50.00),
('10101010', 'Beatriz', 'Díaz',   '1010101010', 2, DATE_SUB(NOW(), INTERVAL 4 DAY),  40.00),
('20202020', 'Diego',   'Ruiz',   '2020202020', 3, DATE_SUB(NOW(), INTERVAL 3 DAY),  45.00),
('30303030', 'Elena',   'Vega',   '3030303030', 4, DATE_SUB(NOW(), INTERVAL 2 DAY),  35.00),
('40404040', 'Felipe',  'Castro', '4040404040', 5, DATE_SUB(NOW(), INTERVAL 1 DAY),  55.00),
('50505050', 'Gabriela','Núñez',  '5050505050', 6, TIMESTAMP(CURDATE(), '10:00:00'), 60.00),
('60606060', 'Héctor',  'Molina', '6060606060', 1, CURDATE(),                        50.00),
('70707070', 'Ivana',   'López',  '7070707070', 2, DATE_SUB(NOW(), INTERVAL 6 DAY),  40.00),
('80808080', 'Mauro',   'García', '8080808080', 3, DATE_SUB(NOW(), INTERVAL 7 DAY),  45.00);

-- ============================================
-- PROFESORES (4 especialidades + sueldos variados)
-- ============================================
INSERT INTO profesores (dni, nombre, apellido, telefono, email, especialidad, sueldo_base) VALUES
('50505050', 'Juan',     'Entrenador',  '5050505050', 'juan.trainer@email.com',       'Musculación', 5000.00),
('60606060', 'Patricia', 'Instructora', '6060606060', 'patricia.instructor@email.com','Pilates',     4500.00),
('70707070', 'Marcos',   'Coach',       '7070707070', 'marcos.coach@email.com',       'Spinning',    4800.00),
('80808080', 'Verónica', 'Entrenadora', '8080808080', 'veronica.trainer@email.com',   'Yoga',        4200.00),
('90909091', 'Lucas',    'Benítez',     '9090909191', 'lucas.benitez@email.com',      'Natación',    4600.00),
('91919192', 'Federico', 'Montes',      '9191919192', 'federico.montes@email.com',    'Crossfit',    5200.00);

-- ============================================
-- CARNETS (1 por socio; socio 8 con carnet vencido)
-- ============================================
INSERT INTO carnets (socio_id, numero, fecha_emision, fecha_vencimiento, foto) VALUES
(1,  'CARNET-0001', DATE_SUB(CURDATE(), INTERVAL 365 DAY), DATE_ADD(CURDATE(), INTERVAL 30 DAY),  NULL),
(2,  'CARNET-0002', DATE_SUB(CURDATE(), INTERVAL 300 DAY), DATE_ADD(CURDATE(), INTERVAL 60 DAY),  NULL),
(3,  'CARNET-0003', DATE_SUB(CURDATE(), INTERVAL 280 DAY), DATE_ADD(CURDATE(), INTERVAL 45 DAY),  NULL),
(4,  'CARNET-0004', DATE_SUB(CURDATE(), INTERVAL 260 DAY), DATE_ADD(CURDATE(), INTERVAL 20 DAY),  NULL),
(5,  'CARNET-0005', DATE_SUB(CURDATE(), INTERVAL 240 DAY), DATE_ADD(CURDATE(), INTERVAL 90 DAY),  NULL),
(6,  'CARNET-0006', DATE_SUB(CURDATE(), INTERVAL 200 DAY), DATE_ADD(CURDATE(), INTERVAL 120 DAY), NULL),
(7,  'CARNET-0007', DATE_SUB(CURDATE(), INTERVAL 180 DAY), DATE_ADD(CURDATE(), INTERVAL 15 DAY),  NULL),
(8,  'CARNET-0008', DATE_SUB(CURDATE(), INTERVAL 400 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY),  NULL),
(9,  'CARNET-0009', DATE_SUB(CURDATE(), INTERVAL 60 DAY),  DATE_ADD(CURDATE(), INTERVAL 305 DAY), NULL),
(10, 'CARNET-0010', DATE_SUB(CURDATE(), INTERVAL 45 DAY),  DATE_ADD(CURDATE(), INTERVAL 320 DAY),NULL),
(11, 'CARNET-0011', DATE_SUB(CURDATE(), INTERVAL 15 DAY),  DATE_ADD(CURDATE(), INTERVAL 350 DAY),NULL),
(12, 'CARNET-0012', DATE_SUB(CURDATE(), INTERVAL 90 DAY),  DATE_ADD(CURDATE(), INTERVAL 275 DAY),NULL);

-- ============================================
-- CUOTAS (estados: PAGADA, AL_DIA, VENCIDA; en_mora TRUE/FALSE)
-- Fechas relativas a CURDATE() para reportes RF-15 / RF-16
-- ============================================

-- Socio 1 — Carlos: historial pagado + cuota vigente
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(1, 150.00, DATE_SUB(CURDATE(), INTERVAL 75 DAY), DATE_SUB(CURDATE(), INTERVAL 45 DAY), 'PAGADA', FALSE),
(1, 150.00, DATE_SUB(CURDATE(), INTERVAL 44 DAY), DATE_SUB(CURDATE(), INTERVAL 14 DAY), 'PAGADA', FALSE),
(1, 150.00, DATE_SUB(CURDATE(), INTERVAL 13 DAY), DATE_ADD(CURDATE(), INTERVAL 17 DAY),  'AL_DIA', FALSE);

-- Socio 2 — María: cuota por vencer en 7 días (RF-15)
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(2, 150.00, DATE_SUB(CURDATE(), INTERVAL 50 DAY), DATE_SUB(CURDATE(), INTERVAL 20 DAY), 'PAGADA', FALSE),
(2, 150.00, DATE_SUB(CURDATE(), INTERVAL 19 DAY), DATE_ADD(CURDATE(), INTERVAL 7 DAY),   'AL_DIA', FALSE);

-- Socio 3 — Juan: cuota vigente aún no pagada
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(3, 150.00, DATE_SUB(CURDATE(), INTERVAL 40 DAY), DATE_SUB(CURDATE(), INTERVAL 10 DAY), 'PAGADA', FALSE),
(3, 150.00, DATE_SUB(CURDATE(), INTERVAL 9 DAY),  DATE_ADD(CURDATE(), INTERVAL 21 DAY),  'AL_DIA', FALSE);

-- Socio 4 — Ana: mora con cuotas vencidas (RF-16 / RF-05)
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(4, 150.00, DATE_SUB(CURDATE(), INTERVAL 90 DAY), DATE_SUB(CURDATE(), INTERVAL 60 DAY), 'VENCIDA', TRUE),
(4, 150.00, DATE_SUB(CURDATE(), INTERVAL 59 DAY), DATE_SUB(CURDATE(), INTERVAL 29 DAY), 'VENCIDA', TRUE),
(4, 150.00, DATE_SUB(CURDATE(), INTERVAL 28 DAY), DATE_SUB(CURDATE(), INTERVAL 5 DAY),  'VENCIDA', TRUE);

-- Socio 5 — Pedro: al día, última pagada
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(5, 150.00, DATE_SUB(CURDATE(), INTERVAL 35 DAY), DATE_SUB(CURDATE(), INTERVAL 5 DAY),  'PAGADA', FALSE),
(5, 150.00, DATE_SUB(CURDATE(), INTERVAL 4 DAY),  DATE_ADD(CURDATE(), INTERVAL 26 DAY),  'AL_DIA', FALSE);

-- Socio 6 — Laura
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(6, 150.00, DATE_SUB(CURDATE(), INTERVAL 32 DAY), DATE_SUB(CURDATE(), INTERVAL 2 DAY),  'PAGADA', FALSE),
(6, 150.00, DATE_SUB(CURDATE(), INTERVAL 1 DAY),  DATE_ADD(CURDATE(), INTERVAL 29 DAY),  'AL_DIA', FALSE);

-- Socio 7 — Roberto: solo vencidas, sin pagos
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(7, 150.00, DATE_SUB(CURDATE(), INTERVAL 120 DAY), DATE_SUB(CURDATE(), INTERVAL 90 DAY), 'VENCIDA', TRUE),
(7, 150.00, DATE_SUB(CURDATE(), INTERVAL 89 DAY),  DATE_SUB(CURDATE(), INTERVAL 59 DAY), 'VENCIDA', TRUE),
(7, 150.00, DATE_SUB(CURDATE(), INTERVAL 58 DAY),  DATE_SUB(CURDATE(), INTERVAL 20 DAY), 'VENCIDA', TRUE);

-- Socio 8 — Sofía: cuota por vencer en 3 días
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(8, 150.00, DATE_SUB(CURDATE(), INTERVAL 28 DAY), DATE_SUB(CURDATE(), INTERVAL 3 DAY),  'PAGADA', FALSE),
(8, 150.00, DATE_SUB(CURDATE(), INTERVAL 2 DAY),  DATE_ADD(CURDATE(), INTERVAL 3 DAY),   'AL_DIA', FALSE);

-- Socio 9 — Miguel: socio reciente, primera cuota
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(9, 150.00, DATE_SUB(CURDATE(), INTERVAL 10 DAY), DATE_ADD(CURDATE(), INTERVAL 20 DAY),  'AL_DIA', FALSE);

-- Socio 10 — Claudia
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(10, 150.00, DATE_SUB(CURDATE(), INTERVAL 35 DAY), DATE_SUB(CURDATE(), INTERVAL 5 DAY), 'PAGADA', FALSE),
(10, 150.00, DATE_SUB(CURDATE(), INTERVAL 4 DAY),  DATE_ADD(CURDATE(), INTERVAL 26 DAY), 'AL_DIA', FALSE);

-- Socio 11 — Diego: alta reciente
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(11, 150.00, DATE_SUB(CURDATE(), INTERVAL 5 DAY),  DATE_ADD(CURDATE(), INTERVAL 25 DAY), 'AL_DIA', FALSE);

-- Socio 12 — Valentina: una vencida reciente
INSERT INTO cuotas (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora) VALUES
(12, 150.00, DATE_SUB(CURDATE(), INTERVAL 45 DAY), DATE_SUB(CURDATE(), INTERVAL 15 DAY), 'VENCIDA', TRUE),
(12, 150.00, DATE_SUB(CURDATE(), INTERVAL 14 DAY), DATE_ADD(CURDATE(), INTERVAL 16 DAY),  'AL_DIA', TRUE);

-- ============================================
-- PAGOS SOCIOS (solo cuotas PAGADA; varios medios)
-- ============================================
INSERT INTO pagos (tipo, socio_id, cuota_id, monto, fecha_pago, medio_pago, concepto)
SELECT 'SOCIO', c.socio_id, c.id_cuota, c.monto,
       DATE_SUB(c.fecha_vencimiento, INTERVAL 3 DAY),
       CASE (c.socio_id % 4)
           WHEN 0 THEN 'Efectivo'
           WHEN 1 THEN 'Tarjeta Débito'
           WHEN 2 THEN 'Transferencia'
           ELSE 'Tarjeta Crédito'
       END,
       CONCAT('Cuota mensual — venc. ', DATE_FORMAT(c.fecha_vencimiento, '%d/%m/%Y'))
FROM cuotas c
WHERE c.estado = 'PAGADA';

-- ============================================
-- PAGOS VISITANTES (visitantes 1-6; 7 sin pago aún)
-- ============================================
INSERT INTO pagos (tipo, visitante_id, monto, fecha_pago, medio_pago, concepto) VALUES
('VISITANTE', 1, 50.00, DATE_SUB(NOW(), INTERVAL 5 DAY), 'Efectivo',         'Entrada diaria — Musculación'),
('VISITANTE', 1, 50.00, DATE_SUB(NOW(), INTERVAL 2 DAY), 'Efectivo',         'Entrada diaria — Musculación'),
('VISITANTE', 2, 40.00, DATE_SUB(NOW(), INTERVAL 4 DAY), 'Tarjeta Débito',   'Entrada diaria — Pilates'),
('VISITANTE', 3, 45.00, DATE_SUB(NOW(), INTERVAL 3 DAY), 'Efectivo',         'Entrada diaria — Natación'),
('VISITANTE', 4, 35.00, DATE_SUB(NOW(), INTERVAL 2 DAY), 'Tarjeta Débito',   'Entrada diaria — Yoga'),
('VISITANTE', 5, 55.00, DATE_SUB(NOW(), INTERVAL 1 DAY), 'Transferencia',    'Entrada diaria — Spinning'),
('VISITANTE', 6, 60.00, NOW(),                            'Tarjeta Crédito',  'Entrada diaria — Crossfit');

-- ============================================
-- FICHAS MÉDICAS (1 por socio — todas cubiertas)
-- ============================================
INSERT INTO fichas_medicas (socio_id, peso, altura, alergias, medicacion, observaciones, carga_permitida) VALUES
(1,  75.50, 1.75, 'Penicilina',           'Ninguna',           'Buena salud general',              'Sin restricciones'),
(2,  62.00, 1.62, 'Ninguna',              'Vitaminas B12',     'Histórico de lumbalgia',           'Evitar >50kg en espalda'),
(3,  88.00, 1.82, 'Ninguna',              'Ninguna',           'Deportista activo',                'Sin restricciones'),
(4,  58.00, 1.60, 'Ibuprofeno',           'Levotiroxina',      'Hipotiroidismo controlado',        'Actividad moderada'),
(5,  72.00, 1.78, 'Ninguna',              'Ninguna',           'Excelente estado físico',          'Sin restricciones'),
(6,  65.00, 1.68, 'Polen',                'Antihistamínicos',  'Alergia estacional',               'Sin restricciones'),
(7,  91.00, 1.80, 'Ninguna',              'Ninguna',           'Sobrepeso; en mora de cuota',      'Cardio y máquinas guiadas'),
(8,  95.00, 1.85, 'Ninguna',              'Ninguna',           'Sobrepeso moderado',               'Entrenamiento controlado'),
(9,  70.00, 1.75, 'Ninguna',              'Ninguna',           'Socio nuevo — evaluación pendiente','Sin definir'),
(10, 68.00, 1.70, 'Mariscos',             'Ninguna',           'Buen estado físico',               'Sin restricciones'),
(11, 0.00,  0.00, NULL,                   NULL,                'Ficha recién creada — completar',  'Pendiente de evaluación'),
(12, 55.00, 1.58, 'Aspirina',             'Anticonceptivos',   'Control nutricional recomendado',  'Yoga y pilates liviano');

-- ============================================
-- NUTRICIONISTAS
-- ============================================
INSERT INTO nutricionistas (dni, nombre, apellido, telefono, email, matricula) VALUES
('90909090', 'Daniela', 'Salas',  '9090909090', 'daniela.salas@email.com',  'MAT-NUTRI-001'),
('91919191', 'Rodrigo', 'Vargas', '9191919191', 'rodrigo.vargas@email.com', 'MAT-NUTRI-002'),
('92929292', 'Camila',  'Ortiz',  '9292929292', 'camila.ortiz@email.com',   'MAT-NUTRI-003');

-- ============================================
-- HORARIOS ACTIVIDAD (todos los días + 5 profesores)
-- ============================================
INSERT INTO horarios_actividad (profesor_id, actividad_id, dia_semana, hora_inicio, hora_fin) VALUES
(1, 1, 'Lunes',     '09:00:00', '10:00:00'),
(1, 1, 'Miércoles', '16:00:00', '17:00:00'),
(1, 1, 'Viernes',   '18:00:00', '19:00:00'),
(1, 1, 'Sábado',    '10:00:00', '11:30:00'),
(2, 2, 'Martes',    '10:30:00', '11:30:00'),
(2, 2, 'Jueves',    '19:00:00', '20:00:00'),
(2, 2, 'Sábado',    '09:00:00', '10:00:00'),
(3, 5, 'Lunes',     '19:30:00', '20:30:00'),
(3, 5, 'Miércoles', '20:00:00', '21:00:00'),
(3, 5, 'Viernes',   '07:30:00', '08:30:00'),
(4, 4, 'Martes',    '08:00:00', '09:00:00'),
(4, 4, 'Jueves',    '18:30:00', '19:30:00'),
(4, 4, 'Domingo',   '11:00:00', '12:00:00'),
(5, 3, 'Lunes',     '15:00:00', '16:00:00'),
(5, 3, 'Miércoles', '15:00:00', '16:00:00'),
(5, 3, 'Viernes',   '17:00:00', '18:00:00');

-- ============================================
-- RUTINAS (socios activos; distintos profesores)
-- ============================================
INSERT INTO rutinas (socio_id, profesor_id, descripcion, fecha_creacion, observaciones) VALUES
(1,  1, 'Hipertrofia: Press banca, Sentadillas, Peso muerto, Remo con barra', CURDATE(), '4 series × 8-10 reps'),
(2,  2, 'Pilates: Tabla, Puente glúteo, Círculos de piernas, Roll up',         CURDATE(), 'Core y movilidad'),
(3,  1, 'Fuerza: Sentadilla profunda, Peso muerto rumano, Press militar',      CURDATE(), 'Progresión +2.5% semanal'),
(5,  1, 'Principiante: Máquinas guiadas + cardio 20 min',                     CURDATE(), '3 series × 10 reps'),
(6,  2, 'Pilates avanzado: Flexibilidad y tonificación',                      CURDATE(), '2-3 series'),
(8,  4, 'Yoga + movilidad para sobrepeso',                                    CURDATE(), 'Sin impacto'),
(9,  1, 'Adaptación inicial: circuito full body ligero',                      CURDATE(), 'Primera rutina'),
(10, 3, 'Spinning + trabajo de base',                                         CURDATE(), '2 sesiones/semana'),
(11, 5, 'Natación técnica: crol y espalda',                                   CURDATE(), 'Evaluar en pileta'),
(12, 4, 'Movilidad y yoga: core y estiramientos',                               CURDATE(), 'Sesión para flexibilidad'),
(14, 2, 'Pilates + fuerza profunda',                                           CURDATE(), 'Hacer énfasis en postura');

-- ============================================
-- ASISTENCIAS PROFESORES (presente/ausente; con/sin firma)
-- ============================================
INSERT INTO asistencias (profesor_id, fecha, presente, firma) VALUES
(1, DATE_SUB(CURDATE(), INTERVAL 7 DAY), TRUE,  'Firma Juan Entrenador'),
(1, DATE_SUB(CURDATE(), INTERVAL 5 DAY), TRUE,  'Firma Juan Entrenador'),
(1, DATE_SUB(CURDATE(), INTERVAL 3 DAY), TRUE,  'Firma Juan Entrenador'),
(1, DATE_SUB(CURDATE(), INTERVAL 1 DAY), TRUE,  'Firma Juan Entrenador'),
(1, CURDATE(),                           TRUE,  'Firma Juan Entrenador'),
(2, DATE_SUB(CURDATE(), INTERVAL 6 DAY), TRUE,  'Firma Patricia Instructora'),
(2, DATE_SUB(CURDATE(), INTERVAL 4 DAY), TRUE,  'Firma Patricia Instructora'),
(2, DATE_SUB(CURDATE(), INTERVAL 2 DAY), FALSE, NULL),
(2, CURDATE(),                           TRUE,  'Firma Patricia Instructora'),
(3, DATE_SUB(CURDATE(), INTERVAL 5 DAY), FALSE, NULL),
(3, DATE_SUB(CURDATE(), INTERVAL 3 DAY), TRUE,  'Firma Marcos Coach'),
(3, DATE_SUB(CURDATE(), INTERVAL 1 DAY), TRUE,  'Firma Marcos Coach'),
(4, DATE_SUB(CURDATE(), INTERVAL 4 DAY), TRUE,  'Firma Verónica Entrenadora'),
(4, DATE_SUB(CURDATE(), INTERVAL 2 DAY), TRUE,  'Firma Verónica Entrenadora'),
(4, CURDATE(),                           TRUE,  'Firma Verónica Entrenadora'),
(5, DATE_SUB(CURDATE(), INTERVAL 3 DAY), TRUE,  'Firma Lucas Benítez'),
(5, CURDATE(),                           FALSE, NULL);

-- ============================================
-- TURNOS NUTRICIÓN (DISPONIBLE, CONFIRMADO, CANCELADO)
-- ============================================
INSERT INTO turnos_nutricion (socio_id, nutricionista_id, fecha, hora, estado) VALUES
(1,  1, DATE_SUB(CURDATE(), INTERVAL 10 DAY), '10:00:00', 'CONFIRMADO'),
(2,  2, DATE_ADD(CURDATE(), INTERVAL 5 DAY),  '14:30:00', 'DISPONIBLE'),
(3,  1, DATE_ADD(CURDATE(), INTERVAL 8 DAY),  '11:00:00', 'CONFIRMADO'),
(5,  2, DATE_ADD(CURDATE(), INTERVAL 12 DAY), '15:00:00', 'CONFIRMADO'),
(6,  1, DATE_SUB(CURDATE(), INTERVAL 3 DAY),  '09:30:00', 'CANCELADO'),
(8,  3, DATE_ADD(CURDATE(), INTERVAL 2 DAY),  '16:00:00', 'DISPONIBLE'),
(9,  1, DATE_ADD(CURDATE(), INTERVAL 15 DAY), '10:30:00', 'DISPONIBLE'),
(10, 2, DATE_SUB(CURDATE(), INTERVAL 7 DAY),  '11:30:00', 'CONFIRMADO'),
(4,  1, DATE_ADD(CURDATE(), INTERVAL 20 DAY), '08:30:00', 'DISPONIBLE'),
(12, 3, DATE_ADD(CURDATE(), INTERVAL 6 DAY),  '17:00:00', 'CONFIRMADO'),
(1,  2, DATE_ADD(CURDATE(), INTERVAL 3 DAY),  '09:00:00', 'DISPONIBLE'),
(3,  3, DATE_ADD(CURDATE(), INTERVAL 4 DAY),  '13:30:00', 'DISPONIBLE'),
(5,  1, DATE_ADD(CURDATE(), INTERVAL 7 DAY),  '10:15:00', 'DISPONIBLE'),
(7,  2, DATE_ADD(CURDATE(), INTERVAL 10 DAY), '12:00:00', 'DISPONIBLE');

-- ============================================
-- LIQUIDACIONES (PAGADO / PENDIENTE; varios meses)
-- ============================================
INSERT INTO liquidaciones (profesor_id, mes, anio, monto_bruto, descuentos, monto_neto, fecha_pago, estado) VALUES
(1, MONTH(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), 5000.00, 500.00, 4500.00, DATE_SUB(CURDATE(), INTERVAL 55 DAY), 'PAGADO'),
(2, MONTH(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), 4500.00, 450.00, 4050.00, DATE_SUB(CURDATE(), INTERVAL 55 DAY), 'PAGADO'),
(3, MONTH(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), 4800.00, 480.00, 4320.00, DATE_SUB(CURDATE(), INTERVAL 54 DAY), 'PAGADO'),
(4, MONTH(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), 4200.00, 420.00, 3780.00, DATE_SUB(CURDATE(), INTERVAL 54 DAY), 'PAGADO'),
(5, MONTH(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)), 4600.00, 460.00, 4140.00, DATE_SUB(CURDATE(), INTERVAL 53 DAY), 'PAGADO'),
(1, MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), 5000.00, 750.00, 4250.00, DATE_SUB(CURDATE(), INTERVAL 25 DAY), 'PAGADO'),
(2, MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), 4500.00, 450.00, 4050.00, DATE_SUB(CURDATE(), INTERVAL 25 DAY), 'PAGADO'),
(3, MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), 4800.00, 960.00, 3840.00, NULL, 'PENDIENTE'),
(4, MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), 4200.00, 420.00, 3780.00, NULL, 'PENDIENTE'),
(5, MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), 4600.00, 0.00,   4600.00, NULL, 'PENDIENTE'),
(1, MONTH(CURDATE()), YEAR(CURDATE()), 5000.00, 0.00, 5000.00, NULL, 'PENDIENTE'),
(2, MONTH(CURDATE()), YEAR(CURDATE()), 4500.00, 0.00, 4500.00, NULL, 'PENDIENTE'),
(3, MONTH(CURDATE()), YEAR(CURDATE()), 4800.00, 0.00, 4800.00, NULL, 'PENDIENTE'),
(4, MONTH(CURDATE()), YEAR(CURDATE()), 4200.00, 0.00, 4200.00, NULL, 'PENDIENTE'),
(5, MONTH(CURDATE()), YEAR(CURDATE()), 4600.00, 0.00, 4600.00, NULL, 'PENDIENTE');
