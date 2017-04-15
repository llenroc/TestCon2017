using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestableReactiveSearch.Services
{
    public interface ISearchServiceClient 
    {
        Task<IEnumerable<string>> SearchAsync(string searchTerm);
    }
}