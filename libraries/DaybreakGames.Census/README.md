# DaybreakGames.Census

[![NuGet](https://img.shields.io/nuget/v/DaybreakGames.Census.svg)](https://www.nuget.org/packages/DaybreakGames.Census/)

**This package is unofficial and is NOT in any way supported by Daybreak Games.**

All use of game data and other content must comply with [Daybreak Games Intellectual Property policy](http://census.daybreakgames.com/#intellectual-property)
and Daybreak Game Company's [Terms of Service](https://www.daybreakgames.com/termsofservice.vm?locale=en_US).

---

DaybreakGames.Census is a library that makes it much easier to use the Census API provided by Daybreak Games Company.

For a full demonstration check out the [DemoApp](https://github.com/Lampjaw/DaybreakGames.Census/tree/master/src/DemoApp)!

## Table of Contents

- [Installing](#installing)
- [Creating a new query](#creating-a-new-query)
- [Returning data from the query](#returning-data-from-the-query)
- [Setting global parameters](#setting-global-parameters)
- [Defining a condition](#defining-a-condition)
- [Setting a language](#setting-a-language)
- [Show certain fields](#show-certain-fields)
- [Hide certain fields](#hide-certain-fields)
- [Set a limit for number of rows to return](#set-a-limit-for-number-of-rows-to-return)
- [Set the starting row](#set-the-starting-row)
- [Add a resolve](#add-a-resolve)
- [Join to another service](#join-to-another-service)
- [Tree results on a field](#tree-results-on-a-field)
- [Getting the url of the query](#getting-the-url-of-the-query)
- [Streaming data](#streaming-data)

### Installing

DaybreakGames.Census is available via [NuGet](https://www.nuget.org/packages/DaybreakGames.Census/).

To inject DaybreakGames.Census services into your project add `services.AddCensusServices();`
to your ConfigureServices block.

### Creating a new query

To start creating queries, use `ICensusQueryFactory` via DI and call
`ICensusQueryFactory.Create(<serviceName>)` to get a `CensusQuery` object.

```C#
public MyClass(ICensusQueryFactory censusFactory)
{
    var query = censusFactory.Create("character");
    query.SetServiceId("my-service-id")
        .SetServiceNamespace("ps2");
}
```

`SetServiceId` and `SetServiceNamespace` are optional. If not set than the values
provided in `AddCensusServices()` are used. Otherwise it defaults to `ServiceId="example"`
and `ServiceNamespace="ps2:v2"`.

### Returning data from the query

There are three methods of query resolution: `GetAsync<T>()`, `GetListAsync<T>()`, and
`GetBatchAsync<T>()`. A model can be passed in that deserializes to the expected census model
that matches Pascal Case versions of the census properties (i.e `character_id` becomes
`CharacterId`). For other cases, the `[JsonProperty]` decorators should be respected.

If a model is not given than the default return type is as a JToken.

```C#
var resultJToken = await query.GetAsync();
```

### Setting global parameters

If your application is always using the same service id and namespace you might as well set it
globally so you don't have to configure it with every query.

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddCensusServices(options =>
            {
                options.CensusServiceId = "my-service-id";
                options.CensusServiceNamespace = "ps2";
            });
}
```

### Defining a condition

```C#
query.Where("name.lower").Equals("lampjaw");
```

The following operations and their equivalent syntax is below:

* `Equals`: =
* `NotEquals`: =!
* `IsLessThan`: =<
* `IsLessThanOrEquals`: =[
* `IsGreaterThan`: =>
* `IsGreaterThanOrEquals`: =]
* `StartsWith`: =^
* `Contains`: =*

### Setting a language

```C#
query.SetLanguage("en");

OR

query.SetLanguage(CensusLanguage.English);
```

No language is set by default so you will receive all localized strings if available.

### Show certain fields

```C#
query.ShowFields("character_id"[, "field2", "field3", ...]);
```

### Hide certain fields

```C#
query.HideFields("currency"[, "field2", "field3", ...]);
```

### Set a limit for number of rows to return

```C#
query.SetLimit(10);
```

### Set the starting row

```C#
query.SetStart(100);
```

### Add a resolve

```C#
query.AddResolve("world"[, "resolve2", "resolve3", ...]);
```

### Join to another service

```C#
var worldJoin = query.JoinService("characters_world");
```

Join objects have the following methods:

* `IsList(bool)`
* `IsOuterJoin(bool)`
* `ShowFields(array)`: See the 'Show certain fields' section above
* `HideFields(array)`: See the 'Hide certain fields' section above
* `OnField(string)`
* `ToField(string)`
* `InjectAt(string)`
* `Where(string)`: See the 'Defining a condition' section above
* `JoinService(string)`: Returns another join object for sub joining

### Tree results on a field

```C#
var query = queryFactory.Create("vehicle");
var vehicleTree = query.TreeField("type_id");
```

Tree objects have the following methods:

* `IsList(bool)`
* `GroupPrefix(string)`
* `StartField(string)`
* `TreeField(string)`: Returns another tree object for sub grouping

### Getting the url of the query

```C#
Uri uri = query.GetUri();
```

### Streaming data

This package also allows you to leverage the Census' websocket service to get data
in real time. Inject `ICensusStreamClient` and subcribe to events.
See the [WebsocketMonitor class in the DemoApp](https://github.com/Lampjaw/DaybreakGames.Census/blob/master/src/DemoApp/WebsocketMonitor.cs)
for a full example.