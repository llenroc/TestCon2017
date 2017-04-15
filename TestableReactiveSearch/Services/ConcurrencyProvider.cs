using System.Reactive.Concurrency;

namespace TestableReactiveSearch.Services
{
    class ConcurrencyProvider : IConcurrencyProvider
    {
        public ConcurrencyProvider()
        {
            TimeBasedOperations = DefaultScheduler.Instance;
            Task = TaskPoolScheduler.Default;
            Thread = NewThreadScheduler.Default;
            Dispatcher = DispatcherScheduler.Current;
        }

        public IScheduler TimeBasedOperations { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler Dispatcher { get; }
    }
}