# Ejecutar desde la raíz del repositorio. Las migraciones ya están incluidas.
$ErrorActionPreference = 'Stop'
dotnet ef database update --project src/PetCare.Infrastructure --startup-project src/PetCare.Web --context PetCareDbContext
if ($LASTEXITCODE -ne 0) { throw 'No se pudo aplicar la migración.' }
