# Building Eppie Console

## Prerequisites

- .NET 7.0 [download](https://dotnet.microsoft.com/en-us/download)

## Build

To build the **Eppie.CLI** project, run the following command in the project root directory:

```console
dotnet build .\src\Eppie.CLI\
```

### How to build with forked submodules

To build the project with your own forked submodules, you have to add the following option to the `.gitconfig` file:

- When using the SSH protocol

```ini
# .gitconfig

[url "git@github.com:<USER-NAME>/<REPOSITORY-NAME>.git"]
    insteadOf = https://github.com/Eppie-io/<REPOSITORY-NAME>.git
```

- When using HTTPS protocol

```ini
# .gitconfig

[url "https://github.com/<USER-NAME>/<REPOSITORY-NAME>.git"]
    insteadOf = https://github.com/Eppie-io/<REPOSITORY-NAME>.git
```

## Launches

To launch **Eppie Console** application, you can run the following command:

```console
dotnet run --project .\src\Eppie.CLI\Eppie.CLI\Eppie.CLI.csproj
```

### Environment

To change the default environment configuration to some custom configuration, run the command with the arguments `--ENVIRONMENT=<environment-name>`:

```console
dotnet run --project .\src\Eppie.CLI\Eppie.CLI\Eppie.CLI.csproj -- --ENVIRONMENT=Development
```

- The default environment is **Production**. The location of the configuration file is *.\src\Eppie.CLI\Eppie.CLI\appsettings.json*.
- The **Development** environment configuration file location: *.\src\Eppie.CLI\Eppie.CLI\appsettings.Development.json*. This file overrides the default configuration.
