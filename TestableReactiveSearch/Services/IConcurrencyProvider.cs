using System.Reactive.Concurrency;

namespace TestableReactiveSearch.Services
{
    public interface IConcurrencyProvider
    {
        IScheduler TimeBasedOperations { get; }
        IScheduler Task { get; }
        IScheduler Thread { get; }
        IScheduler Dispatcher { get; }
    }
}