-- Add sample data for testing

-- Insert test venues
INSERT INTO Venues (VenueName, Location, Capacity, ImageUrl)
VALUES 
	('Grand Hall', 'Downtown', 500, 'https://example.com/grand.jpg'),
	('Small Theater', 'Midtown', 150, 'https://example.com/theater.jpg'),
	('Outdoor Park', 'Suburb', 1000, 'https://example.com/park.jpg');

-- Insert test events
INSERT INTO Events (EventName, EventDate, Description, VenueId, ImageUrl)
VALUES 
	('Summer Concert', '2025-06-15', 'Live music performance', 1, 'https://example.com/concert.jpg'),
	('Tech Conference', '2025-07-20', 'Technology and innovation', 1, 'https://example.com/tech.jpg'),
	('Comedy Night', '2025-06-22', 'Stand-up comedy show', 2, 'https://example.com/comedy.jpg'),
	('Wedding Reception', '2025-08-10', 'Private event', 2, 'https://example.com/wedding.jpg');

-- Insert test bookings
INSERT INTO Bookings (CustomerName, CustomerEmail, BookingDate, NumberOfTickets, EventId)
VALUES 
	('John Smith', 'john.smith@email.com', '2025-01-10', 4, 1),
	('Sarah Johnson', 'sarah.j@email.com', '2025-01-11', 2, 1),
	('Mike Wilson', 'mike.w@email.com', '2025-01-12', 6, 2),
	('Emma Davis', 'emma.d@email.com', '2025-01-13', 2, 3),
	('David Brown', 'david.b@email.com', '2025-01-14', 10, 4);

SELECT 'Sample data inserted successfully!' as Status;
