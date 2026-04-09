# Eppie CLI agent test plan

Use this document when an autonomous or semi-autonomous agent must test `eppie-console` in `--non-interactive=true` mode, with maximum coverage and reproducible results.

This file complements `eppie-cli-agent-skill.md`.
The skill document explains how an agent should operate the CLI in general.
This document explains how to validate the non-interactive execution paths.

## Goal

Cover:
- startup options,
- command execution in `--non-interactive=true` mode,
- JSON and text outputs,
- success and failure cases,
- local reproducible scenarios,
- optional provider-specific scenarios that may require manual credentials.

## Core testing rules for AI agents

1. Prefer `--non-interactive=true` for automation.
2. Prefer `--output=json` when validating machine-readable behavior.
3. Put global launch options before `--` and put the startup command after `--`.
4. For stateful non-interactive commands, use `--unlock-password-stdin=true`.
5. For destructive commands such as `reset`, use `--assume-yes=true` unless intentionally testing the warning path.
6. Use one isolated working directory per scenario.
7. Do not assume state is reused across separate process launches.
8. Do not rely only on process exit code; inspect JSON `type` and `code`.
9. For `send` in `--non-interactive=true` mode, terminate the body by closing `stdin`.
10. In JSON automation scenarios, validate that machine-readable output stays on `stdout` and is still parseable even when the command returns `type = error`.
11. For paged read commands, prefer explicit `-l` when you need reproducible payload size. If `-l` is omitted, the command uses its default total limit.
12. Synchronize the relevant folder or folders before validating newly arrived messages. Without synchronization, newly arrived messages should not be expected to appear.

To avoid repeating the same startup flags in test commands, you can preset them in `appsettings.json` next to `eppie-console`, for example:
- `{ "non-interactive": true, "output": "json", "unlock-password-stdin": true }`

## Coverage map

### Startup options

Mandatory coverage:
- `--non-interactive=true`
- `--output=json`
- `--assume-yes=true`
- `--unlock-password-stdin=true`

Recommended combinations:
- `--non-interactive=true --output=json`
- `--non-interactive=true --unlock-password-stdin=true --output=json`
- `--non-interactive=true --assume-yes=true --output=json`

### Commands

For paged read commands such as `show-all-messages`, `show-folder-messages`, `list-contacts`, and `show-contact-messages`:
- `-s` / `--page-size` controls only the page size
- `-l` / `--limit` controls the total number of returned records
- if `-l` is omitted, the command uses its default total limit
- paged result `meta` can include fields such as `header`, `returned`, and `hasMore`

Local reproducible coverage:
- `init`
- `reset`
- `add-account -t dec`
- `list-accounts`
- `list-folders`
- `send`
- `sync-folder`
- `show-all-messages`
- `show-message`
- `delete-message`
- `show-folder-messages`
- `list-contacts`
- `show-contact-messages` if a contact is available

Optional/manual coverage:
- `add-account -t email`
- `add-account -t proton`
- provider-specific sync and message flows for Gmail, Outlook, Proton, or IMAP/SMTP accounts
- `restore`
- `import`

## Standard test environment

Create one dedicated working directory for the whole scenario.

Use that working directory for scenario data such as the local vault and database.
If you preset startup flags in `appsettings.json`, place that file next to `eppie-console`, not in the scenario working directory.
Use an isolated temporary directory for disposable or destructive scenarios.

Examples:
- Windows: `C:\temp\eppie-test-001`
- Linux: `/tmp/eppie-test-001`

Run every command in the scenario with that directory as the process working directory.

Example workflow:
- create `C:\temp\eppie-run-001`
- run `--non-interactive=true init` with `WorkingDirectory = C:\temp\eppie-run-001`
- run `list-accounts`, `show-all-messages`, `reset`, and other checks with the same working directory
- delete the folder when finished

Short PowerShell example:

```powershell
$workDir = Join-Path $env:TEMP "eppie-run-001"
New-Item -ItemType Directory -Force -Path $workDir | Out-Null
Push-Location $workDir

# run eppie-console commands here

Pop-Location
Remove-Item $workDir -Recurse -Force
```

Short Linux / bash example:

```sh
workDir="/tmp/eppie-run-001"
mkdir -p "$workDir"
cd "$workDir"

# run eppie-console commands here

cd /
rm -rf "$workDir"
```

## Baseline local scenario

This is the main scenario that should run first.

### 1. Initialize a fresh vault
Command:
- `--non-interactive=true --output=json -- init`

`stdin`:
1. vault password

Expected result:
- `type = status`
- `code = initialized`
- `data.seedPhrase` is a non-empty array

### 2. Verify locked command warning without `--unlock-password-stdin=true`
Command:
- `--non-interactive=true --output=json -- list-accounts`

