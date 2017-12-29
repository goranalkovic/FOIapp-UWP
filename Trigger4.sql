 CREATE TRIGGER DeleteCourseAbsenceEntries
   ON  [dbo].[UserCourses] 
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @userId INT;
	SET @userId = (SELECT DISTINCT UserID FROM deleted);

	DECLARE @courseId INT;
	SET @courseId = (SELECT DISTINCT CourseID FROM deleted);

	BEGIN
		DELETE FROM UserAbsences WHERE CourseID = @courseId AND UserID = @userId;
		PRINT 'Removed absence entries';
	END;
END
