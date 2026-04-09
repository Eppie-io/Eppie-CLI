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

using NUnit.Framework;

using Tuvi.Toolkit.Cli;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ConsoleExtensionTests
    {
        [Test]
        public void ReadMultiLineWhenStandardInputClosesAfterContentReturnsCollectedText()
        {
            using StringReader input = new("First line" + Environment.NewLine + "Second line" + Environment.NewLine);
            TextReader originalInput = Console.In;

            try
            {
                Console.SetIn(input);

                string? result = ConsoleExtension.ReadMultiLine(string.Empty, "EOF");

                Assert.That(result, Is.EqualTo("First line" + Environment.NewLine + "Second line"));
            }
            finally
            {
                Console.SetIn(originalInput);
            }
        }

        [Test]
        public void ReadMultiLineWhenStandardInputClosesImmediatelyReturnsNull()
        {
            using StringReader input = new(string.Empty);
            TextReader originalInput = Console.In;

            try
            {
                Console.SetIn(input);

                string? result = ConsoleExtension.ReadMultiLine(string.Empty, "EOF");

                Assert.That(result, Is.Null);
            }
            finally
            {
                Console.SetIn(originalInput);
            }
        }
    }
}
