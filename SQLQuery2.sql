CREATE TABLE EmployeeAuditLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EmployeeCode NVARCHAR(100),
    PageVisited NVARCHAR(200),
    ActionTime DATETIME,
    ActionType NVARCHAR(50) 
);

