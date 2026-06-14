-- =============================================
-- SISTEMA DE GESTIÓN DE TURNOS
-- Script de creación de base de datos
-- =============================================

-- Drop en orden inverso por las dependencias
DROP TABLE IF EXISTS cambios_guardia;
DROP TABLE IF EXISTS plantillas_bloques;
DROP TABLE IF EXISTS plantillas_turno;
DROP TABLE IF EXISTS bloques;
DROP TABLE IF EXISTS jornadas;
DROP TABLE IF EXISTS codigos_estado;
DROP TABLE IF EXISTS operadores;


-- =============================================
-- TABLA 1: operadores
-- =============================================
CREATE TABLE operadores (
    id          SERIAL PRIMARY KEY,
    nombre      VARCHAR(100) NOT NULL,
    legajo      VARCHAR(20)  UNIQUE NOT NULL,
    turno_base  VARCHAR(20),
    activo      BOOLEAN DEFAULT true
);


-- =============================================
-- TABLA 2: codigos_estado
-- =============================================
CREATE TABLE codigos_estado (
    codigo              VARCHAR(10) PRIMARY KEY,
    descripcion         VARCHAR(100) NOT NULL,
    cuenta_como_activo  BOOLEAN DEFAULT false
);


-- =============================================
-- TABLA 3: jornadas
-- =============================================
CREATE TABLE jornadas (
    id              SERIAL PRIMARY KEY,
    operador_id     INT NOT NULL,
    fecha_inicio    TIMESTAMP NOT NULL,
    fecha_fin       TIMESTAMP NOT NULL,
    turno           VARCHAR(20),

    FOREIGN KEY (operador_id) REFERENCES operadores(id),
    UNIQUE(operador_id, fecha_inicio)
);


-- =============================================
-- TABLA 4: bloques
-- =============================================
CREATE TABLE bloques (
    id                  SERIAL PRIMARY KEY,
    jornada_id          INT NOT NULL,
    numero_bloque       INT NOT NULL,
    hora_inicio         TIMESTAMP NOT NULL,
    codigo              VARCHAR(10),
    motivo_ausencia     VARCHAR(10),
    modificado_por      INT,
    modificado_en       TIMESTAMP DEFAULT NOW(),

    FOREIGN KEY (jornada_id)        REFERENCES jornadas(id),
    FOREIGN KEY (codigo)            REFERENCES codigos_estado(codigo),
    FOREIGN KEY (modificado_por)    REFERENCES operadores(id),

    UNIQUE(jornada_id, numero_bloque)
);


-- =============================================
-- TABLA 5: cambios_guardia
-- =============================================
CREATE TABLE cambios_guardia (
    id                  SERIAL PRIMARY KEY,
    operador_sale_id    INT NOT NULL,
    operador_entra_id   INT NOT NULL,
    fecha               DATE NOT NULL,
    bloque_inicio       INT NOT NULL,
    bloque_fin          INT NOT NULL,
    devolucion_fecha    DATE,
    estado              VARCHAR(20) DEFAULT 'pendiente',
    registrado_por      INT NOT NULL,
    registrado_en       TIMESTAMP DEFAULT NOW(),
    notas               TEXT,

    FOREIGN KEY (operador_sale_id)  REFERENCES operadores(id),
    FOREIGN KEY (operador_entra_id) REFERENCES operadores(id),
    FOREIGN KEY (registrado_por)    REFERENCES operadores(id)
);


-- =============================================
-- TABLA 6: plantillas_turno
-- =============================================
CREATE TABLE plantillas_turno (
    id          SERIAL PRIMARY KEY,
    nombre      VARCHAR(100) NOT NULL,
    turno       VARCHAR(20),
    hora_inicio TIME NOT NULL,
    hora_fin    TIME NOT NULL
);


-- =============================================
-- TABLA 7: plantillas_bloques
-- =============================================
CREATE TABLE plantillas_bloques (
    id              SERIAL PRIMARY KEY,
    plantilla_id    INT NOT NULL,
    numero_bloque   INT NOT NULL,
    codigo          VARCHAR(10),

    FOREIGN KEY (plantilla_id)  REFERENCES plantillas_turno(id),
    FOREIGN KEY (codigo)        REFERENCES codigos_estado(codigo),

    UNIQUE(plantilla_id, numero_bloque)
);


-- =============================================
-- DATOS INICIALES
-- =============================================
INSERT INTO codigos_estado (codigo, descripcion, cuenta_como_activo) VALUES
('O',    'Operador Diurno',          true),
('ON',   'Operador Nocturno',        true),
('CG',   'Cambio de Guardia',        true),
('H50',  'Hora Extra al 50%',        true),
('H100', 'Hora Extra al 100%',       true),
('R',    'Recupera Horas',           true),
('BR',   'Break / Descanso',         false),
('Cap',  'Capacitación',             false),
('E',    'Enfermo',                  false),
('A',    'Ausente',                  false),
('V',    'Vacaciones',               false),
('LE',   'Licencia Especial',        false),
('T',    'Tarde',                    false);