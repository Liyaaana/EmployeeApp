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

    -- First result set: single-column (TotalCount), single-row table
    SELECT COUNT(*) AS TotalCount
    FROM EmployeeAuditLogs al
    INNER JOIN AspNetUsers u ON al.EmployeeCode = u.EmployeeCode
    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin'
      AND (@EmployeeCode IS NULL OR u.EmployeeCode = @EmployeeCode)
      AND (@StartDate IS NULL OR CAST(al.ActionTime AS DATE) >= @StartDate)
      AND (@EndDate IS NULL OR CAST(al.ActionTime AS DATE) <= @EndDate);

    -- Second result set: table containing logs
    SELECT al.Id, al.PageVisited, al.ActionType, al.ActionTime,
           u.EmployeeCode, u.FullName
    FROM EmployeeAuditLogs al
    INNER JOIN AspNetUsers u ON al.EmployeeCode = u.EmployeeCode
    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin'
      AND (@EmployeeCode IS NULL OR u.EmployeeCode = @EmployeeCode)
      AND (@StartDate IS NULL OR CAST(al.ActionTime AS DATE) >= @StartDate)
      AND (@EndDate IS NULL OR CAST(al.ActionTime AS DATE) <= @EndDate)
    ORDER BY al.ActionTime DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
