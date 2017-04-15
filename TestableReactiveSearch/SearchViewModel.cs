using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TestableReactiveSearch.Properties;
using TestableReactiveSearch.Services;

namespace TestableReactiveSearch
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        private string _searchTerm;
        private IDisposable _subscription;
        private IEnumerable<string> _searchResults;
        public event PropertyChangedEventHandler PropertyChanged;

        public SearchViewModel(
            ISearchServiceClient searchServiceClient,
            IConcurrencyProvider concurrencyProvider)
        {
            int minTextLength = 3;

            var terms =
                Observable.FromEventPattern<PropertyChangedEventArgs>(this, nameof(PropertyChanged))
                    .Where(e => e.EventArgs.PropertyName == nameof(SearchTerm))
                    .Select(_ => SearchTerm);


            _subscription =
                terms
                    .Where(txt => txt.Length >= 3)
                    .Throttle(TimeSpan.FromSeconds(0.5))
                    .DistinctUntilChanged()
                    .Select(txt => searchServiceClient.SearchAsync(txt))
                    .Switch()
                    .ObserveOn(concurrencyProvider.Dispatcher)
                    .Subscribe(
                        results => SearchResults = results,
                        err => { Debug.WriteLine(err); },
                        () =>
                        {
                            /* OnCompleted */
                        });


            #region clearing results for short search terms

            terms
                .Where(txt => txt.Length < minTextLength)
                .ObserveOnDispatcher()
                .Subscribe(
                    results => SearchResults = Enumerable.Empty<string>(),
                    err => { Debug.WriteLine(err); },
                    () => {/* OnCompleted */});

            #endregion
        }
        public SearchViewModel() :
            this(new SearchServiceClient(),new ConcurrencyProvider())
        {
        }



        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                if (value == _searchTerm) return;
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> SearchResults
        {
            get { return _searchResults; }
            set
            {
                if (Equals(value, _searchResults)) return;
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        #region INPC generated code

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
