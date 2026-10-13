# EventEase Azure deployment

This project is an ASP.NET Core Razor Pages application targeting .NET 10. It uses Entity Framework Core migrations and the `DefaultConnection` connection-string key.

## 1. Prerequisites

Install and sign in to the Azure CLI, then select the correct subscription:

```powershell
az login
az account set --subscription "<subscription-id-or-name>"
```

Use globally unique, lower-case names for the resource group, SQL server and App Service.

## 2. Create Azure resources

Run these commands in PowerShell after replacing the placeholders. Do not commit the SQL administrator password.

```powershell
$location = "South Africa North"
$resourceGroup = "rg-eventease"
$sqlServer = "eventease-sql-<unique-suffix>"
$sqlDatabase = "EventEaseDB"
$appName = "eventease-app-<unique-suffix>"
$sqlAdmin = "eventeaseadmin"
$sqlPassword = Read-Host "SQL administrator password" -AsSecureString
$sqlPasswordText = [System.Net.NetworkCredential]::new('', $sqlPassword).Password

az group create --name $resourceGroup --location $location
az sql server create --name $sqlServer --resource-group $resourceGroup --location $location --admin-user $sqlAdmin --admin-password $sqlPasswordText
az sql db create --resource-group $resourceGroup --server $sqlServer --name $sqlDatabase --service-objective S0
az sql server firewall-rule create --resource-group $resourceGroup --server $sqlServer --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
az appservice plan create --name "$appName-plan" --resource-group $resourceGroup --location $location --is-linux --sku B1
az webapp create --resource-group $resourceGroup --plan "$appName-plan" --name $appName --runtime "DOTNETCORE:10.0"
```

If the selected App Service region does not offer the requested .NET runtime, create the app with the runtime shown by:

```powershell
az webapp list-runtimes --os-type linux
```

## 3. Configure the Azure SQL connection

```powershell
$connectionString = "Server=tcp:$sqlServer.database.windows.net,1433;Initial Catalog=$sqlDatabase;Persist Security Info=False;User ID=$sqlAdmin;Password=$sqlPasswordText;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set --resource-group $resourceGroup --name $appName --settings DefaultConnection="$connectionString" --connection-string-type SQLAzure
az webapp config set --resource-group $resourceGroup --name $appName --always-on true --https-only true
```

The application reads this value through `GetConnectionString("DefaultConnection")`. Never place this production value in `appsettings.json` or commit it to source control.

## 4. Create the database schema

The repository contains `Data/Migrations/InitialCreate`. The application calls `Database.Migrate()` during startup, so the migration is applied when the deployed application starts.

For a controlled release, the migration can also be applied from a secure deployment environment:

```powershell
dotnet ef database update --project EventEase.csproj --connection "$connectionString"
```

## 5. Publish and deploy

From the `EventEase` project directory:

```powershell
dotnet publish EventEase.csproj -c Release -o ./publish
Compress-Archive -Path ./publish/* -DestinationPath ./eventease.zip -Force
az webapp deploy --resource-group $resourceGroup --name $appName --src-path ./eventease.zip --type zip
```

Open the deployed application:

```powershell
az webapp browse --resource-group $resourceGroup --name $appName
```

## 6. Automatic deployment from GitHub

The workflow at `.github/workflows/deploy-eventease.yml` builds, tests, publishes and deploys the application to the `EventEase1` App Service whenever a commit is pushed to `main`.

### Rotate exposed publishing credentials

If a publish profile has been shared in chat, email, logs or source control, treat it as compromised. In the Azure portal, open the `EventEase1` App Service and reset its publishing credentials, then download a new publish profile. Never commit the profile to the repository.

### Configure the GitHub secret

1. In the GitHub repository, open **Settings > Secrets and variables > Actions > Secrets**.
2. Create a repository secret named `AZURE_WEBAPP_PUBLISH_PROFILE`.
3. Paste the complete XML from the newly downloaded `EventEase1` publish profile into the secret value.

After the secret exists, push to `main` or manually run **Build and deploy EventEase** from the repository's Actions tab. Migrating back to OIDC is recommended because it avoids a long-lived publishing password.

## 7. Deployment evidence for the assessment

Capture screenshots of:

- Resource group and deployed App Service
- Azure SQL Database and server
- App Service connection string configuration (hide the password)
- Successful deployment output
- The public EventEase URL
- Tables and sample data in Azure SQL
- Events, venues and bookings working from the deployed application

## 8. Troubleshooting

View application logs with:

```powershell
az webapp log config --resource-group $resourceGroup --name $appName --docker-container-logging filesystem
az webapp log tail --resource-group $resourceGroup --name $appName
```

If the application cannot connect to Azure SQL, check the connection-string name, SQL firewall rules, server/database names and that the App Service has restarted after configuration changes.
