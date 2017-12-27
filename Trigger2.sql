CREATE TRIGGER NewCourseAbsenceEntries
   ON  [dbo].[UserCourses] 
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @userId INT;
	SET @userId = (SELECT DISTINCT UserID FROM inserted);

	DECLARE @courseId INT;
	SET @courseId = (SELECT DISTINCT CourseID FROM inserted);

	IF (SELECT COUNT(*) FROM AbsenceItems WHERE CourseID = @courseId) > 0
	BEGIN
		INSERT INTO UserAbsences
		SELECT @userId, @courseId, AbsenceCategoryID, 0 FROM AbsenceItems WHERE CourseID = @courseId;
		PRINT 'Inserted needed absence entries';
	END;
	ELSE
		PRINT 'No absence requirements';
END