`stdin`:
- none

Expected result:
- `type = warning`
- `code = startupCommandRequiresUnlockPasswordFromStandardInput`

### 3. Verify invalid password handling
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- list-accounts`

`stdin`:
1. wrong password

Expected result:
- `type = warning`
- `code = invalidPassword`

### 4. List accounts in a fresh vault
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- list-accounts`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = accounts`
- `data` is an empty array
- `meta = null`

### 5. Add a DEC account
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- add-account -t dec`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = accountAdded`
- `data.address` is non-empty
- `data.accountType = Dec`
- `meta = null`

Store the returned `data.address` as `<dec-address>`.

### 6. List accounts after account creation
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- list-accounts`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = accounts`
- `data` contains at least one account
- one account address matches `<dec-address>`
- `data[0].accountType = Dec`

### 6a. List folders for the account
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- list-folders -a <dec-address>`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = folders`
- `data` is an array
- `meta.account = <dec-address>`

If one folder is available, store:
- `<folder-name>` from a suitable item such as the one whose `roles` contains `inbox`; otherwise keep using `Inbox` only if it is explicitly present in the returned list

### 7. Send a message to the DEC account itself
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- send -s <dec-address> -r <dec-address> -t "Hello from automation"`

`stdin` order:
1. vault password
2. `Hello from automation`
3. `Second body line`
4. close `stdin`

Expected result:
- `type = status`
- `code = messageSent`
- `data.from = <dec-address>`
- `data.to = <dec-address>`
- `data.subject = Hello from automation`

Notes:
- In `--non-interactive=true` mode, `send` reads the body until end-of-stream.

### 7a. Synchronize the relevant folder before validating newly arrived messages
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- sync-folder -a <dec-address> -f <folder-name>`

`stdin`:
1. vault password

Expected result:
- `type = status`
- `code = folderSynced`

If the current account type or folder does not support a meaningful sync in the local scenario, record the observed result and continue.

### 8. Read recent messages after synchronization
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- show-all-messages -s 5 -l 5`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = messages`
- `data` is an array
- `meta.header` may be `All messages:`
- `meta.returned` is a non-negative integer
- `meta.hasMore` is a boolean

If at least one message is present, also validate:
- `data[0].to` is an array
- `data[0].from` is an array

If at least one message is present, store:
- `<message-id>` from `data[0].id`
- `<message-pk>` from `data[0].pk`

### 9. Read one message
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- show-message -a <dec-address> -f Inbox -i <message-id> -k <message-pk>`

`stdin`:
1. vault password

Run this step only if step 8 returned a message item.

Expected result:
- `type = result`
- `code = message`
- `data.id = <message-id>`
- `data.pk = <message-pk>`
- `data.to` is an array
- `data.from` is an array

### 10. Read one folder
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- show-folder-messages -a <dec-address> -f <folder-name> -s 5 -l 5`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = messages`
- `meta.returned` is a non-negative integer
- `meta.hasMore` is a boolean

### 10a. Delete one message
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- delete-message -a <dec-address> -f <folder-name> -i <message-id> -k <message-pk>`

`stdin`:
1. vault password

Run this step only if step 8 returned a message item.

Expected result:
- `type = status`
- `code = messageDeleted`
- `data.account = <dec-address>`
- `data.folder = <folder-name>`
- `data.id = <message-id>`
- `data.pk = <message-pk>`

Recommended follow-up validation:
- run `show-folder-messages` again for `<folder-name>`
- verify the deleted message no longer appears in that folder result

Notes:
- when deleting from any folder other than `Trash`, the message should move to `Trash`
- when deleting from `Trash`, the message should be removed permanently
- validate both stages if the scenario has a usable `Trash` folder

### 11. List contacts
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- list-contacts -s 10 -l 10`

`stdin`:
1. vault password

Expected result:
- `type = result`
- `code = contacts`
- `data` is an array

If one contact is available, store `<contact-address>` from the first item.

