In order to run the EF migrations we need to in always in sync

*** Command to install the dotnet tool for Entity framework core ***

dotnet tool install -g dotnet-ef

**** Command to Run the created the Migrations ****

dotnet ef migrations add AddInitialMigration --startup-project EPR.Calculator.API --project EPR.Calculator.API.Data

**** Command to add a migration *****

dotnet ef migrations add AddBlogCreatedTimestamp

**** Command to remove latest Migration *****

dotnet ef migrations remove --verbose --project "EPR.Calculator.API.Data" --startup-project "EPR.Calculator.API" 

**** Listing Migrations *****

dotnet ef migrations list

**** Checking for Model changes *****

dotnet ef migrations has-pending-model-changes

**** Creating Migrations for Sql ******
Should be run in the Data project

dotnet ef migrations script -o  ./Scripts/migrations.sql -i

***** Command to Update the migrations on the database *****

dotnet ef database update --verbose --project "EPR.Calculator.API.Data" --startup-project "EPR.Calculator.API"