# Orleans.StorageProviders.SimpleSQLServerStorage

[![Build status](https://ci.appveyor.com/api/projects/status/2nhdpoljh1k470le?svg=true)](https://ci.appveyor.com/project/OrleansContrib/orleans-storageproviders-simplesqlserverstorage)

[![NuGet](https://img.shields.io/nuget/v/Orleans.StorageProviders.SimpleSQLServerStorage.svg?style=flat)](https://www.nuget.org/packages/Orleans.StorageProviders.SimpleSQLServerStorage)

A KeyValue SQLServer implementation of the Orleans Storage Provider model. Uses an EF code-first table to store grain keys with binary and/or json serialized data

## Usage

```ps
Install-Package Orleans.StorageProviders.SimpleSQLServerStorage
```

Decorate your grain with the StorageProvider attribute e.g.

```cs
[StorageProvider(ProviderName = "PubSubStore")]
```

in your OrleansConfiguration.xml configure the provider like this:

```xml
<StorageProviders>
      <Provider Type="Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage" Name="PubSubStore"
                ConnectionString="Data Source=(LocalDB)\v11.0; Integrated Security=True;"
                UseJsonFormat="false" />
      
      <Provider Type="Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage" Name="SomeOtherGrainStorage"
                ConnectionString="Data Source=(LocalDB)\v11.0; Integrated Security=True;"
                UseJsonFormat="both" />
    </StorageProviders>
```

## Setup
If using SQLServer proper, create an empty database and make sure the connecting user has the following permissions
```sql
[db_datareader]
[db_datawriter]
[db_ddladmin]
```


## Configuration

The following attributes can be used on the `<Provider/>` tag to configure the provider:

* __UseJsonFormat="true/false/both"__ (optional) Defaults to `false`, if set to `false` the Orleans binary serializer is used. If set to `true` json data is serialized.  if set to `both` then both json and binary data is produced and persisted, but the binary data is used for deserialization(meant for debugging purposes).
* __ConnectionString="..."__ (required) the connection string to your SQLServer database (i.e. `any standard SQL Server connection string`)


