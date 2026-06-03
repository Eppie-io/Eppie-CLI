// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System.Diagnostics;
using System.Text.Json;

using Eppie.CLI.Services;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    internal sealed class ApplicationCommandTestHarness : IDisposable
    {
        private const string DefaultPassword = TestConstants.Password;
        private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(15);
        private readonly TemporaryWorkingDirectory _workingDirectory;

        private ApplicationCommandTestHarness(TemporaryWorkingDirectory workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        internal string WorkingDirectory => _workingDirectory.FullName;

        internal static ApplicationCommandTestHarness Create(string workingDirectoryPrefix)
        {
            return new ApplicationCommandTestHarness(TemporaryWorkingDirectory.Create(workingDirectoryPrefix));
        }

        internal Task<ProcessResult> InitializeAsync(bool nonInteractive = false, bool outputJson = false, string password = DefaultPassword)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            string standardInput = nonInteractive
                ? CreateStandardInput(password)
                : CreateStandardInput(password, password);

            return RunApplicationAsync("init", standardInput, nonInteractive: nonInteractive, outputJson: outputJson);
        }

        internal async Task<JsonCommandResult> InitializeJsonAsync(bool nonInteractive = true, string password = DefaultPassword)
        {
            ProcessResult processResult = await InitializeAsync(nonInteractive: nonInteractive, outputJson: true, password: password).ConfigureAwait(false);
            return JsonCommandResult.Create(processResult);
        }

        internal Task<JsonCommandResult> AddDecAccountJsonAsync(string vaultPassword = DefaultPassword, bool nonInteractive = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(vaultPassword);

            return RunJsonCommandAsync([TestConstants.AddAccountCommand, TestConstants.ShortTypeOption, TestConstants.DecAccountCommandType], CreateStandardInput(vaultPassword), unlockPasswordFromStandardInput: true, nonInteractive: nonInteractive);
        }

        internal async Task<JsonCommandResult> RunJsonCommandAsync(string startupCommand, string? standardInput = null, bool? unlockPasswordFromStandardInput = null, bool nonInteractive = false, bool assumeYes = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(startupCommand);

            ProcessResult processResult = await RunApplicationAsync(startupCommand, standardInput, unlockPasswordFromStandardInput, nonInteractive, assumeYes, outputJson: true).ConfigureAwait(false);
            return JsonCommandResult.Create(processResult);
        }

        internal async Task<JsonCommandResult> RunJsonCommandAsync(IReadOnlyList<string> startupCommandArguments, string? standardInput = null, bool? unlockPasswordFromStandardInput = null, bool nonInteractive = false, bool assumeYes = false)
        {
            ArgumentNullException.ThrowIfNull(startupCommandArguments);

            ProcessResult processResult = await RunApplicationAsync(startupCommandArguments, standardInput, unlockPasswordFromStandardInput, nonInteractive, assumeYes, outputJson: true).ConfigureAwait(false);
            return JsonCommandResult.Create(processResult);
        }

        internal static string GetAddressFromAddAccountResult(JsonCommandResult addAccountResult)
        {
            ArgumentNullException.ThrowIfNull(addAccountResult);

            return addAccountResult.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonAddressProperty).GetString()!;
        }

        internal static MessageIdentity GetFirstMessageIdentity(JsonCommandResult messagesResult)
        {
            ArgumentNullException.ThrowIfNull(messagesResult);

            JsonElement message = messagesResult.RootElement.GetProperty(TestConstants.JsonDataProperty)[0];

            return new MessageIdentity(
                message.GetProperty(TestConstants.JsonIdProperty).GetUInt32(),
                message.GetProperty(TestConstants.JsonPkProperty).GetInt32(),
                message.GetProperty(TestConstants.JsonFolderProperty).GetString()!);
        }

        internal static string GetTrashFolderName(JsonCommandResult foldersResult)
        {
            ArgumentNullException.ThrowIfNull(foldersResult);

            return foldersResult.RootElement.GetProperty(TestConstants.JsonDataProperty)
                .EnumerateArray()
                .First(x => x.GetProperty(TestConstants.JsonRolesProperty).EnumerateArray().Any(role => string.Equals(role.GetString(), TestConstants.TrashRole, StringComparison.Ordinal)))
                .GetProperty(TestConstants.JsonFullNameProperty)
                .GetString()!;
        }

        internal Task<ProcessResult> RunApplicationAsync(string startupCommand, string? standardInput = null, bool? unlockPasswordFromStandardInput = null, bool nonInteractive = false, bool assumeYes = false, bool outputJson = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(startupCommand);

            return RunApplicationAsync([startupCommand], standardInput, unlockPasswordFromStandardInput, nonInteractive, assumeYes, outputJson);
        }

        internal async Task<ProcessResult> RunApplicationAsync(IReadOnlyList<string> startupCommandArguments, string? standardInput = null, bool? unlockPasswordFromStandardInput = null, bool nonInteractive = false, bool assumeYes = false, bool outputJson = false)
        {
            ArgumentNullException.ThrowIfNull(startupCommandArguments);

            string applicationPath = typeof(ApplicationLaunchOptions).Assembly.Location;

            ProcessStartInfo startInfo = new("dotnet")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory
            };

            startInfo.ArgumentList.Add(applicationPath);

            if (unlockPasswordFromStandardInput is not null)
            {
                startInfo.ArgumentList.Add($"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={(unlockPasswordFromStandardInput.Value ? TestConstants.True : TestConstants.False)}");
            }

            if (nonInteractive)
            {
                startInfo.ArgumentList.Add($"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}");
            }

            if (assumeYes)
            {
                startInfo.ArgumentList.Add($"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}={TestConstants.True}");
            }

            if (outputJson)
            {
                startInfo.ArgumentList.Add($"--{ApplicationLaunchOptions.OutputConfigurationKey}={TestConstants.Json}");
            }

            if (startupCommandArguments.Count > 0)
            {
                startInfo.ArgumentList.Add(StartupCommandArguments.CommandDelimiter);
            }

            foreach (string startupCommandArgument in startupCommandArguments)
            {
                startInfo.ArgumentList.Add(startupCommandArgument);
            }

            using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the application process.");

            Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

            if (standardInput is not null)
            {
                await process.StandardInput.WriteAsync(standardInput).ConfigureAwait(false);
            }

            process.StandardInput.Close();

            using CancellationTokenSource timeoutCancellationTokenSource = new(ProcessTimeout);

            try
            {
                await process.WaitForExitAsync(timeoutCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (timeoutCancellationTokenSource.IsCancellationRequested)
            {
                TryKillProcess(process);
                throw new TimeoutException($"The application process did not exit within {ProcessTimeout.TotalSeconds:0} seconds. Arguments: {string.Join(' ', startInfo.ArgumentList)}.");
            }

            string standardOutput = await standardOutputTask.ConfigureAwait(false);
            string standardError = await standardErrorTask.ConfigureAwait(false);

            return new ProcessResult(process.ExitCode, standardOutput, standardError);
        }

        public void Dispose()
        {
            _workingDirectory.Dispose();
        }

        private static string CreateStandardInput(params string[] lines)
        {
            ArgumentNullException.ThrowIfNull(lines);

            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        private static void TryKillProcess(Process process)
        {
            ArgumentNullException.ThrowIfNull(process);

            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

        internal sealed record MessageIdentity(uint Id, int Pk, string Folder);

        internal sealed class JsonCommandResult : IDisposable
        {
            private JsonCommandResult(ProcessResult processResult, JsonDocument json)
            {
                ProcessResult = processResult;
                Json = json;
            }

            internal ProcessResult ProcessResult { get; }

            internal JsonDocument Json { get; }

            internal JsonElement RootElement => Json.RootElement;

            internal static JsonCommandResult Create(ProcessResult processResult)
            {
                ArgumentNullException.ThrowIfNull(processResult);

                try
                {
                    return new JsonCommandResult(processResult, JsonDocument.Parse(processResult.StandardOutput));
                }
                catch (JsonException ex)
                {
                    Assert.Fail($"Output is not valid JSON: {ex.Message}{Environment.NewLine}Actual output:{Environment.NewLine}{processResult.StandardOutput}");
                    throw;
                }
            }

            public void Dispose()
            {
                Json.Dispose();
            }
        }
    }
}
