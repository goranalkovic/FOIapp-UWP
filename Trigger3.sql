CREATE TRIGGER NewUserSettings
   ON  Users
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @userId INT;
	SET @userId = (SELECT TOP 1 UserID FROM inserted);

	INSERT INTO UserSettings VALUES (@userId, 1);
	
	PRINT 'Inserted needed settings entries';
END
