# EventEase Part 2 - Implementation Guide

## Overview

This document outlines all the improvements and enhancements implemented in Part 2 of the EventEase project, including error handling, validation, Azure Blob Storage integration, and enhanced search functionality.

## Completed Features

### 1. Error Handling and Validation

#### Business Logic Validation
- **Double Booking Prevention**: The system now prevents venues from being booked for two different events on the same date and time
- **Deletion Restrictions**: Users cannot delete venues or events that have associated bookings
- **Required Field Validation**: All models validate required fields before allowing operations

#### Implementation Details
- Created `BookingValidationService` in `Services/` folder
- Service methods:
  - `ValidateVenueAvailability()` - Checks venue availability on specific dates
  - `CanDeleteVenue()` - Checks if venue has active bookings
  - `CanDeleteEvent()` - Checks if event has active bookings
  - `ValidateVenue()`, `ValidateEvent()`, `ValidateBooking()` - Field-level validation

#### User Experience
- Error messages display in alert boxes on all forms
- Validation failures prevent data loss
- Clear, actionable error messages help users understand and fix issues

### 2. Database Enhancements

#### BookingDisplayView
A new SQL view that consolidates booking information from three tables:
- **Bookings**: Customer booking records
- **Events**: Event details
- **Venues**: Venue information

```sql
CREATE VIEW dbo.BookingDisplayView AS
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
INNER JOIN Venues v ON e.VenueId = v.VenueId
```

**Benefits**:
- Provides enriched booking information for display
- Reduces multiple queries to a single view query
- Makes data relationships explicit

### 3. Search Functionality

#### Bookings Search
The Bookings Index page now includes advanced search:
- **Search by Booking ID**: Direct lookup of specific bookings
- **Search by Event Name**: Find bookings related to specific events
- **Search All Fields**: Global search across customer names, emails, and event names

#### Implementation
- Modified `IndexModel` in `Pages/Bookings/Index.cshtml.cs`
- Added search parameters: `SearchTerm` and `SearchType`
- Displays search results with enhanced column visibility (Venue information included)
- Clear button to reset search

### 4. Azure Blob Storage Integration

#### Setup
1. Update `appsettings.json` with your Azure Storage credentials:
```json
"AzureStorage": {
	"ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT_NAME;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core.windows.net",
	"ContainerName": "eventease-images"
}
```

2. The `AzureStorageService` is automatically registered if valid credentials are provided

#### Service Methods
- `UploadImageAsync(IFormFile file, string categoryName)` - Upload images to blob storage
- `DeleteImageAsync(string blobUri)` - Remove images from storage
- `BlobExistsAsync(string blobUri)` - Check blob existence

#### Capacity
- Supports storing venue and event images
- Generates unique filenames using GUIDs
- Returns public blob URIs for web display

### 5. Updated Page Handlers

All CRUD page handlers now include:
- Input validation
- Error handling
- Informative error messages
- Exception catching for unexpected errors

**Updated Handlers:**
- `Events/Create.cshtml.cs` - Validates events and checks venue availability
- `Events/Edit.cshtml.cs` - Preserves validation during updates
- `Events/Delete.cshtml.cs` - Prevents deletion of events with bookings
- `Venues/Create.cshtml.cs` - Validates required venue fields
- `Venues/Edit.cshtml.cs` - Ensures valid venue data
- `Venues/Delete.cshtml.cs` - Prevents deletion of venues with events
- `Bookings/Create.cshtml.cs` - Validates booking data
- `Bookings/Delete.cshtml.cs` - Safe booking removal

### 6. Enhanced User Interface

#### Error Display
- Bootstrap alert boxes for validation errors
- Color-coded alerts (danger for errors)
- Dismissible alerts with close buttons

#### Search Interface
- Dropdown selector for search type
- Text input for search terms
- Clear button to reset filters
- Responsive design

#### Improved Data Display
The Bookings Index now shows:
- Booking ID (unique identifier)
- Customer Name & Email
- Event Name & Date
- Venue Name & Location
- Booking Date

## Application Architecture

### Services Layer
```
Services/
├── BookingValidationService.cs      (Business logic validation)
└── AzureStorageService.cs            (Cloud storage management)
```

### Models
```
Models/
├── BookingDisplayView.cs             (Database view model)
├── ValidationResult.cs               (Standardized validation responses)
├── Booking.cs
├── Event.cs
└── Venue.cs
```

### Data Layer
```
Data/
├── EventEaseDbContext.cs             (Updated with BookingDisplayView)
└── Migrations/
	└── 20260912184722_AddBookingDisplayView.cs
```

## Deployment to Azure

### Prerequisites
- Azure SQL Database
- Azure Storage Account
- Azure App Service

### Steps

