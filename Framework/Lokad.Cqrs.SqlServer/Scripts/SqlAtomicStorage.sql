CREATE TABLE [dbo].[Blob]
(
    [HashPath]		BINARY(20) NOT NULL,
    [Path]			NVARCHAR(255) NOT NULL,
    [CreatedOnUtc]	DATETIME NOT NULL,
    [UpdatedOnUtc]	DATETIME NULL,
    [Blob]			VARBINARY(MAX) NOT NULL,

    CONSTRAINT [PK_Blob] PRIMARY KEY CLUSTERED 
    (
        [HashPath] ASC
    )
)
GO

CREATE PROCEDURE [dbo].[Blob_Update]
(
    @path NVARCHAR(255),
    @blob VARBINARY(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @hashPath BINARY(20)
    SELECT @hashPath = HASHBYTES('SHA1', @path)
    
    UPDATE 
        [dbo].[Blob]
    SET 
        Blob = @blob,
        UpdatedOnUtc = GETUTCDATE()
    WHERE 
        HashPath = @hashPath
    
END
GO

CREATE PROCEDURE [dbo].[Blob_Insert]
(
    @path NVARCHAR(255),
    @blob VARBINARY(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @hashPath BINARY(20)
    SELECT @hashPath = HASHBYTES('SHA1', @path)
    
    INSERT INTO [dbo].[Blob]
    (
        HashPath,
        [Path],
        CreatedOnUtc,
        Blob
    )
    VALUES
    (
        @hashPath,
        @path,
        GETUTCDATE(),
        @blob
    )
END
GO

CREATE PROCEDURE [dbo].[Blob_Get]
(
    @path NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @hashPath BINARY(20)
    SELECT @hashPath = HASHBYTES('SHA1', @path)
    
    SELECT 
        Blob
    FROM 
        [dbo].[Blob] WITH (NOLOCK)
    WHERE 
        HashPath = @hashPath
END
GO

CREATE PROCEDURE [dbo].[Blob_Delete]
(
    @path NVARCHAR(255)
)
AS
BEGIN
    
    DECLARE @hashPath BINARY(20)
    SELECT @hashPath = HASHBYTES('SHA1', @path)
    
    DELETE FROM [dbo].[Blob]
    WHERE HashPath = @hashPath
END
GO
