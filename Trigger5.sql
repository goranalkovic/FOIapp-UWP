CREATE TRIGGER DeleteCoursePointEntries
   ON  [dbo].[UserCourses] 
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @userId INT;
	SET @userId = (SELECT DISTINCT UserID FROM deleted);

	DECLARE @courseId INT;
	SET @courseId = (SELECT DISTINCT CourseID FROM deleted);

	DECLARE @min INT;
	SET @min = (SELECT MIN(CourseItemID) FROM CourseItems WHERE CourseID = @courseId);

	DECLARE @max INT;
	SET @max = (SELECT MAX(CourseItemID) FROM CourseItems WHERE CourseID = @courseId);
	
	DELETE FROM UserPoints WHERE UserID = @userId AND CourseItemID BETWEEN @min AND @max;

	PRINT 'Removed needed course entries';
END
