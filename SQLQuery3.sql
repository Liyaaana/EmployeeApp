ALTER PROCEDURE GetEmployeeAuditLogs
	@StartDate DATETIME = NULL,
	@EndDate DATETIME = NULL,
	@EmployeeCode NVARCHAR(100) = NULL
AS 
BEGIN
	SELECT 
		al.Id,
		al.EmployeeCode,
		u.FullName,
		al.PageVisited,
		al.ActionType,
		al.ActionTime
FROM EmployeeAuditLogs al
JOIN AspNetUsers u ON al.EmployeeCode = u.EmployeeCode
WHERE (@StartDate IS NULL OR al.ActionTime >= @StartDate)
	AND (@EndDate IS NULL OR al.ActionTime <= @EndDate)
	AND (@EmployeeCode IS NULL OR al.EmployeeCode = @EmployeeCode)
 ORDER BY al.ActionTime DESC
END