-- Create EF Core Migrations History table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
	CREATE TABLE [__EFMigrationsHistory] (
		[MigrationId] nvarchar(150) NOT NULL,
		[ProductVersion] nvarchar(32) NOT NULL,
		CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
	);
END

-- Register the initial migration as applied
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260822183546_InitialCreate')
BEGIN
	INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
	VALUES ('20260822183546_InitialCreate', '8.0.0');
END

-- Create the BookingDisplayView
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'BookingDisplayView')
BEGIN
	EXEC sp_executesql N'CREATE VIEW dbo.BookingDisplayView AS
	SELECT 
		b.BookingId,
		b.CustomerName,
		b.CustomerEmail,
		b.BookingDate,
		b.NumberOfTickets,
		e.EventId,
		e.EventName,
		e.EventDate,
		e.Description as EventDescription,
		v.VenueId,
		v.VenueName,
		v.Location,
		v.Capacity
	FROM Bookings b
	INNER JOIN Events e ON b.EventId = e.EventId
	INNER JOIN Venues v ON e.VenueId = v.VenueId';
END

-- Register the view migration as applied
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260912184722_AddBookingDisplayView')
BEGIN
	INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
	VALUES ('20260912184722_AddBookingDisplayView', '8.0.0');
END

-- Verify success
SELECT 'Migration setup complete!' as Status;
SELECT * FROM __EFMigrationsHistory;
SELECT 'BookingDisplayView created' as ViewStatus FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'BookingDisplayView';