1. **Update Connection Strings**
   - Replace `DefaultConnection` with your Azure SQL connection string
   - Replace `AzureStorage` configuration with your storage account credentials

2. **Database Migration**
   - Run `dotnet ef database update --startup-project EventEase` to apply migrations
   - Ensure the BookingDisplayView is created in your Azure SQL database

3. **Publish Application**
   ```bash
   dotnet publish -c Release
   ```

4. **Upload to Azure App Service**
   - Use Visual Studio publish profile
   - Or use Azure CLI:
   ```bash
   az webapp up --name <app-name> --resource-group <resource-group>
   ```

5. **Configure Application Settings**
   - Set `AzureStorage:ConnectionString` in App Service Configuration
   - Set `DefaultConnection` in App Service Configuration

## Testing Validation

### Test Double Booking Prevention
1. Create a venue
2. Create an event for that venue on Jan 1, 2025
3. Try to create another event for the same venue on Jan 1, 2025
4. Expected: Error message displayed

### Test Deletion Restrictions
1. Create a venue with an event
2. Create a booking for that event
3. Try to delete the venue
4. Expected: Error message - "Cannot delete venue with active bookings"

### Test Search Functionality
1. Navigate to Bookings page
2. Enter a booking ID in search
3. Select "Booking ID" from dropdown
4. Click Search
5. Expected: Only matching booking(s) displayed

### Test Field Validation
1. Go to Create Venue page
2. Leave VenueName empty
3. Click Create
4. Expected: Error message "Venue name is required"

## Configuration Guide

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=.;Database=EventEaseDB;..."
  },
  "AzureStorage": {
	"ConnectionString": "DefaultEndpointsProtocol=https;...",
	"ContainerName": "eventease-images"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning"
	}
  },
  "AllowedHosts": "*"
}
```

## API/Service Contracts

### BookingValidationService
```csharp
public class BookingValidationService
{
	public Task<ValidationResult> ValidateVenueAvailability(int venueId, DateTime eventDate);
	public Task<ValidationResult> CanDeleteVenue(int venueId);
	public Task<ValidationResult> CanDeleteEvent(int eventId);
	public ValidationResult ValidateVenue(Venue venue);
	public ValidationResult ValidateEvent(Event evt);
	public ValidationResult ValidateBooking(Booking booking);
}
```

### AzureStorageService
```csharp
public class AzureStorageService
{
	public Task<string> UploadImageAsync(IFormFile file, string categoryName);
	public Task<bool> DeleteImageAsync(string blobUri);
	public Task<bool> BlobExistsAsync(string blobUri);
}
```

### ValidationResult
```csharp
public class ValidationResult
{
	public bool IsSuccess { get; set; }
	public string Message { get; set; }
	public List<string> Errors { get; set; }

	public static ValidationResult Success(string message);
	public static ValidationResult Failure(string message, params string[] errors);
}
```

## Performance Considerations

1. **Database View**: The BookingDisplayView uses INNER JOINs, ensuring only complete booking records are returned
2. **Indexing**: Ensure indexes exist on:
   - `Venues.VenueId`
   - `Events.EventId`, `Events.VenueId`
   - `Bookings.BookingId`, `Bookings.EventId`

3. **Search Optimization**: Consider adding full-text search if booking database grows > 10,000 records

## Security

1. **Input Validation**: All user inputs are validated server-side
2. **SQL Safety**: Entity Framework protects against SQL injection
3. **Authentication**: Implement authentication before production (not included in Part 2)

## Known Limitations

1. Azure Storage service gracefully skips if credentials not configured
2. Search currently uses LIKE queries (not full-text search)
3. No soft-delete implemented (deleted records are permanently removed)
4. No audit trail of changes

## Future Enhancements

1. Implement soft-delete for auditing
2. Add full-text search for large datasets
3. Implement image optimization and thumbnail generation
4. Add role-based access control (admin, staff)
5. Implement real-time availability calendar
6. Add email notifications for bookings
7. Implement payment processing
8. Add multi-tenant support

## Troubleshooting

### "Cannot connect to SQL Server"
- Check connection string in appsettings.json
- Ensure SQL Server is running
- Verify database name exists

### "Azure Storage not configured"
- Verify appsettings.json has valid Azure Storage credentials
- Check container name exists in Storage Account
- Verify connection string format

### "View already exists"
- If migration fails, drop the view and retry:
  ```sql
  DROP VIEW IF EXISTS dbo.BookingDisplayView
  ```

### Validation errors not displaying
- Ensure error alert div is in Razor template
- Check ErrorMessage property is populated in page model

## Support

For issues or questions, refer to:
- Azure Documentation: https://docs.microsoft.com/azure/
- Entity Framework Core: https://docs.microsoft.com/ef/core/
- ASP.NET Core Razor Pages: https://docs.microsoft.com/aspnet/core/razor-pages/
