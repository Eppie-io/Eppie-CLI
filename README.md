# Command line client for Eppie &#8212; an encrypted p2p email

[![LicenseBadge](https://img.shields.io/github/license/Eppie-io/Eppie-CLI.svg)](https://raw.githubusercontent.com/Eppie-io/Eppie-CLI/main/LICENSE)
![WindowsBadge](https://img.shields.io/badge/Windows-blue?logo=windows)
![LinuxBadge](https://img.shields.io/badge/Linux-gold?logo=linux&logoColor=black)
![macOSBadge](https://img.shields.io/badge/macOS-black?logo=apple)
[![FrameworkBadge](https://img.shields.io/badge/dynamic/xml?label=framework&query=//TargetFramework[1]&url=https://raw.githubusercontent.com/Eppie-io/Eppie-CLI/main/src/Eppie.CLI/Eppie.CLI/Eppie.CLI.csproj)](https://dotnet.microsoft.com/en-us/download/dotnet)
[![Build and Test](https://img.shields.io/github/actions/workflow/status/eppie-io/eppie-cli/build-and-test.yml?logo=github&branch=main&event=push)](https://github.com/Eppie-io/Eppie-CLI/actions/workflows/build-and-test.yml?query=branch%3Amain+event%3Apush)
[![CrowdinBadge](https://badges.crowdin.net/e/8fee200a40ee70ffd3fa6b7d8d23deee/localized.svg)](https://eppie.crowdin.com/eppie)
[![Release](https://img.shields.io/github/v/release/Eppie-io/Eppie-CLI)](https://github.com/Eppie-io/Eppie-CLI/releases/latest)

## Intro

Eppie-CLI is a work-in-progress command line client for the upcoming encrypted p2p email [Eppie](https://eppie.io). Currently, it is primarily used for testing during Eppie [Core module](https://github.com/Eppie-io/TuviCore) development. It has limited functionality at the moment, but in the future Eppie-CLI will become an official fully featured CLI for Eppie.

You might find Eppie-CLI interesting if

- You would like to contribute to Eppie development (yay! &#128077;)
- You want a CLI for your SMTP email. Eppie can be used as a conventional email client with Gmail, Microsoft Outlook etc. It can also work with Proton Mail.
- You want to be the first to try our p2p email, as soon as it's ready

In any case you are very welcome to fork, build and explore this module to your heart's content (see instructions below).

## Features

The decentralized protoccol is still in development and its features are not yet available in Eppie-CLI. Meanwhile Eppie works as a conventional CLI email client with additional security features:

- Creating a local account using a Seed Phrase according to [BIP39](https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki) standard
- [PGP](https://www.openpgp.org/) encryption support (WIP)
- Connecting any number of third-party email accounts (e.g. Gmail. Microsoft Outlook etc.)
- Connecting Proton Mail account
- Creating a local backup
- Viewing mailboxes
- Viewing a single message
- Writing & sending messages

## Build project

### Prerequisites

- [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Clone

First of all, clone the project repository to your machine:

```console
git clone --recursive https://github.com/Eppie-io/Eppie-CLI.git eppie-cli
```

### Build

To build the **Eppie.CLI** project, run the following command in the project root directory:

```console
dotnet build ./src/Eppie.CLI/
```

## Downloads

You may want to skip the building and download the latest release for your system:

### Windows binaries

- [Eppie.CLI-win-x64.zip](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-win-x64.zip)
- [Eppie.CLI-win-x86.zip](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-win-x86.zip)
- [Eppie.CLI-win-arm64.zip](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-win-arm64.zip)

### Linux binaries

- [Eppie.CLI-linux-x64.tar.gz](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-linux-x64.tar.gz)
- [Eppie.CLI-linux-arm64.tar.gz](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-linux-arm64.tar.gz)
- [Eppie.CLI-linux-arm.tar.gz](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-linux-arm.tar.gz)

### MacOS binaries

- [Eppie.CLI-osx-x64.tar.gz](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-osx-x64.tar.gz)
- [Eppie.CLI-osx-arm64.tar.gz](https://github.com/Eppie-io/Eppie-CLI/releases/latest/download/Eppie.CLI-osx-arm64.tar.gz)

## Launch

### If built from the source

To launch **Eppie Console** application, run the following command:

```console
dotnet run --project ./src/Eppie.CLI/Eppie.CLI/Eppie.CLI.csproj
```

To use **Gmail** or **Microsoft Outlook** OAuth2 authorization, you need to [register the application](docs/Register%20the%20application.md) and then to pass the **ClientId** and **ClientSecret** arguments with the command:

```console
dotnet run --project ./src/Eppie.CLI/Eppie.CLI/Eppie.CLI.csproj -- --Authorization:Gmail:ClientId="<gmail-client-id>" --Authorization:Gmail:ClientSecret="<gmail-client-secret>" --Authorization:Outlook:ClientId="<outlook-client-id>"
```

### If downloaded the binaries

Run this while in the project folder

#### Linux and MacOS

```console
./eppie-console
```

To authorize Eppie with **Gmail** and **Microsoft Outlook**:

```console
./eppie-console --Authorization:Gmail:ClientId="<gmail-client-id>" --Authorization:Gmail:ClientSecret="<gmail-client-secret>" --Authorization:Outlook:ClientId="<outlook-client-id>"
```

#### Windows

```console
.\eppie-console.exe
```

To authorize Eppie with **Gmail** and **Microsoft Outlook**:

```console
.\eppie-console.exe --Authorization:Gmail:ClientId="<gmail-client-id>" --Authorization:Gmail:ClientSecret="<gmail-client-secret>" --Authorization:Outlook:ClientId="<outlook-client-id>"
```

## Available commands

- **`-?|-h|--help`**

  Prints out a list of available commands.

- **`init`**

  Initializes the application and creates Eppie account.

- **`reset`**

  Resets the application and erases all its data.

- **`open`**

  Opens an existing Eppie account.

- **`add-account`**
  
  Adds an email account.

- **`list-accounts`**

  Displays a list of accounts.

- **`list-contacts`**

  Displays contacts from all accounts.

- **`show-all-messages`**

  Shows all messages from all accounts.

- **`show-folder-messages`**

  Shows messages from a specific account folder.

- **`show-contact-messages`**

  Shows messages for a specific contact.

- **`show-message`**

  Shows details of a specific message.

- **`send`**

  Sends a message.

- **`restore`**

  Restores your Eppie account.

- **`import`**

  Imports a key bundle from a file.

- **`exit`**

  Closes the application.

## Usage

WIP

## Planned features

As the main project matures more features will be added to this CLI, including but not limited to

- Creating a decentralized Eppie account
- Encrypted p2p messaging
- Encrypted decentralized backup
- Connecting existing decentralized identities, e.g. [ENS](https://ens.domains/)

## How to contribute

First of all this is a pretty ambitiois project and we are greateful beyond measure for every bit of help from our community.

If you decide to contribute, please create an issue first, or find an existing one, unless it's a very minor fix, like a typo.

[Here](https://eppie.crowdin.com/eppie) you can help Eppie with localization.

## Background

Eppie is a next generation email and decentralized identity provider. It features open protocol, serverless architecture, cryptocurrency-grade privacy with full account ownership, ease of use, and SMTP-to-web3 messaging capability.

Eppie is developped to store the data using [IPFS](https://github.com/ipfs/ipfs) infrastructure, and the transport layer will work through [SBBS](https://github.com/BeamMW/beam/wiki/Secure-bulletin-board-system-%28SBBS%29). But the architecture allows to easily plug in multiple storage and transport technologies. Eppie's e2e encryption is based on [Elliptic-curve](https://en.wikipedia.org/wiki/Elliptic-curve_cryptography) cryptography. GUI application is being written in C# with [Uno](https://github.com/unoplatform/uno), and CLI is pure C#. Eppie targets Windows, macOS, Linux, iOS, and Android platforms.
