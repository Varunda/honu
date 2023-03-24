# Honu

[Honu](https://wt.honu.pw) is a Planetside 2 website that shows the top players in each faction certain categories. You can also view a breakdown of their actions, such as seeing what weapons a player is using, who a medic is reviving, or what types of spawns someone is providing

## Other realtime tracking webites

- [fisu](https://ps2.fisu.pw/activity/)
- [voidwell](https://voidwell.com/ps2/worlds)
- [squittal.LivePlanetmans](https://github.com/eating-coleslaw/squittal.LivePlanetmans)
- if you have one i'd be happy to add it!

## General Info

honu is a .NET 6 server that runs on Windows 10 and Linux

### Technologies used

Tech | Use
--- | ---
.NET 6 | backbone for everything
ASP.NET Core MVC | routing
SignalR | WebSocket connections for realtime updates
VueJS | frontend framework
Typescript | all frontend logic
DaybreakGames.Census | C# package by Lampjaw
PostgreSQL | database

## Setup

1. download honu
    - `git clone https://github.com/varunda/honu.git`
1. download and install PostgreSQL 13.3 or higher
    - earlier versions probably work but I haven't tested them
1. setup a `ps2` database for honu to create all the tables for
    - Linux:
        - log into user that has permissions in the default psql server 
            - `$ sudo -iu postgres` 
        - start a psql client
            - `$ psql`
        - create ps2 database
            - `postgres=# CREATE DATABASE ps2;`
        - ensure it exists
            - `postgres=# \c ps2;`
        - done!
    - Windows:
        - use pgAdmin or something I'm not sure exactly
1. change `syncronous_commit` to `off` instead of the default `on`.
    - https://www.postgresql.org/docs/current/wal-async-commit.html
    - Find your `postgresql.conf`, then to reload call `SELECT pg_reload_conf();`
1. compile the frontend code
    - install the node modules
        - `npm install`
    - compile the typescript into Javascript
        - `npm run watch` (you'll need to like ^C after or something)
    - done!
1. build the backend server
    - `dotnet build`
1. run honu
    - `dotnet run`

There is (ideally) no configuration beyond this needed. All database tables will be created automatically as part of the startup process. A default service ID of `asdf` has been provided, you can change this in `Startup.cs`, in the function `services.AddCensusServices()`

If you want to change database options, you can either add a new object in `appsettings.json` called `DbOptions`, or add user secrets

### DbOptions in user secrets

1. initalize the user secrets
    - `dotnet user-secrets init`
1. set the options you want to change
    - `dotnet user-secrets set "$OptionName" "$Value"`

`$OptionName` can be:
- `DbOptions.ServerUrl`: Host of the server, localhost is the default which means running on the same machine
- `DbOptions.DatabaseName`: Name of the database, default is `ps2`, which is what was set in the database setup steps
- `DbOptions.Username`: Username used to connect to the database
- `DbOptions.Password`: Password of the user used to connect to the database

## Running

honu is really a collection of programs that run under the same process. Each program, or hosted service, handles a different part of creating the world data. Some hosted services run once then return, others act as a queue processor, while others run period taks. Some services can be interacted with by inputting commands to the server

### Services

Hosted service | Lifetime | Purpose
--- | --- | ---
DataBuilderService.cs | Task | Runs all the queries to build the world data for each world/server
DbCreatorHostedService.cs | Once | Run once at startup to create all the db stuff
EventCleanupService.cs | Task | Removes old data that no longer needs to be tracked
EventProcessService.cs | Queue | Processes the queue of realtime events
HostedBackgroundCharacterCacheQueue.cs | Queue | Caches characters in the background
RealtimeResubscribeService.cs | Task | Periodically resubscribes to all realtime events
WorldDataBroadcastService.cs | Task | Broadcasts the world data to all signalR clients

Lifetime | Description
--- | ---
Once | Ran once either at startup or shutdown
Task | Periodically runs a task
Queue | Continiously processes a list of things to do

### Commands

honu can be interacted with in the console by typing commands. It is a bit jank currently. If text is output while you're typing a command, the input text will be split. There is no command history or auto-complete.

Commands are processed in `/Code/Commands`, and can aid in running honu. To list all commands available, use `.list`. To safely close honu, use `.close`. Closing honu thru SIGTERM can be done (such as ^C), but can prevent some non-essential cleanup processes from running (such as ending current sessions).

#### Common commands

resubscribe to the realtime events:
- `realtime restart`

print all hosted service:
- `service print`

disable a hosted service:
- `service disable $ServiceName`
- useful when testing on a 5 year old laptop that can't really handle the db queries 
- not all hosted services actually stop when disabled

enable a hosted service:
- `service enable $ServiceName`

### Tracing

honu uses OpenTelemetry to do profiling with a Jaeger exporter. (Not Planetside 2 Jaeger)

to run the tracing, run the command:

`docker compose -f jaeger-docker-compose.yml up`

if these Docker services aren't running, profiling will not take place

### Report Generator

honu uses a specific string format to specify the parameters for an outfit report.

a generator string can be two different forms:
- ID form: only the id of a previously used generator string is given. A db lookup is then performed to load the generator string, which is in options form
- Options form: the options are provided in this form

#### ID form

`#{GUID};`
- GUID: GUID - A valid globally unique identifer

#### Options form

`{START},{END},{TEAM_ID};{ENTITIES};`

- START: int - the unix epoch (in seconds) for when the report starts. cannot come after END
- END: int - the unix epoch (in seconds) for when the report ends. cannot come before START
- TEAM_ID: short - an optional short to filter the team_id of events. used for NSO outfits, or outfits with NSO. this is optional as often a valid team_id can be infered from the outfits are characters being added
- ENTITIES: many - there are many different entities that can be given, some change what characters are included in an outfit, some include extra information

##### ENTITIES

the following are valid entities to use within an options form generator string. each entity must end with `;`.

- `o{OUTFIT_ID};`: ID of an outfit to include in the report. note sessions are saved with the outfit_id a character is currently in, and if someone leaves an outfit in the future, it will not change previous reports
    - Example: `o37588782541444244;`: Include the outfit with ID `37588782541444244` (honu, the outfit)
- `+{CHARACTER_ID};`: ID of a character to include in the report
    - Example: `+5429109374092411329;`: Include the character with ID `5429109374092411329` (honu, the character)
- `-{CHARACTER_ID};`: ID of a character to exclude from a report. useful in large outfits where not everyone was participating
- `${OPTION}={VALUE};`: Extra options that change what data is included in a report. Valid options are:
    - `$rd=[1|0]`: Include revived deaths or not
    - `$itk=[1|0]`: Include team kills
    - `$iae=[1|0]`: Include achievements earned

#### Examples

Examples of valid generator strings

`#91ee7652-6ede-4e52-b32f-3646b14e8887;`: use the db saved generator string with the ID of `91ee7652-6ede-4e52-b32f-3646b14e8887`

`1640055600,1640062800;o37567362753122235;+5428011263335537297;-5428345446430485649;`: 
- start the report at `1640055600` (`2021-12-21T03:00:00.000Z`), and go till `1640062800` (`2021-12-21T05:00:00.000Z`)
- include all characters in the outfit `37567362753122235` ([T1DE] Tide)
- include the character `5428011263335537297` (Wrel)
- exclude the character `5428345446430485649` (varunda)t
