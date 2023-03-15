using System;
using System.Collections.Generic;
using Trementa.OpenApi.Generator;

namespace Trementa.OpenApi.Generator.Tracking
{
    public class ErrorTracker : IObservable<Error>, IDisposable
    {
        public ErrorTracker(IEnumerable<IObserver<Error>> errorObservers)
            => Observers.AddRange(errorObservers);

        protected List<IObserver<Error>> Observers { get; } = new List<IObserver<Error>>();

        public IDisposable Subscribe(IObserver<Error> observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            return new ErrorUnsubscriber(Observers, observer);
        }

        public void ReportError(Exception ex, string message) =>
            ReportError(new Error(ex, message));

        public void ReportError(Error error)
        {
            foreach (var observer in Observers)
                observer.OnNext(error);
        }

        protected void End()
        {
            foreach (var observer in Observers.ToArray())
                if (Observers.Contains(observer))
                    observer.OnCompleted();

            Observers.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
                if (disposing)
                    End();
            _disposedValue = true;
        }

        public class ErrorUnsubscriber : Unsubscriber<Error>
        {
            public ErrorUnsubscriber(IList<IObserver<Error>> errorObservers, IObserver<Error> errorObserver) : base(errorObservers, errorObserver)
            { }
        }
    }
}