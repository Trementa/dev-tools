using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenApi.Generator
{
    public class ErrorTracker : IObservable<Error>
    {
        protected IList<IObserver<Error>> Observers { get; } = new List<IObserver<Error>>();

        public IDisposable Subscribe(IObserver<Error> observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            return new Unsubscriber(Observers, observer);
        }

        public class Unsubscriber : IDisposable
        {
            protected readonly IList<IObserver<Error>> Observers;
            protected readonly IObserver<Error> Observer;

            public Unsubscriber(IList<IObserver<Error>> observers, IObserver<Error> observer) =>
                (Observers, Observer) = (observers, observer);

            public void Dispose()
            {
                if (Observer != null && Observers.Contains(Observer))
                    Observers.Remove(Observer);
            }
        }

        public void ReportError(Exception ex, string message) =>
            ReportError(new Error(ex, message));

        public void ReportError(Error error)
        {
            foreach (var observer in Observers)
            {
                observer.OnNext(error);
            }
        }

        public void End()
        {
            foreach (var observer in Observers.ToArray())
                if (Observers.Contains(observer))
                    observer.OnCompleted();

            Observers.Clear();
        }
    }
}