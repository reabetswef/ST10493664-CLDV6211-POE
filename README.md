## ST10493664-CLDV6211-POE
# EventEase - Venue Booking System

## Links
YouTube Link:

## 📋 Project Overview
EventEase is a comprehensive venue booking management system developed for EventEase, an event management company. This application streamlines venue management, prevents booking conflicts, provides clear visibility of scheduled events, and enables booking specialists to manage venue bookings efficiently.

### 🎯 Key Features (Part 1)
- **Complete CRUD Operations** for Venues, Events, and Bookings
- **Local Database Persistence** using SQL LocalDB
- **Data Validation** with comprehensive error handling
- **Prevention of Double Bookings** for venues
- **Delete Restriction Logic** preventing deletion of venues/events with existing bookings
- **User-Friendly Interface** with responsive design
- **Dashboard Overview** showing key metrics and recent bookings

### Database Tables
#### Venue Table
| Column | Type | Constraints |
|--------|------|-------------|
| VenueId | INT | PRIMARY KEY, IDENTITY |
| VenueName | NVARCHAR(100) | NOT NULL |
| Location | NVARCHAR(200) | NOT NULL |
| Capacity | INT | NOT NULL |
| ImageUrl | NVARCHAR(MAX) | NULL |

#### Event Table
| Column | Type | Constraints |
|--------|------|-------------|
| EventId | INT | PRIMARY KEY, IDENTITY |
| EventName | NVARCHAR(100) | NOT NULL |
| Description | NVARCHAR(500) | NOT NULL |
| StartDate | DATETIME2 | NOT NULL |
| EndDate | DATETIME2 | NOT NULL |
| ImageUrl | NVARCHAR(MAX) | NULL |

#### Booking Table
| Column | Type | Constraints |
|--------|------|-------------|
| BookingId | INT | PRIMARY KEY, IDENTITY |
| VenueId | INT | FOREIGN KEY, NOT NULL |
| EventId | INT | FOREIGN KEY, NOT NULL |
| BookingDate | DATETIME2 | NOT NULL |
| CustomerName | NVARCHAR(100) | NOT NULL |
| CustomerEmail | NVARCHAR(100) | NULL |
| CustomerPhone | NVARCHAR(20) | NULL |
| Status | INT | NOT NULL |

## Getting Started
### Prerequisites
- **Visual Studio 2022** (with ASP.NET and web development workload)
- **.NET 8.0 SDK**
- **SQL Server LocalDB** (comes with Visual Studio)
- **Git** (for version control)

### Installation
1. **Clone the repository**

2. **Open the solution**

3. **Open Visual Studio 2022**

4. **Open the EventEase.sln file**

5. **Restore NuGet packages**

6. **Configure the database connection**

7. **Create and update the database**
# Open Package Manager Console
- Add-Migration InitialCreate
- Update-Database
- Run the application

8. **Press F5 or click "Run" in Visual Studio**

9. **The application will open at https://localhost:5001 or http://localhost:5000**

### Features Demonstrated
1. Venue Management
- Create, Read, Update, Delete venues
- View venues in card layout with images
- Prevent deletion of venues with existing bookings

2. Event Management
- Create, Read, Update, Delete events
- Date validation (end date must be after start date)
- Prevent deletion of events with existing bookings

3. Booking Management
- Create, Read, Update, Delete bookings
- Double booking prevention (one venue per day)
- Date range validation (booking date must be within event dates)
- Status tracking (Confirmed, Pending, Cancelled)

4. Dashboard
- Overview statistics (total venues, events, bookings)
- Recent bookings display
- Quick navigation to all modules

### Validation Rules
1. Venue Validation
- Field	Validation Rule
- VenueName	Required, Max 100 characters
- Location	Required, Max 200 characters
- Capacity	Required, Between 1 and 10,000
- ImageUrl	Optional, URL format

2. Event Validation
- Field	Validation Rule
- EventName	Required, Max 100 characters
- Description	Required, Max 500 characters
- StartDate	Required, Cannot be in past
- EndDate	Required, Must be after StartDate
- ImageUrl	Optional, URL format

3. Booking Validation
- Field	Validation Rule
- Venue	Required, Must exist
- Event	Required, Must exist
- BookingDate	Required, Must be within Event date range
- CustomerName	Required, Max 100 characters
- CustomerEmail	Optional, Valid email format
- CustomerPhone	Optional, Valid phone format
- Status	Required, Enum value


### Testing
# Test the Application
1. Create a Venue
Navigate to Venues → Create New Venue
Fill in venue details with placeholder image URL
Submit and verify creation

2. Create an Event
Navigate to Events → Create New Event
Set start and end dates
Submit and verify creation

3. Create a Booking
Navigate to Bookings → Create New Booking
Select venue, event, and date within event range
Submit and verify creation

4. Test Validations
Try creating a booking outside event date range
Try double booking a venue on same date
Try deleting a venue with existing bookings


### Dependencies
Package	Version	Purpose
Microsoft.EntityFrameworkCore.SqlServer	8.0.0	SQL Server database provider
Microsoft.EntityFrameworkCore.Tools	8.0.0	EF Core migration tools
Microsoft.EntityFrameworkCore.Design	8.0.0	EF Core design-time support

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Authors
Name: Reabetswe Tebogo Fafudi
Student Number: ST10493664
Module Code: CLDV6211
Group: 4

## Acknowledgments
EventEase for the project requirements
ASP.NET Core documentation
Bootstrap for the UI framework
Font Awesome for the icons

## Contact
For questions or support, please contact:
Email: rtfafudi@gmail.com
GitHub Issues: Create an issue

## Future Enhancements (Part 2 & 3)
- Image Upload: Replace placeholder URLs with actual image uploads using Azurite
- Search Functionality: Search venues, events, and bookings
- Advanced Filtering: Filter by event type, venue, date range
- Cloud Migration: Deploy to Azure App Service
- Azure SQL Database: Migrate from LocalDB to Azure SQL
- Azure Blob Storage: Store images in the cloud

Version: 1.0.0
Status: ✅ Part 1 Complete - Project Foundation
Last Updated: March 30, 2026