### 12. Read messages for a contact
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- show-contact-messages -c <contact-address> -s 10 -l 10`

`stdin`:
1. vault password

Run this step only if step 11 returned a contact.

Expected result:
- `type = result`
- `code = messages`
- `meta.returned` is a non-negative integer
- `meta.hasMore` is a boolean

### 13. Reset without `--assume-yes=true` to validate warning behavior
Command:
- `--non-interactive=true --output=json -- reset`

`stdin`:
- none

Expected result:
- `type = warning`
- `code = commandRequiresAssumeYesInNonInteractiveMode`

### 14. Reset with `--assume-yes=true`
Command:
- `--non-interactive=true --assume-yes=true --output=json -- reset`

`stdin`:
- none

Expected result:
- `type = status`
- `code = reset`

### 15. Post-reset note
If post-reset validation is needed, use a suitable non-interactive command whose behavior explicitly documents the expected uninitialized result.

## Text-output spot checks

In addition to the JSON scenario, validate a small text-mode subset.

Recommended checks:
- `--non-interactive=true --assume-yes=true -- reset`
- `--non-interactive=true --unlock-password-stdin=true -- add-account -t dec`
- `--non-interactive=true --unlock-password-stdin=true -- send -s <sender> -r <receiver> -t "<subject>"`

Expected text samples:
- `The application has just been reset.`
- `Account <generated-address> added (Dec).`
- `Message sent from <sender> to <receiver>. Subject: <subject>`

## Negative tests

### Empty body for `send`
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- send -s <dec-address> -r <dec-address> -t "Empty body"`

`stdin` order:
1. vault password
2. close `stdin` immediately, providing no body lines

Expected result:
- `type = status`
- `code = messageSent`
- `data.subject = Empty body`

Note:
- in `--non-interactive=true` mode, end-of-stream is the message body terminator
- closing `stdin` immediately sends the message with an empty body

### Unknown sender account
Command:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- send -s missing@eppie -r missing@eppie -t "Missing account"`

`stdin` order:
1. vault password
2. message body
3. close `stdin`

Expected result:
- `type = error`
- `code = unhandledException`
- `data.exceptionType = Tuvi.Core.Entities.AccountIsNotExistInDatabaseException`
- `stdout` remains valid JSON
- `stderr` does not prepend a stack trace before the JSON response in this mode

### Unsupported interactive-only behavior in `--non-interactive=true`
When a command path still requires interactive selection or confirmation, expect either:
- a structured warning, or
- a structured error such as `nonInteractiveOperationNotSupported`

## Optional provider-specific scenarios

These scenarios are useful, but may require manual setup, secrets, browser auth, or live network access.

### Add regular email account
Command family:
- `add-account -t email ...`

Validate:
- explicit server settings,
- login/password handling,
- account appears in `list-accounts`,
- `sync-folder` returns expected status,
- messages can be listed and opened.

### Add Proton account
Command family:
- `add-account -t proton --input-json-stdin ...`

Validate:
- authorization flow,
- account creation result,
- folder sync,
- message listing.

Recommended automation pattern:
- `--non-interactive=true --unlock-password-stdin=true --output=json -- add-account -t proton --input-json-stdin`

`stdin` shape:
1. vault password on the first line if `--unlock-password-stdin=true` is used
2. the remaining standard input as a JSON object

Recommended JSON object fields:
- `email` — required
- `accountPassword` — required
- `mailboxPassword` — required only if the Proton flow requests it
- `twoFactorCode` — required only if the Proton flow requests it

Recommended negative coverage:
- invalid JSON returns:
  - `type = error`
  - `code = structuredStandardInputInvalidJson`
- missing required property such as `email` returns:
  - `type = error`
  - `code = structuredStandardInputMissingProperty`
  - `data.propertyName = email`

Automation notes:
- do not put Proton secrets in command-line arguments
- verify that the JSON response remains parseable on `stdout` for both success and structured error cases
- use `list-folders` before folder-specific commands so provider-specific names such as `All Sent` are discovered instead of guessed
- use `sync-folder` before validating newly arrived messages

### Restore
Command family:
- `restore ...`

Validate:
- reset-or-confirm behavior if local data already exists,
- restore path handling,
- restored account availability,
- post-restore `list-accounts` / message access.

### Import
Command family:
- `import ...`

Validate:
- valid file path,
- invalid file path,
- post-import behavior of dependent operations.

## Agent report template

For each run, record:
- executable path
- platform: Windows / Linux / macOS
- shell: PowerShell / bash
- working directory
- command line
- exact `stdin` shape
- raw output
- raw stderr output
- parsed JSON `type`
- parsed JSON `code`
- pass/fail verdict
- notes about deviations from documentation

## Pass criteria

A test run is successful when:
- all mandatory local steps execute,
- expected JSON envelopes match documented behavior,
- `sync-folder` is used before validating newly arrived messages,
- destructive warnings and unlock-password warnings are observed where expected,
- `send` works with multi-line body terminated by end-of-stream,
- `delete-message` returns `messageDeleted`, removes the message from the original folder result, and supports the expected two-step Trash flow,
- reset completes successfully and any follow-up verification uses a documented non-interactive check,
- no undocumented regressions are observed.

## Cleanup

After the scenario:
- run `--non-interactive=true --assume-yes=true -- reset` if needed,
- delete the isolated working directory,
- do not reuse the same vault directory for unrelated scenarios.
