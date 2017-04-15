using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using TestableReactiveSearch.Services;

namespace TestableReactiveSearch.Tests
{
    [TestClass]
    public class SearchViewModelTests
    {
        [TestMethod]
        public void SearchTermChanged_MoreThanThreeLetters_SearchSentToService()
        {
            var serviceClient=Substitute.For<ISearchServiceClient>();
            
            var vm=new SearchViewModel(serviceClient);
            vm.SearchTerm = "reactive";

            serviceClient.Received().SearchAsync("reactive");
        }
    }
}
