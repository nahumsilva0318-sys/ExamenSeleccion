--Validación de la existencia de la BD
IF DB_ID('BdiExamen') IS NULL
	CREATE DATABASE BdiExamen;
GO
--Creación de la BD
USE BdiExamen;
GO

--Validación de la existencia de la TABLA
IF OBJECT_ID(N'dbo.tblExamen', 'U') IS NOT NULL
 DROP TABLE dbo.tblExamen;
GO

--Creación de la tabla tblExamen
CREATE TABLE dbo.tblExamen(
	Id INT NOT NULL,
	Nombre VARCHAR(255) NULL,
	Descripcion VARCHAR(255) NULL,
	CONSTRAINT PK_tblExamen PRIMARY KEY CLUSTERED (Id)
);
GO

--Creación de los SPs

--SP para agregar
CREATE OR ALTER PROCEDURE dbo.spAgregar
	@Id INT,
	@Nombre VARCHAR(255),
	@Descripcion  VARCHAR(255)
AS
BEGIN 
	SET NOCOUNT ON;

	--Inician Validaciones

	--Validación de el id, no puede ser menor o igual a 0 ni nulo
	IF @Id <=0 OR @Id IS NULL
	BEGIN
		SELECT 1 AS CodigoRetorno, 'El ID debe ser mayor a cero' AS DescripcionRetorno;
		RETURN;
	END

	--Validación del Nombre, no puede ser vacio o nulo 
	IF NULLIF(@Nombre,'') IS NULL
	BEGIN
		SELECT 2 AS CodigoRetorno, 'El Nombre es obligatorio' AS DescripcionRetorno;
		RETURN;
	END

	--Validación de duplicidad de id, si ya existe no lo deja crear
	IF EXISTS (SELECT 1 FROM dbo.tblExamen WHERE Id = @Id)
	BEGIN
		SELECT 3 AS CodigoRetorno,'Ya existe un registro con el Id ' + CAST(@Id AS NVARCHAR(10))  AS DescripcionRetorno;
		RETURN;
	END

	BEGIN TRY
		INSERT INTO dbo.tblExamen (Id, Nombre, Descripcion) VALUES (@Id,@Nombre,@Descripcion);
		SELECT 0 AS CodigoRetorno, 'Registro insertado satisfactoriamente' AS DescripcionRetorno;
	END TRY

	BEGIN CATCH
		 SELECT ERROR_NUMBER() AS CodigoRetorno, ERROR_MESSAGE() AS DescripcionRetorno;
	END CATCH
END
GO

--SP para actualizar

CREATE OR ALTER PROCEDURE dbo.spActualizar
	 @Id INT,
	 @Nombre VARCHAR(255),
	 @Descripcion VARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

--Inician Validaciones

	--Validación de el id, no puede ser menor o igual a 0 ni nulo
	IF @Id <=0 OR @Id IS NULL
	BEGIN
		SELECT 1 AS CodigoRetorno, 'El ID debe ser mayor a cero' AS DescripcionRetorno;
		RETURN;
	END

	--Validación del Nombre, no puede ser vacio o nulo 
	IF NULLIF(@Nombre,'') IS NULL
	BEGIN
		SELECT 2 AS CodigoRetorno, 'El Nombre es obligatorio' AS DescripcionRetorno;
		RETURN;
	END

	--Validación de que el id existe, si no existe no debe proseguir 
	IF NOT EXISTS (SELECT 1 FROM dbo.tblExamen WHERE Id = @Id)
	BEGIN
		SELECT 4 AS CodigoRetorno,'No existe un registro con el Id ' + CAST(@Id AS NVARCHAR(10))   AS DescripcionRetorno;
		RETURN;
	END

 BEGIN TRY
	
	 UPDATE dbo.tblExamen SET Nombre = @Nombre,Descripcion = @Descripcion WHERE Id = @Id;
	 SELECT 0 AS CodigoRetorno, 'Registro actualizado satisfactoriamente' AS DescripcionRetorno;
 END TRY

 BEGIN CATCH
	 SELECT ERROR_NUMBER() AS CodigoRetorno, ERROR_MESSAGE() AS DescripcionRetorno;
 END CATCH
END
GO


--SP para Eliminar 
CREATE OR ALTER PROCEDURE dbo.spEliminar
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	--Inician Validaciones

	--Validación de el id, no puede ser menor o igual a 0 ni nulo
	IF @Id <=0 OR @Id IS NULL
	BEGIN
		SELECT 1 AS CodigoRetorno, 'El ID debe ser mayor a cero' AS DescripcionRetorno;
		RETURN;
	END

	--Validación de que el id existe, si no existe no debe proseguir 
	IF NOT EXISTS (SELECT 1 FROM dbo.tblExamen WHERE Id = @Id)
	BEGIN
		SELECT 4 AS CodigoRetorno,'No existe un registro con el Id ' + CAST(@Id AS NVARCHAR(10))   AS DescripcionRetorno;
		RETURN;
	END

	BEGIN TRY
		DELETE FROM dbo.tblExamen WHERE Id = @Id;
		SELECT 0 AS CodigoRetorno, 'Registro eliminado satisfactoriamente' AS DescripcionRetorno;
	END TRY

	BEGIN CATCH 
		SELECT ERROR_NUMBER() AS CodigoRetorno, ERROR_MESSAGE() AS DescripcionRetorno;
	END CATCH
END
GO

--SP para Consultar
CREATE OR ALTER PROCEDURE dbo.spConsultar
	@Nombre VARCHAR(255) = NULL,
	@Descripcion VARCHAR(255) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id, Nombre, Descripcion
	FROM dbo.tblExamen
	WHERE (NULLIF(@Nombre,'') IS NULL OR Nombre LIKE '%'+@Nombre+'%')
	AND (NULLIF(@Descripcion,'') IS NULL OR Descripcion LIKE '%'+@Descripcion+'%')
	ORDER BY Id;
END
GO



	









