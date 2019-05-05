# OneData
[![Powered by C#](https://img.icons8.com/color/48/000000/c-sharp-logo-2.png)](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials)
[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://github.com/jorgemk85/OneData)

So, you are looking for a VERY easy solution to access your data inside a MySQL or MsSQL server...

## Features
* Very fast!
* Ease of relational data access.
* Automatic class/model binding.
* On-RAM per class/model cache.
* Automatic class/model structure sync inside your database.
* Automatic logging in disk and database of every transaction (optional)
* Supports simultaneous database connections.
* Use your default database connection or switch with ease to use a diferent one.
* Very easy query pagination on database.
* Supports Async.
* Events for every transaction type (Insert, Select, Update, Delete, etc).
* Per class/model configuration.
* Get foreign data into your "local" desired property with ease.

... And much more!

Don't know SQL? Why should you? OneData got you covered ;)

## Steps
Now that we got all the intro out of our way, let's stablish what tasks must be done to get your project up and running with OneData:

1) Install the package.
2) Configure your project .config file.
3) Setup your classes.

DONE!

## Installation

Download the library from [GitHub](https://github.com/jorgemk85/OneData/). 

OR

Install it via [NuGet](https://www.nuget.org/packages/OneData/).

#### Package Manager
```
Install-Package OneData
```

#### .NET CLI
```
dotnet add package OneData
```

## Configuration

Now that you have the library installed in your project, you need to set up your .config configuration file (if you are using .Net Framework) or your .json configuration file (if you are using .Net Core or .Net Standard). [What version should I use?](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

### .Net Framework 4.6.1 and Later
In case you don't have a ConnectionString section inside your configuration file, please add it (don't include the placeholders <>):
```xml
  <connectionStrings>
    <add name="<your connection name>" connectionString="server=<your server ip / hostname>;Uid=<db username>;Pwd=<db password>;persistsecurityinfo=True;database=<your database name>;SslMode=none;AllowUserVariables=True;CheckParameters=False" />
  </connectionStrings>
```
Notice that there are some special settings in the connection string. Make you you include them in EVERY connection string you got.
```
SslMode=none;AllowUserVariables=True;CheckParameters=False
```
The following is a complete list of the configurations available (don't include the placeholders <>).

Please add them ALL to your project:
```xml
  <appSettings>
    <add key="DefaultConnection" value="<your default connection name>" />
    <add key="ConnectionType" value="<MySQL or MSSQL>" />
    <add key="InsertSuffix" value="<the suffix to use in the INSERT SPs>" />
    <add key="UpdateSuffix" value="<the suffix to use in the UPDATE SPs>" />
    <add key="DeleteSuffix" value="<the suffix to use in the DELETE SPs>" />
    <add key="StoredProcedurePrefix" value="<the prefix for all the SPs>" />
    <add key="TablePrefix" value="<the prefix for all the tables>" />
    <add key="AutoCreateTables" value="<true or false>" />
    <add key="AutoCreateStoredProcedures" value="<true or false>" />
    <add key="AutoAlterStoredProcedures" value="<true or false>" />
    <add key="AutoAlterTables" value="<true or false>" />
    <add key="EnableLogInDatabase" value="<true or false>" />
    <add key="EnableLogInFile" value="<true or false>" />
    <add key="DefaultSchema" value="<name of your default schema>" />
    <add key="ConstantTableConsolidation" value="<true or false>" />
    <add key="OverrideOnlyInDebug" value="<true or false>" />
  </appSettings>
```
### .Net Standard 2.0 and Later / .NET Core 2.0 and Later
In case you don't have a ConnectionString section inside your configuration file, please add it (don't include the placeholders <>):
```json
  "ConnectionStrings": {
      "<your connection name>": "server=<your server ip / hostname>;Uid=<db username>;Pwd=<db password>$;persistsecurityinfo=True;database=<your database name>;SslMode=none;AllowUserVariables=True;CheckParameters=False"
    }
```
Notice that there are some special settings in the connection string. Make you you include them in EVERY connection string you got.
```
SslMode=none;AllowUserVariables=True;CheckParameters=False
```
The following is a complete list of the configurations available (don't include the placeholders <>).

Please add them ALL to your project:
```json
  "AppSettings": {
    "DefaultConnection": "<your default connection name as stated in ConnectionString>",
    "ConnectionType": "<MySQL or MSSQL>",
    "InsertSuffix": "<the suffix to use in the UPDATE SPs>",
    "UpdateSuffix": "<the suffix to use in the DELETE SPs>",
    "DeleteSuffix": "<the suffix to use in the DELETE SPs>",
    "StoredProcedurePrefix": "<the prefix for all the SPs>",
    "TablePrefix": "<the prefix for all the tables>",
    "AutoCreateTables": "<true or false>",
    "AutoCreateStoredProcedures": "<true or false>",
    "AutoAlterStoredProcedures": "<true or false>",
    "AutoAlterTables": "<true or false>",
    "EnableLogInDatabase": "<true or false>",
    "EnableLogInFile": "<true or false>",
    "DefaultSchema": "<name of your default schema>",
    "ConstantTableConsolidation": "<true or false>",
    "OverrideOnlyInDebug": "<true or false>"
  }
```
In case you don't have a ConnectionString section inside your configuration file, please add it:
```xml
  <connectionStrings>
    <add name="MochaHost" connectionString="server=<your server ip / hostname>;Uid=<db username>;Pwd=<db password>;persistsecurityinfo=True;database=<your database name>;SslMode=none;AllowUserVariables=True;CheckParameters=False" />
  </connectionStrings>
```

Ok, that wasn't hard, isn't it? We are done with the configuration!
## Setup

The last step is to setup your classes!

But... Which one's should I setup? Well, every class you will need to connect / have-access-to in you database.

Here's an example with the minimum required setup for the library to understand your class:
```c#
using OneData.Attributes;
using OneData.Interfaces;

[DataTable("logs")]
    public class Log : Cope<Log>, IManageable
    {
        [PrimaryKeyProperty]
        public Guid Id { get; set; }
        [DateCreatedProperty]
        public DateTime DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime DateModified { get; set; }
    }
```

The attributes DataTable, PrimaryKeyProperty, DateCreatedProperty and DateModifiedProperty will be explained with detail later in this document.

Please note the Generic class Cope<T> which NEEDS sent the class you are working on. As the example shows, Log is the class name and the generic class is Cope<Log>. This is very important to setup properly since the compiler MIGHT not show a compilation error (it varies from class to class).
  
Well done! You now have an up and running a complete relational data management solution inside your project.
## Usage
Let's begin with a list of available attributes:
| Name | Type | Description |
| ------ | ------ |------ |

  
Now
```c#
import foobar

foobar.pluralize('word') # returns 'words'
foobar.pluralize('goose') # returns 'geese'
foobar.singularize('phenomena') # returns 'phenomenon'
```

## FAQ

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

## Icons used
[Icons by Icons8](https://icons8.com)
