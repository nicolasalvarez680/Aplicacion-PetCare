# Ejecutar desde la raíz del repositorio
dotnet ef migrations add InitialCreate --project src/PetCare.Infrastructure --startup-project src/PetCare.Web --context PetCareDbContext --output-dir Migrations
dotnet ef database update --project src/PetCare.Infrastructure --startup-project src/PetCare.Web --context PetCareDbContext
