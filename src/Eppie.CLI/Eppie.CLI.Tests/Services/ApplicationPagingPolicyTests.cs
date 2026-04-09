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
    public class ApplicationPagingPolicyTests
    {
        [Test]
        public void ShouldAggregatePagesBeforeWriteWhenOutputIsJsonReturnsTrue()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json"));

            Assert.That(policy.ShouldAggregatePagesBeforeWrite, Is.True);
        }

        [Test]
        public void ShouldContinueWhenOutputIsJsonAndHasMoreDoesNotAskAndReturnsTrue()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json"));
            bool askMoreCalled = false;

            bool result = policy.ShouldContinue(hasMore: true, () =>
            {
                askMoreCalled = true;
                return false;
            });

            Assert.That(result, Is.True);
            Assert.That(askMoreCalled, Is.False);
        }

        [Test]
        public void ShouldContinueWhenNonInteractiveAndHasMoreReturnsFalse()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: true));
            bool askMoreCalled = false;

            bool result = policy.ShouldContinue(hasMore: true, () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.That(result, Is.False);
            Assert.That(askMoreCalled, Is.False);
        }

        [Test]
        public void ShouldContinueWhenInteractiveTextAndHasMoreUsesAskMoreResult()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions());
            bool askMoreCalled = false;

            bool result = policy.ShouldContinue(hasMore: true, () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.That(result, Is.True);
            Assert.That(askMoreCalled, Is.True);
        }

        [Test]
        public void ShouldContinueWhenInteractiveTextAndAskMoreReturnsFalseReturnsFalse()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions());
            bool askMoreCalled = false;

            bool result = policy.ShouldContinue(hasMore: true, () =>
            {
                askMoreCalled = true;
                return false;
            });

            Assert.That(result, Is.False);
            Assert.That(askMoreCalled, Is.True);
        }

        [Test]
        public void ShouldContinueWhenOutputIsJsonAndHasMoreIsFalseReturnsFalseWithoutPrompt()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json"));
            bool askMoreCalled = false;

            bool result = policy.ShouldContinue(hasMore: false, () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.That(result, Is.False);
            Assert.That(askMoreCalled, Is.False);
        }

        [Test]
        public void ShouldContinueWhenHasMoreIsFalseReturnsFalse()
        {
            ApplicationPagingPolicy policy = new(TestApplicationFactory.CreateLaunchOptionsOptions());
            bool askMoreCalled = false;

            bool result = policy.ShouldContinue(hasMore: false, () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.That(result, Is.False);
            Assert.That(askMoreCalled, Is.False);
        }
    }
}
