CREATE TABLE [dbo].[ArchivedMessages]
(
    [MessageID]				BIGINT NOT NULL,
    [EnvelopeID]			NVARCHAR(255) NOT NULL,
    [QueueID]				INT NOT NULL,
    [VisibilityStartTime]	DATETIME NOT NULL,
    [CreatedOnUtc]			DATETIME NOT NULL,
    [DequeueCount]			INT NOT NULL,
    [Envelope]				VARBINARY(MAX) NOT NULL,
    [CompletedOnUtc]		DATETIME NOT NULL,

    CONSTRAINT [PK_ArchivedMessages] PRIMARY KEY CLUSTERED 
    (
        [MessageID] ASC
    )
)
GO

CREATE TABLE [dbo].[Queues]
(
    [QueueID]	INT IDENTITY(1,1) NOT NULL,
    [Name]		NVARCHAR(50) NOT NULL,

    CONSTRAINT [PK_Queues] PRIMARY KEY CLUSTERED 
    (
        [QueueID] ASC
    )
)
GO

CREATE TABLE [dbo].[Messages]
(
    [MessageID]				BIGINT IDENTITY(1,1) NOT NULL,
    [EnvelopeID]			NVARCHAR(255) NOT NULL,
    [QueueID]				INT NOT NULL,
    [VisibilityStartTime]	DATETIME NOT NULL,
    [CreatedOnUtc]			DATETIME NOT NULL,
    [DequeueCount]			INT NOT NULL,
    [Envelope]				VARBINARY(MAX) NOT NULL,

    CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
    (
        [MessageID] ASC
    )
)
GO

CREATE PROCEDURE [dbo].[GetQueueID]
(
    @name NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @queueID INT
    SELECT @queueID = QueueID FROM [dbo].[Queues] WHERE Name = @name

    IF @queueID IS NULL
    BEGIN
        INSERT INTO [dbo].[Queues](Name) VALUES(UPPER(@name)) 
        SELECT @queueID = QueueID FROM [dbo].[Queues] WHERE Name = @name
    END
    
    SELECT @queueID
END
GO

CREATE PROCEDURE [dbo].[Enqueue]
(
    @queueID		INT,
    @envelopeID		NVARCHAR(255),
    @createdOnUtc	DATETIME,
    @deliverOnUtc	DATETIME,
    @envelope		VARBINARY(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[Messages]
    (
        QueueID,
        EnvelopeID,
        VisibilityStartTime,
        CreatedOnUtc,
        DequeueCount,
        Envelope
    )
    VALUES
    (
        @queueID,
        @envelopeID,
        @deliverOnUtc,
        @createdOnUtc,
        0,
        @envelope
    )
END
GO

CREATE PROCEDURE [dbo].[Dequeue]
(
    @queueID			INT,
    @visibilityTimeout	INT,
    @dequeueCount		INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @currentTime DATETIME
    SET @currentTime = GETUTCDATE()

    DECLARE @newVisibilityTime DATETIME
    SET @newVisibilityTime = DATEADD(SS, @visibilityTimeout, GETUTCDATE())

    UPDATE [dbo].[Messages]
    SET 
        VisibilityStartTime = @newVisibilityTime,
        DequeueCount = DequeueCount + 1
    OUTPUT 
        inserted.MessageID,
        inserted.EnvelopeID,
        inserted.Envelope,
        inserted.CreatedOnUtc,
        inserted.DequeueCount
    WHERE MessageID IN
    (
        SELECT TOP(@dequeueCount)
            MessageID
        FROM 
            [dbo].[Messages] WITH (ROWLOCK, READPAST)
        WHERE 
            QueueID = @queueID
            AND VisibilityStartTime <= @currentTime
        ORDER BY
            VisibilityStartTime, CreatedOnUtc
    )
END
GO

CREATE PROCEDURE [dbo].[Delete]
(
    @messageID BIGINT
)
AS
BEGIN
    DELETE FROM [dbo].[Messages]
        OUTPUT
        deleted.[MessageID],
        deleted.[EnvelopeID],
        deleted.[QueueID],
        deleted.[VisibilityStartTime],
        deleted.[CreatedOnUtc],
        deleted.[DequeueCount],
        deleted.[Envelope],
        GETUTCDATE() AS [CompletedOnUtc]
    INTO 
        [dbo].[ArchivedMessages]
    WHERE 
        MessageID = @messageID
END
GO

ALTER TABLE [dbo].[Messages] WITH CHECK ADD CONSTRAINT [FK_Messages_Queues] FOREIGN KEY([QueueID]) REFERENCES [dbo].[Queues] ([QueueID])
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_Messages_Queues]
GO

CREATE NONCLUSTERED INDEX [IX_Messages] ON [dbo].[Messages] 
(
    [QueueID] ASC,
    [VisibilityStartTime] ASC
)
GO
