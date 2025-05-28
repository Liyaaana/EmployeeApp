ALTER PROCEDURE GetEmployeeAuditLogs 
    @EmployeeCode NVARCHAR(50) = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 8
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- First result set: Total Count only 1 row
    SELECT COUNT(*) AS TotalCount
    FROM EmployeeAuditLogs al
    INNER JOIN AspNetUsers u ON al.EmployeeCode = u.EmployeeCode
    WHERE (@EmployeeCode IS NULL OR u.EmployeeCode = @EmployeeCode)
      AND (@StartDate IS NULL OR CAST(al.ActionTime AS DATE) >= @StartDate)
      AND (@EndDate IS NULL OR CAST(al.ActionTime AS DATE) <= @EndDate);

    -- Second result set: data
    SELECT al.Id, al.PageVisited, al.ActionType, al.ActionTime,
           u.EmployeeCode, u.FullName
    FROM EmployeeAuditLogs al
    INNER JOIN AspNetUsers u ON al.EmployeeCode = u.EmployeeCode
    WHERE (@EmployeeCode IS NULL OR u.EmployeeCode = @EmployeeCode)
      AND (@StartDate IS NULL OR CAST(al.ActionTime AS DATE) >= @StartDate)
      AND (@EndDate IS NULL OR CAST(al.ActionTime AS DATE) <= @EndDate)
    ORDER BY al.ActionTime DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END