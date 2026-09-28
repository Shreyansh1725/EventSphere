USE EventSphereDB;
GO

-- Passwords are "Admin@123" and "Student@123" hashed via SHA256 "EventSphere_Salt_..." logic
INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive) 
VALUES ('System Admin', 'admin@gmail.com', 'V93FtxDEIx+0yTRixKDPIxKoE6OjUjg6JC1WRVsEFy0=', 'Admin', 1);

INSERT INTO Users (FullName, EnrollmentNumber, Email, Department, Semester, PasswordHash, Role, IsActive) 
VALUES ('John Doe', 'EN2026001', 'student@eventsphere.com', 'Computer Engineering', 5, 'isUGKPCRN+0r2QdShpiaT7+2dmWe6Jxauq8aHbLhaM0=', 'Student', 1);

INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive) 
VALUES ('Event Coordinator 1', 'coordinator@gmail.com', 'sYjtOgnt8ts5z2qDcq0CnrwlWomN8DKIU87s+xHVtZE=', 'Coordinator', 1);

INSERT INTO EventCategories (CategoryName, Description) VALUES
('Technical', 'Coding and tech events'),
('Cultural', 'Arts, dance, and music'),
('Sports', 'Athletics and games'),
('Workshop', 'Hands-on learning sessions'),
('Seminar', 'Guest lectures and talks'),
('Competition', 'Competitive events');

DECLARE @TechId INT = (SELECT CategoryId FROM EventCategories WHERE CategoryName = 'Technical');
DECLARE @CultId INT = (SELECT CategoryId FROM EventCategories WHERE CategoryName = 'Cultural');
DECLARE @WorkId INT = (SELECT CategoryId FROM EventCategories WHERE CategoryName = 'Workshop');

INSERT INTO Events (EventName, Description, CategoryId, Venue, EventDate, StartTime, EndTime, Organizer, MaximumParticipants, AvailableSeats, Status) VALUES
('Tech Fest 2026', 'Annual technology festival.', @TechId, 'Main Auditorium', DATEADD(day, 10, GETDATE()), '09:00', '17:00', 'CS Department', 500, 500, 'Published'),
('AI & Machine Learning Workshop', 'Hands-on ML workshop.', @WorkId, 'Lab 1', DATEADD(day, 5, GETDATE()), '10:00', '14:00', 'AI Club', 50, 50, 'Published'),
('Web Development Bootcamp', 'Learn modern web dev.', @WorkId, 'Lab 2', DATEADD(day, 15, GETDATE()), '09:00', '16:00', 'Web Club', 60, 60, 'Published'),
('Coding Competition', 'Algorithmic challenges.', @TechId, 'Lab 3', DATEADD(day, 2, GETDATE()), '11:00', '14:00', 'CS Department', 100, 100, 'Published'),
('Cultural Fest', 'Annual cultural festival.', @CultId, 'College Ground', DATEADD(day, 20, GETDATE()), '18:00', '22:00', 'Student Council', 1000, 1000, 'Published');
GO
