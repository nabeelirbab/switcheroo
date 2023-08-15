# Switcheroo Server

- [Switcheroo Server](#switcheroo-server)
  - [Install](#install)
    - [Download .NET Core](#download-dotnetcore)
    - [Download Docker](#download-docker)
    - [Run Some Commands](#run-some-commands)
  - [Running](#running)
  - [Project Structure](#project-structure)

## Install

### Download dotnetcore

https://dotnet.microsoft.com/download

### Download Docker

https://www.docker.com/

### Run some commands
- `docker volume create --name=roodbvol` - Do this once. It will create you a volume to put your deebz
- `dotnet tool install --global dotnet-ef` - Do this once. It will allow you to do migrations / update you deebz
- `dotnet restore` - Do this whenever there are new deps (generally when you've pulled from master). This will pull in deps for the server project

**NOTES**
You will need a .env file with the following

```dot
SMTP_HOST=localhost
SMTP_PORT=2525
SMTP_FROM_ADDRESS=dev@switcherooapp.com.au
SMTP_UI_PORT=3000

POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_USER=local
POSTGRES_PASSWORD=pass123
POSTGRES_DATABASE=roodb

ADMINER_PORT=5433
```

NOTE: still working out a nice way to use the .env file (env $(cat ../.env | grep -v "#" | xargs)) - yuck! This just spreads the .env file into command args to the dotnet process
I'm assuming this wont support Mawcraserft Wandows

## Running

- Use VSCODE and smash an F5
- Use Visual Studio Mac and smash an F5

## Databasing

###Tasks
If using VSCode I have set up tasks to run/apply migrations. Check the tasks.json file if you want to know more.

###Manual commands (OSX)
In order to create a database migration, navigate to the Infrastructure project and run the following command
- env $(cat ../.env | grep -v "#" | xargs) dotnet ef --startup-project '../API/API.csproj' migrations add InitialCreate --output-dir './Database/Migrations'

In order to apply a database migration, navigate to the Infrastructure project and run the following command
- env $(cat ../.env | grep -v "#" | xargs) dotnet ef --startup-project '../API/API.csproj' database update

## Project Structure

- API: This is our application. Hot choc and graph ql made available for the world
- Domain: This is where we do our domain modeling and data/business rule validation
- Infrastructure: This is where we talk to our infrastructure

Rules! 
- Everyone can ref Domain. 
- API can only ref infra for DI purposes
- We use C# nullable ref types - take care with this. The database schema may be fiddly
  - https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/nullable-reference-types
  - https://docs.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

Question?
- Is there a better place for interfaces to live? Maybe in domain? But then surely you have coupled *HOW* we do things with the domain?


# Deployment to staging
-- INTIAL DEPLOYMENT --
- 1. Download heroku cli - https://devcenter.heroku.com/articles/heroku-cli
- 2. `heroku login` and follow prompts
- 3. Install pg tools so we can run sql against the server - https://devcenter.heroku.com/articles/heroku-postgresql#set-up-postgres-on-windows
-- You can continue from below after the above have been run --
- 3. `heroku container:login`
- 4. `heroku container:push web --app switchy-testing`
- 5. `heroku pg:reset --app switchy-testing --confirm switchy-testing`
- 6. ENSURE YOU ARE NOT RUNNING THE PROJECT AS IT NEEDS TO BUILD `dotnet ef --project API migrations script -o out/migration.sql`
- 7. `heroku pg:psql --app switchy-testing < out/migration.sql`
- 8. `heroku container:release web --app switchy-testing`# switcheroo
