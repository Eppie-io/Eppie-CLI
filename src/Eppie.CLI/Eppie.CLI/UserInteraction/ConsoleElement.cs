﻿// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie (https://eppie.io)                                    //
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

using Tuvi.Toolkit.Cli;

namespace Eppie.CLI.UserInteraction
{
    internal static class ConsoleElement
    {
        public static string? ReadValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), Console.ReadLine);
        }

        public static string? ReadSecretValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), () => ConsoleExtension.ReadSecretLine());
        }
    }
}