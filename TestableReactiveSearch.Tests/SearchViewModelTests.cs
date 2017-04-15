using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Extensions;
using TestableReactiveSearch.Services;

namespace TestableReactiveSearch.Tests
{
    [TestClass]
    public class SearchViewModelTests : ReactiveTest
    {
        const long ONE_SECOND = TimeSpan.TicksPerSecond;

        [TestMethod]
        public void OneLetterWord_HalfSecondGap_NoSearchSentToService()
        {
            var fakeServiceClient = Substitute.For<ISearchServiceClient>();
            var fakeConcurrencyProvider = Substitute.For<IConcurrencyProvider>();
            var testScheduler = new TestScheduler();

            fakeConcurrencyProvider.ReturnsForAll<IScheduler>(testScheduler);

            var vm = new SearchViewModel(fakeServiceClient, fakeConcurrencyProvider);

            testScheduler.Start();
            testScheduler.AdvanceTo(500);
            vm.SearchTerm = "A";
            testScheduler.AdvanceBy(ONE_SECOND / 2);

            fakeServiceClient.DidNotReceive().SearchAsync("A");
        }
        [TestMethod]
        public void MoreThanThreeLetters_HalfSecondGap_SearchSentToService()
        {
            var fakeServiceClient = Substitute.For<ISearchServiceClient>();
            var fakeConcurrencyProvider = Substitute.For<IConcurrencyProvider>();
            var testScheduler = new TestScheduler();

            fakeConcurrencyProvider.ReturnsForAll<IScheduler>(testScheduler);

            var vm = new SearchViewModel(fakeServiceClient, fakeConcurrencyProvider);

            testScheduler.Start();
            testScheduler.AdvanceTo(500);
            vm.SearchTerm = "reactive";
            testScheduler.AdvanceBy(ONE_SECOND / 2);

            fakeServiceClient.Received().SearchAsync("reactive");
        }

        [TestMethod]
        public void MoreThanThreeLetters_LessThanHalfSecondGap_NoSearchSentToService()
        {
            var fakeServiceClient = Substitute.For<ISearchServiceClient>();
            var fakeConcurrencyProvider = Substitute.For<IConcurrencyProvider>();
            var testScheduler = new TestScheduler();

            fakeConcurrencyProvider.ReturnsForAll<IScheduler>(testScheduler);

            var vm = new SearchViewModel(fakeServiceClient, fakeConcurrencyProvider);

            testScheduler.Start();
            testScheduler.AdvanceTo(500);
            vm.SearchTerm = "reactive";
            testScheduler.AdvanceBy(ONE_SECOND / 4);

            fakeServiceClient.DidNotReceive().SearchAsync("reactive");
        }

        [TestMethod]
        public void TwoValidWords_OneSecondGap_OnlyFirstIsSearched()
        {
            var fakeServiceClient = Substitute.For<ISearchServiceClient>();
            var fakeConcurrencyProvider = Substitute.For<IConcurrencyProvider>();
            var testScheduler = new TestScheduler();

            fakeConcurrencyProvider.ReturnsForAll<IScheduler>(testScheduler);

            var vm = new SearchViewModel(fakeServiceClient, fakeConcurrencyProvider);

            testScheduler.Start();
            testScheduler.AdvanceTo(500);
            vm.SearchTerm = "reactive";
            testScheduler.AdvanceBy(ONE_SECOND);
            vm.SearchTerm = "reactive";
            testScheduler.AdvanceBy(ONE_SECOND);

            fakeServiceClient.Received(1).SearchAsync("reactive");
        }

        [TestMethod]
        public void TwoValidWords_SlowSearchThenFastSearch_SecondSearchResultsOnly()
        {
            var fakeServiceClient = Substitute.For<ISearchServiceClient>();
            var fakeConcurrencyProvider = Substitute.For<IConcurrencyProvider>();
            var testScheduler = new TestScheduler();

            fakeConcurrencyProvider.ReturnsForAll<IScheduler>(testScheduler);

            fakeServiceClient.SearchAsync("first").Returns(
                testScheduler.CreateColdObservable(OnNext<IEnumerable<string>>(2 * ONE_SECOND, new[] {"first"}),OnCompleted<IEnumerable<string>>(2 * ONE_SECOND)).ToTask());
            fakeServiceClient.SearchAsync("second").Returns(
                testScheduler.CreateColdObservable(OnNext<IEnumerable<string>>(1, new[] { "second" }), OnCompleted<IEnumerable<string>>(1)).ToTask());
            var vm = new SearchViewModel(fakeServiceClient, fakeConcurrencyProvider);

            testScheduler.Start();
            vm.SearchTerm = "first";
            testScheduler.AdvanceBy(ONE_SECOND);
            vm.SearchTerm = "second";
            testScheduler.AdvanceBy(5 * ONE_SECOND);

            Assert.AreEqual("second", vm.SearchResults.First());
        }
    }
}
