# Carl Watchtower

Carl Watchtower (or carl-wt), is a Planetside 2 tracker that shows the top players in each faction certain categories. You can also view a breakdown of their actions, such as seeing what weapons a player is using, who a medic is reviving, or what types of spawns someone is providing

## Other realtime tracking webites

- [fisu](https://ps2.fisu.pw/activity/)
- [voidwell](https://voidwell.com/ps2/worlds)
- [squittal.LivePlanetmans](https://github.com/eating-coleslaw/squittal.LivePlanetmans)
- if you have one i'd be happy to add it!

## General Info

carl-wt is a .NET Core 5 server that runs on Windows 10 and Linux

### Technologies used

Tech | Use
--- | ---
.NET Core 5 | backbone for everything
ASP.NET Core MVC | routing
SignalR | WebSocket connections for realtime updates
VueJS | frontend framework
Typescript | all frontend logic
DaybreakGames.Census | C# package by Lampjaw
PostgreSQL | database

## Setup

1. download Carl Watchtower
    - `git clone https://github.com/varunda/carl-wt.git`
1. download and install PostgreSQL 13.3 or higher
    - earlier versions probably work but I haven't tested them
1. setup a `ps2` database for carl-wt to create all the tables for
    - Linux:
        - log into user that has permissions in the default psql server 
            - `> sudo -iu postgres` 
        - start a psql client
            - `> psql`
        - create ps2 database
            - `postgres=# CREATE DATABASE ps2;`
        - ensure it exists
            - `postgres=# \c ps2;`
        - done!
    - Windows:
        - use pgAdmin or something I'm not sure exactly
1. compile the frontend code
    - install the node modules
        - `npm install`
    - compile the typescript into Javascript
        - `npm run watch` (you'll need to like ^C after or something)
    - done!
1. build the backend server
    - `dotnet build`
1. run carl-wt
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

carl-wt is really a collection of programs that run under the same process. Each program, or hosted service, handles a different part of creating the world data. Some hosted services run once then return, others act as a queue processor, while others run period taks. Some services can be interacted with by inputting commands to the server

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

carl-wt can be interacted with in the console by typing commands. It is a bit jank currently. If text is output while you're typing a command, the input text will be split. There is no command history or auto-complete.

Commands are processed in `/Code/Commands`, and can aid in running carl-wt. To list all commands available, use `.list`. To safely close carl-wt, use `.close`. Closing carl-wt thru SIGTERM can be done (such as ^C), but can prevent some non-essential cleanup processes from running.

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