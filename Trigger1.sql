CREATE TRIGGER NewCoursePointEntries
   ON  [dbo].[UserCourses] 
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @userId INT;
	SET @userId = (SELECT DISTINCT UserID FROM inserted);

	DECLARE @courseId INT;
	SET @courseId = (SELECT DISTINCT CourseID FROM inserted);

	INSERT INTO UserPoints
	SELECT @userId, 0, CourseItemID FROM CourseItems WHERE CourseID = @courseId;

	PRINT 'Inserted needed course entries';
END
