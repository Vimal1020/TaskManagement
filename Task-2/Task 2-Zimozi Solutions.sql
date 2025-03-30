--------------------------------------------------------
-- Drop and create the database
--------------------------------------------------------
USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TaskManagement')
BEGIN
    ALTER DATABASE TaskManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TaskManagement;
END
GO
CREATE DATABASE TaskManagement;
GO
USE TaskManagement;
GO

--------------------------------------------------------
-- Create Tables with Constraints
--------------------------------------------------------
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Tasks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Status NVARCHAR(50) NOT NULL CHECK (Status IN ('Pending', 'InProgress', 'Completed')),
    DueDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    CONSTRAINT FK_Tasks_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);
GO

CREATE TABLE TaskHistory (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    OldStatus NVARCHAR(50) NULL,
    NewStatus NVARCHAR(50) NOT NULL,
    ChangedByUserId INT NOT NULL,
    ChangedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TaskHistory_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    CONSTRAINT FK_TaskHistory_ChangedBy FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id)
);
GO

CREATE TABLE UserTasks (
    UserId INT NOT NULL,
    TaskId INT NOT NULL,
    AssignedAt DATETIME2 DEFAULT GETUTCDATE(),
    AssignedBy INT NOT NULL,
    CONSTRAINT PK_UserTasks PRIMARY KEY (UserId, TaskId),
    CONSTRAINT FK_UserTasks_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserTasks_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    CONSTRAINT FK_UserTasks_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(Id)
);
GO

CREATE TABLE TaskComments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    CommentText NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TaskComments_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    CONSTRAINT FK_TaskComments_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE CommentMentions (
    CommentId INT NOT NULL,
    MentionedUserId INT NOT NULL,
    CONSTRAINT PK_CommentMentions PRIMARY KEY (CommentId, MentionedUserId),
    CONSTRAINT FK_CommentMentions_Comment FOREIGN KEY (CommentId) REFERENCES TaskComments(Id),
    CONSTRAINT FK_CommentMentions_MentionedUser FOREIGN KEY (MentionedUserId) REFERENCES Users(Id)
);
GO

CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TaskId INT NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsRead BIT DEFAULT 0,
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Notifications_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id)
);
GO

--------------------------------------------------------
-- Create Optimized Indexes
--------------------------------------------------------
CREATE NONCLUSTERED INDEX IX_Tasks_Status ON Tasks(Status);
CREATE NONCLUSTERED INDEX IX_Tasks_DueDate ON Tasks(DueDate);
CREATE NONCLUSTERED INDEX IX_UserTasks_UserId ON UserTasks(UserId);
CREATE NONCLUSTERED INDEX IX_TaskHistory_TaskId ON TaskHistory(TaskId);
CREATE NONCLUSTERED INDEX IX_TaskComments_TaskId ON TaskComments(TaskId);
GO

--------------------------------------------------------
-- Insert Consistent Sample Data
--------------------------------------------------------
-- Users
INSERT INTO Users (UserName, Email, PasswordHash)
VALUES 
('Rahul', 'rahul@example.com', 'hash1'),
('Raj', 'raj@example.com', 'hash2'),
('Karan', 'karan@example.com', 'hash3');

-- Tasks (All initially Pending)
INSERT INTO Tasks (Title, Description, Status, DueDate, CreatedBy)
VALUES 
('Design Homepage', 'Website main page design', 'Pending', DATEADD(day, 7, GETUTCDATE()), 1),
('Develop API', 'Build REST API', 'Pending', DATEADD(day, 14, GETUTCDATE()), 2),
('Testing', 'Integration testing', 'Pending', DATEADD(day, 10, GETUTCDATE()), 3);

-- User Assignments
INSERT INTO UserTasks (UserId, TaskId, AssignedBy)
VALUES
(1, 1, 1),
(2, 2, 2),
(3, 3, 3);

-- Task History (Ensure valid status transitions)
INSERT INTO TaskHistory (TaskId, OldStatus, NewStatus, ChangedByUserId)
VALUES
(1, NULL, 'Pending', 1),
(1, 'Pending', 'InProgress', 1),
(2, NULL, 'Pending', 2),
(3, NULL, 'Pending', 3);

-- Comments with Mentions
INSERT INTO TaskComments (TaskId, UserId, CommentText)
VALUES
(1, 1, '@Bob Please review the design'),
(2, 2, '@Charlie API specs are ready');

INSERT INTO CommentMentions (CommentId, MentionedUserId)
VALUES
(1, 2), 
(2, 3);

-- Notifications
INSERT INTO Notifications (UserId, TaskId, Message)
VALUES
(2, 1, 'You were mentioned in a comment on Task 1'),
(3, 2, 'API specs ready for review');
GO

--------------------------------------------------------
-- Task Queries
--------------------------------------------------------
-- 1. Get all tasks assigned to a user(Rahul) (paginated & filterable by status, due date).
DECLARE @UserId INT = 1, @Status NVARCHAR(50) = 'Pending';
SELECT t.Id, t.Title, t.Status, t.DueDate 
FROM Tasks t
JOIN UserTasks ut ON t.Id = ut.TaskId
WHERE ut.UserId = @UserId AND t.Status = @Status;


-- 2. Get a task’s full history (status changes, comments, notifications)
DECLARE @TaskId INT = 1;
SELECT 
    'Status' AS EventType, 
    th.ChangedAt AS Timestamp,
    u.UserName AS ChangedBy,
    th.OldStatus,
    th.NewStatus,
    NULL AS Comment
FROM TaskHistory th
JOIN Users u ON th.ChangedByUserId = u.Id
WHERE th.TaskId = @TaskId

UNION ALL

SELECT 
    'Comment' AS EventType,
    tc.CreatedAt,
    u.UserName AS CommentBy,
    NULL,
    NULL,
    tc.CommentText
FROM TaskComments tc
JOIN Users u ON tc.UserId = u.Id
WHERE tc.TaskId = @TaskId
ORDER BY Timestamp DESC;

-- 3. Get users who interacted with a specific task (commented, updated status)
SELECT DISTINCT u.Id, u.UserName, 'Assignee' AS Role
FROM UserTasks ut
JOIN Users u ON ut.UserId = u.Id
WHERE ut.TaskId = 1

UNION

SELECT DISTINCT u.Id, u.UserName, 'Commenter'
FROM TaskComments tc
JOIN Users u ON tc.UserId = u.Id
WHERE tc.TaskId = 1

UNION

SELECT DISTINCT u.Id, u.UserName, 'Status Changer'
FROM TaskHistory th
JOIN Users u ON th.ChangedByUserId = u.Id
WHERE th.TaskId = 1;
GO