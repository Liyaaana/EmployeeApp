ALTER PROCEDURE SearchEmployees
    @SearchTerm NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
    @MinSalary DECIMAL(18, 2) = NULL,
    @MaxSalary DECIMAL(18, 2) = NULL
AS
BEGIN
    SELECT Id, Name, Email, PhoneNumber, Department, Salary
    FROM Employees
    WHERE
        (@SearchTerm IS NULL OR Name LIKE @SearchTerm + '%')
        AND (@Department IS NULL OR Department LIKE @Department + '%')
        AND (@MinSalary IS NULL OR Salary >= @MinSalary)
        AND (@MaxSalary IS NULL OR Salary <= @MaxSalary)
    ORDER BY Name;
END
