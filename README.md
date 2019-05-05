# OneData
[![OneData](https://img.icons8.com/cotton/64/000000/cloud-database.png)](https://github.com/jorgemk85/OneData)
[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://github.com/jorgemk85/OneData)

So, you are looking for a VERY easy, Code-First solution to access your data inside a MySQL or MsSQL server...

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
* Strongly typed.
* Supports Async.
* Events for every transaction type (Insert, Select, Update, Delete, etc).
* Per class/model configuration.
* Get foreign data into your desired "local" property with ease.
* Massive operations are supported! You can insert, update or delete in your database with ease.
* Call any stored procedure of your liking with ease.

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

*Can't find it with NuGet? Make sure you enable "Include prerelease" checkbox inside your NuGet Package Explorer.*

## Configuration

Now that you have the library installed in your project, you need to set up your .config configuration file (if you are using .Net Framework) or your .json configuration file (if you are using .Net Core or .Net Standard). [Should I use .Net Standard?](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

### .Net Framework 4.6.1 and Later
In case you don't have a ConnectionString section inside your configuration file, please add it (don't include the placeholders <>):
```xml
  <connectionStrings>
    <add name="<your connection name>" connectionString="server=<your server ip / hostname>;Uid=<db username>;Pwd=<db password>;persistsecurityinfo=True;database=<your database name>;SslMode=none;AllowUserVariables=True;CheckParameters=False" />
  </connectionStrings>
```
Notice that there are some special settings in the connection string. Make you include them in EVERY connection string you got.
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
Notice that there are some special settings in the connection string. Make you include them in EVERY connection string you got.
```
SslMode=none;AllowUserVariables=True;CheckParameters=False
```
The following is a complete list of the configurations available (don't include the placeholders <>).

Please add them ALL to your project:
```json
  "AppSettings": {
    "DefaultConnection": "<your default connection name as stated in ConnectionString>",
    "ConnectionType": "<MySQL or MSSQL>",
    "InsertSuffix": "<the suffix to use in the INSERT SPs>",
    "UpdateSuffix": "<the suffix to use in the UPDATE SPs>",
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

But... Which one's should I setup? Well, every class you will need to connect / have-access-to in your database.

Here's an example with the minimum required setup for the library to understand your class:
```c#
using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;

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
### Configuration:
We have already explained where to put this configurations but haven't detailed what they are.

The following table is a comprehensive list of available configurations with their respectic information:

| Configuration name          | Remarks									| Description                    |
|-----------------------------|-----------------------------------------|--------------------------------|
|`DefaultConnection`          |None.                                    |Type the name of your default connection.|
|`ConnectionType`             |MySQL or MsSQL                           |Choose to configure OneData for MySQL or MsSQL.|
|`InsertSuffix`               |Can be Blank.                            |Literally the suffix to use with the Insert SPs.|
|`UpdateSuffix`               |Can be Blank.                            |Literally the suffix to use with the Update SPs.|
|`DeleteSuffix`               |Can be Blank.                            |Literally the suffix to use with the Delete SPs.|
|`StoredProcedurePrefix`      |Can be Blank.                            |Prefix for every SP.|
|`AutoCreateTables`           |true or false                            |If true, OneData will create a required table inside the database if it doesn't exist.|
|`AutoCreateStoredProcedures` |true or false                            |If true, OneData will create a required stored procedure if it doesn't exist.|
|`AutoAlterStoredProcedures`  |true or false                            |If true, OneData will alter a required stored procedure if it's not in sync with the class/model.|
|`AutoAlterTables`            |true or false                            |If true, OneData will alter a required table if it's not in sync with the class/model.|
|`EnableLogInDatabase`        |true or false                            |Choose to enable logging inside the database.|
|`DefaultSchema`              |For MySQL it's the database name.        |Type the name of your default schema/database.|
|`ConstantTableConsolidation` |true or false. Runs only in Debug.       |Heuristic approach to sync everything in your database based on your classes/models. Caution, it's a bit slow and is not recomended for production. Runs only on Debug mode.|
|`OverrideOnlyInDebug`        |Override settings that run only in debug.|Will override those settings set to run only on Debug mode.|

### Attributes:
Attributes in OneData are used to configure the classes/models and properties.

The following table is a comprehensive list of available attributes with their respectic information:

| Attribute name        | Used with  | Remarks                            | Description                    |
|-----------------------|------------|------------------------------------|--------------------------------|
| `AutoProperty`        | Properties | None.                              | Data is completely managed by OneData based on your settings.   |
| `CacheEnabled`        |  Classes   | Once per Class/Model.              | Enables a class/model to use the On-RAM Cache. Uses minutes as expiration.|
| `DataLength`          | Properties | None.                              | Specify which data length you want to use. If not implemented, will use default.|
| `DataTable`           |  Classes   | Required. Once per Class/Model.    | Sets the table name (and optinally the scheme) to use.|
| `DateCreatedProperty` | Properties | Required. Once per Class/Model.    | Mark the property that will hold date and time of record creation.|
| `DateModifiedProperty`| Properties | Required. Once per Class/Model.    | Mark the property that will hold date and time of record update.   |
| `ForeignData`         | Properties | None.                              | Used when you need to get information from a foreign table.   |
| `ForeignKey`          | Properties | None.                              | Relates a property with the PrimaryKey of another class/model.   |
| `HeaderName`          | Properties | None.                              | Specify the name to look for instead of the propery name.   |
| `PrimaryKeyProperty`  | Properties | Required. Once per Class/Model.    | Mark the property that will be set as the PrimaryKey. |
| `UniqueKey`           | Properties | None.                              | Set a property to hold a unique value.   |
| `UnmanagedProperty`   | Properties | None.                              | Used when you don't want OneData to interfere with.   |

### Transactions
Let's talk a bit about transactions... First of all, we've got an enumeration called TransactionTypes which holds the following:
* Select
* SelectAll
* Delete
* DeleteMassive
* Insert
* InsertMassive
* Update
* UpdateMassive
* StoredProcedure

They are pretty self explanatory, except maybe DeleteMassive, InsertMassive, UpdateMassive and StoredProcedure. The first three types are used to execute the desired transaction but with a big set of data. The last one is used when you want to execute a generic stored procedure inside your database.

## Examples

### Basics
#### Changing my class/model
In section **Setup**, we stablished a class/model called Log, which only have three properties. If we run our program, OneData will be creating the respective table called "logs" (as configured with the DataTable attribute). This table will also have three columns which reflects our class/model... But, what if I add a new property? Should I go into the table manually and change it as I please? NO! You just need to add this new property to your class/model and let OneData take care of the rest (make sure you have the corresponding settings inside your .config file)! Please see the following:

This is our new Log class/model (note the new properties UserId and Transaction).
```c#
[DataTable("logs")]
public class Log : Cope<Log>, IManageable
{
    [PrimaryKeyProperty]
    public Guid Id { get; set; }
    [DateCreatedProperty]
    public DateTime DateCreated { get; set; }
    [DateModifiedProperty]
    public DateTime DateModified { get; set; }

    public Guid UserId { get; set; }
    public string Transaction { get; set; }
}
```
Literally, the next time your program runs and tries to access this object in the database, OneData will make the changes it detected inside your class/model without prompting anything and as transparent as it should be.

This exact steps will trigger if your change is as small as adding a new property or huge as adding twenty, changing datatypes of another three, modifying the datalength of a couple and updating the relationships between classes/models.

#### Adding data to the database
Let's say you have a single Log object with it's respective data already filled. How do I insert it into the corresponding table?
```c#
myNewLog.Insert();
```
A neat trick to ease your way when adding new objects of your desired type, is to add constructors to your class/model. This way, when ever you call ```new``` on your type, the Id will be filled automatically.

This pair is pretty handy:
```c#
public Log()
{
    Id = Guid.NewGuid();
}

public Log(Guid id)
{
    Id = id;
}
```

When you which to insert a set of information contained in a IEnumerable, say, a List, you can simply do the following:
```c#
myLogCollection.InsertMasssive();
```

OneData will then serialize your list and send it to the database for procesing, making just ONE call to insert every single one of your objects in the corresponding table. *Beware of your collection size, since even tho OneData has no cap or limit, your database or server might.*

#### Updating data in the database
Similarly to Insert, if you need to update a record, you can do the following:
```c#
myUpdatedLog.Update();
```
OneData uses the value inside the property identified as PrimaryKey to find the object in the database and update it as you wish. 

**The Update stored procedure uses IFNULL(), so if you want to send partial information, you should send your object with every property set to null except those you really need to update, and of course your PrimaryKey value should be set.**

When you which to update a set of information contained in a IEnumerable, say, a List, you can simply do the following:
```c#
myLogCollection.UpdateMasssive();
```

OneData will then serialize your list and send it to the database for procesing, making just ONE call to update every single one of your objects in the corresponding table. *Beware of your collection size, since even tho OneData has no cap or limit, your database or server might.*

#### Deleting data in the database
Similarly to Insert or Update, if you need to Delete a record, you can do the following:
```c#
myUpdatedLog.Delete();
```
You only need to send the id of your record inside the property you identified as PrimaryKey to find the object in the database and delete it.

When you which to delete a set of information contained in a IEnumerable, say, a List, you can simply do the following:
```c#
myLogCollection.DeleteMasssive();
```

OneData will then serialize your list and send it to the database for procesing, making just ONE call to delete every single one of your objects in the corresponding table. *Beware of your collection size, since even tho OneData has no cap or limit, your database or server might.*

#### Selecting data from the database
Selecting data from the database is NOT performed using stored procedures as with Inserting, Updating or Deleting. This is because of the complex nature and wide variety of queries.

OneData uses lambda expressions to work with queries, making it very readable and of course, refactor friendly along the way.

Returns a list of logs found in the database that match the provided userId.
```c#
private List<Log> GimmeAllTheLogsFromUserId(Guid userId)
{
	return Log.SelectList(q => q.UserId == userId);
}
```
Returns a list of logs found in the database that contain the provided transaction.
```c#
private List<Log> GimmeAllTheLogsThatContainTransaction(string transaction)
{
	return Log.SelectList(q => q.Transaction.Contains(transaction));
}
```
Returns a list of logs found in the database that starts with the provided transaction.
```c#
private List<Log> GimmeAllTheLogsThatStartsWithTransaction(string transaction)
{
	return Log.SelectList(q => q.Transaction.StartsWith(transaction));
}
```
Returns a list of logs found in the database that ends with the provided transaction.
```c#
private List<Log> GimmeAllTheLogsThatEndsWithTransaction(string transaction)
{
	return Log.SelectList(q => q.Transaction.EndsWith(transaction));
}
```
You can, of course, also mix your queries as needed.
```c#
private List<Log> GimmeAllTheLogsFromUserIdThatEndsWithTransaction(Guid userId, string transaction)
{
	return Log.SelectList(q => q.UserId == userId && q.Transaction.EndsWith(transaction));
}
```

If you wish to select just one object, you can do the following:
```c#
private Log GimmeTheLogFromUserIdThatEndsWithTransaction(Guid userId, string transaction)
{
	return Log.Select(q => q.UserId == userId && q.Transaction.EndsWith(transaction));
}
```
The method SelectList has an overload which accepts an object called QueryOptions, intended to further configure your query with the following options:
* ConnectionToUse
> Can be null, and if set as such, will default be automatically set to the value you stated in DefaultConnection, inside your .config file. If you set a value, make sure it's a connection name that exists inside your .config file.
* MaximumResults
> Limits the results of the query. You can set it to -1, which means to bring every record found.
* Offset
> Brings back records starting from the specified offset in this property. If set to 0, will simply start from the beginning, as expected ;)

Every query is returned with ordered records. OneData orders them by the descending value of the property marked with DateCreatedProperty attribute. 
### Intermediate
#### Relationships
We will talk a little about relationships between classes/models inside OneData.

First, let's create a new class/model called User:
```c#
using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;

[DataTable("users")]
public class User : Cope<User>, IManageable
{
    [PrimaryKeyProperty]
    public Guid Id { get; set; }
    [DateCreatedProperty]
    public DateTime DateCreated { get; set; }
    [DateModifiedProperty]
    public DateTime DateModified { get; set; }

	public string Name { get; set; }
	public string Lastname { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
}
```

To achieve a relationship between Users and Logs, we just need to configure it in the Log class/model (note the new attribute on UserId property):
```c#
[DataTable("logs")]
public class Log : Cope<Log>, IManageable
{
    [PrimaryKeyProperty]
    public Guid Id { get; set; }
    [DateCreatedProperty]
    public DateTime DateCreated { get; set; }
    [DateModifiedProperty]
    public DateTime DateModified { get; set; }

	[ForeignKey(typeof(User))]
    public Guid UserId { get; set; }
    public string Transaction { get; set; }
}
```
And just like that, our classes/models are related! By default, OneData configures the relationship ON DELETE to NO ACTION, but you can configure this with an overload on the attribute. Also, the ON UPDATE is always set to NO ACTION at this point.

The following should be used if you need to pull data from another table (related or not) into our class/model for ease of use (note the new property UserName and it's attribute).
```c#
[DataTable("logs")]
public class Log : Cope<Log>, IManageable
{
    [PrimaryKeyProperty]
    public Guid Id { get; set; }
    [DateCreatedProperty]
    public DateTime DateCreated { get; set; }
    [DateModifiedProperty]
    public DateTime DateModified { get; set; }

	[ForeignKey(typeof(User))]
    public Guid UserId { get; set; }
    public string Transaction { get; set; }
	[ForeignData(typeof(User))]
	public string UserName { get; set; }
}
```
With this configuration, OneData will look for the Name value inside our User class/model and get data based on the Log's UserId.

ForeignData attribute has three constructors. In the example above, we used the simplest of them but may raise some eyebrows in confusion since it seems so magical at firsts. Next, you will find a detailed explanation:

Just sending the JoinModel parameter:
```c#
[ForeignData(typeof(User))]
```
By doing this, OneData has to assume some configurations, which are: 
* Your ReferenceModel is the model the property belongs to.
* Your ReferenceIdName is using the name of your JoinModel plus the word 'Id'.
* Your ColumnName is called Name.

Just sending the JoinModel and ColumnName parameter:
```c#
[ForeignData(typeof(User), nameof(User.Name))]
```
By doing this, OneData has to assume some configurations, which are: 
* Your ReferenceModel is the model the property belongs to.
* Your ReferenceIdName is using the name of your JoinModel plus the word 'Id'.
 
Sending every parameter:
```c#
[ForeignData(typeof(User), typeof(Log), nameof(UserId), nameof(User.Name))]
```
Even tho this seems a bit messy, it's VERY powerful when used on nested properties.

All the magic will be done when you call any transaction of type Select.
#### Generic Stored Procedures
OneData knows that sometimes you need to call a stored procedure that's not a common transaction and for this, you can do the following:
```c#
Manager.StoredProcedure("<main table affected by your SP>", "<your stored procedure name>", <your connection name; null for default>, new Parameter("<parameter name>", <parameter value>));
```
The parameter ```tableName``` is not really used to execute anything and it will be removed in a future version.

The last parameter in this method accepts an array of ```Parameter``` objects. This means you could just pass a preconfigured array or parameters or pass one by one.

#### Async Methods
Every method corresponding to a transaction type can be called using Async, although this calls are not as easy as we wish them to be (yet!). Please read the following to undertand more.

This is the list of methods available for Async calls:
* Manager\<T\>.InsertAsync(T obj, QueryOptions queryOptions)
* Manager\<T\>.InsertMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
* Manager\<T\>.UpdateAsync(T obj, QueryOptions queryOptions)
* Manager\<T\>.UpdateMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
* Manager\<T\>.DeleteAsync(T obj, QueryOptions queryOptions)
* Manager\<T\>.DeleteMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
* Manager\<T\>.SelectAsync(Expression<Func<T, bool>> expression, QueryOptions queryOptions)
* Manager\<T\>.SelectAllAsync(QueryOptions queryOptions)
* Manager.StoredProcedureAsync(string tableName, string storedProcedure, QueryOptions queryOptions, params Parameter[] parameters)

As an example, the following would be used to call an Async Insert method to add a new Log into the database:
```c#
await Manager<Log>.InsertAsync(newLog, null);
```
This method's last parameter corresponds to ```QueryOptions``` object, which can be sent as null to apply the defaults.

## FAQ

<Pending.>

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

## Icons
[Icons by Icons8](https://icons8.com)
