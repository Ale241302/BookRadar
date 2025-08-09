/* =========================================================
   1) Esquema base
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'BookRadar')
    EXEC('CREATE SCHEMA BookRadar');
GO

/* =========================================================
   2) Tabla principal de historial (con dedupe 1 min)
   ========================================================= */
IF OBJECT_ID('dbo.HistorialBusquedas') IS NULL
BEGIN
  CREATE TABLE dbo.HistorialBusquedas (
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
    Autor               NVARCHAR(200)  NOT NULL,
    Titulo              NVARCHAR(400)  NOT NULL,
    AnioPublicacion     INT            NULL,
    Editorial           NVARCHAR(200)  NULL,
    FechaConsulta       DATETIME2(0)   NOT NULL CONSTRAINT DF_HB_Fecha DEFAULT SYSUTCDATETIME(),
    -- Normalizaciones para índices/únicos
    FechaConsultaMin    AS DATEADD(minute, DATEDIFF(minute, 0, FechaConsulta), 0) PERSISTED,
    AnioPublicacion_NN  AS ISNULL(AnioPublicacion, -1) PERSISTED,
    Editorial_NN        AS ISNULL(Editorial, N'') PERSISTED
  );
END
GO

-- Índice para consultas rápidas por autor y fecha
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = 'IX_HB_Autor_Fecha'
    AND object_id = OBJECT_ID('dbo.HistorialBusquedas')
)
BEGIN
  CREATE INDEX IX_HB_Autor_Fecha
    ON dbo.HistorialBusquedas(Autor, FechaConsulta DESC);
END
GO

-- Índice único para evitar duplicados en la MISMA ventana de 1 minuto
IF EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = 'UX_HB_Dedupe1Min'
    AND object_id = OBJECT_ID('dbo.HistorialBusquedas')
)
BEGIN
  DROP INDEX UX_HB_Dedupe1Min ON dbo.HistorialBusquedas;
END
GO

CREATE UNIQUE INDEX UX_HB_Dedupe1Min
ON dbo.HistorialBusquedas(Autor, Titulo, AnioPublicacion_NN, Editorial_NN, FechaConsultaMin)
WITH (IGNORE_DUP_KEY = ON);
GO

/* =========================================================
   3) TVP y SP para inserción en lote con dedupe < 1 min
   ========================================================= */
IF TYPE_ID('dbo.BookRow') IS NULL
    CREATE TYPE dbo.BookRow AS TABLE(
      Autor           NVARCHAR(200) NOT NULL,
      Titulo          NVARCHAR(400) NOT NULL,
      AnioPublicacion INT           NULL,
      Editorial       NVARCHAR(200) NULL
    );
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertarHistorialDesdeResultados
  @Resultados dbo.BookRow READONLY,
  @FechaConsulta DATETIME2(0) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @FechaConsulta IS NULL SET @FechaConsulta = SYSUTCDATETIME();

  ;WITH R AS (
      SELECT
          Autor      = LTRIM(RTRIM(Autor)),
          Titulo     = LTRIM(RTRIM(Titulo)),
          AnioPublicacion,
          Editorial  = LTRIM(RTRIM(ISNULL(Editorial, N'')))
      FROM @Resultados
      GROUP BY
          LTRIM(RTRIM(Autor)),
          LTRIM(RTRIM(Titulo)),
          AnioPublicacion,
          LTRIM(RTRIM(ISNULL(Editorial, N'')))
  )
  INSERT INTO dbo.HistorialBusquedas(Autor, Titulo, AnioPublicacion, Editorial, FechaConsulta)
  SELECT
      R.Autor,
      R.Titulo,
      R.AnioPublicacion,
      NULLIF(R.Editorial, N'') AS Editorial,
      @FechaConsulta
  FROM R
  WHERE NOT EXISTS (
      SELECT 1
      FROM dbo.HistorialBusquedas h
      WHERE h.Autor  = R.Autor
        AND h.Titulo = R.Titulo
        AND ISNULL(h.AnioPublicacion, -1) = ISNULL(R.AnioPublicacion, -1)
        AND ISNULL(h.Editorial, N'')      = R.Editorial
        AND ABS(DATEDIFF(SECOND, h.FechaConsulta, @FechaConsulta)) < 60
  );
END
GO

/* =========================================================
   4) Tabla de logs de búsquedas
   ========================================================= */
IF OBJECT_ID('dbo.log_HistorialBusquedas') IS NULL
BEGIN
  CREATE TABLE dbo.log_HistorialBusquedas (
    id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    mensaje         NVARCHAR(MAX) NOT NULL,
    fecha_creacion  DATETIME2(0)  NOT NULL CONSTRAINT DF_logHB_fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_log_HB_json CHECK (ISJSON(mensaje) = 1)
  );
END
GO

/* =========================================================
   5) Trigger para guardar log en formato JSON (evitando truncamiento)
   ========================================================= */
CREATE OR ALTER TRIGGER dbo.trg_HistorialBusquedas_AfterInsert
ON dbo.HistorialBusquedas
AFTER INSERT
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @resultados NVARCHAR(MAX) = (
      SELECT i.Id, i.Autor, i.Titulo, i.AnioPublicacion, i.Editorial, i.FechaConsulta
      FROM inserted i
      FOR JSON PATH
  );

  DECLARE @payload NVARCHAR(MAX) = (
      SELECT
        'search_performed' AS evento,
        SYSUTCDATETIME()   AS fecha_evento
      FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
  );

  SET @payload = JSON_MODIFY(@payload, '$.resultados', JSON_QUERY(@resultados));

  INSERT INTO dbo.log_HistorialBusquedas(mensaje) VALUES (@payload);
END
GO

/* =========================================================
   6) Procedimiento para guardar directamente JSON crudo de la API
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.usp_LogBusquedaApi
  @autor   NVARCHAR(200),
  @rawJson NVARCHAR(MAX)
AS
BEGIN
  SET NOCOUNT ON;
  IF (ISJSON(@rawJson) <> 1) THROW 50001, 'El payload no es JSON válido.', 1;
  INSERT INTO dbo.log_HistorialBusquedas(mensaje) VALUES(@rawJson);
END
GO
