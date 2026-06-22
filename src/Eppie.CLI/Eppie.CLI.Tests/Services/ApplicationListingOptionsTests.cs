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

using Eppie.CLI.Services;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationListingOptionsTests
    {
        [Test]
        public void ConstructorWhenPageSizeIsPositiveStoresValues()
        {
            ApplicationListingOptions options = new(10, 5);

            Assert.Multiple(() =>
            {
                Assert.That(options.PageSize, Is.EqualTo(10));
                Assert.That(options.Limit, Is.EqualTo(5));
            });
        }

        [Test]
        public void ConstructorWhenLimitIsMissingUsesDefaultLimit()
        {
            ApplicationListingOptions options = new(10, null);

            Assert.That(options.Limit, Is.EqualTo(ApplicationListingOptions.DefaultLimit));
        }

        [Test]
        public void ConstructorWhenPageSizeIsZeroThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new ApplicationListingOptions(0, null), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorWhenLimitIsZeroThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new ApplicationListingOptions(10, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetRequestSizeWhenLimitRemainsReturnsMinimumOfPageSizeAndRemainingLimit()
        {
            ApplicationListingOptions options = new(10, 25);

            int result = options.GetRequestSize(3);

            Assert.That(result, Is.EqualTo(3));
        }
    }
}
