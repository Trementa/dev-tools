using System;
using System.Collections.Generic;

namespace OpenApi.Generator.CSharp.Tracking;

public class Unsubscriber<T> : IDisposable
{
    protected readonly IList<IObserver<T>> Observers;
    protected readonly IObserver<T> Observer;

    public Unsubscriber(IList<IObserver<T>> observers, IObserver<T> observer) =>
        (Observers, Observer) = (observers, observer);

    public void Dispose()
    {
        if (Observer != null && Observers.Contains(Observer))
            Observers.Remove(Observer);
    }
}
