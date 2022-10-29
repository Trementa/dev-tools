using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenApi.Generator;

public class FileArtifactTracker : IObservable<FileInfo>, IDisposable
{
    protected IList<IObserver<FileInfo>> Observers { get; } = new List<IObserver<FileInfo>>();

    public IDisposable Subscribe(IObserver<FileInfo> observer)
    {
        if (!Observers.Contains(observer))
            Observers.Add(observer);
        return new Unsubscriber(Observers, observer);
    }

    public class Unsubscriber : IDisposable
    {
        protected readonly IList<IObserver<FileInfo>> Observers;
        protected readonly IObserver<FileInfo> Observer;

        public Unsubscriber(IList<IObserver<FileInfo>> observers, IObserver<FileInfo> observer) =>
            (Observers, Observer) = (observers, observer);

        public void Dispose()
        {
            if (Observer != null && Observers.Contains(Observer))
                Observers.Remove(Observer);
        }
    }

    public void TrackFile(string file) =>
        TrackFile(new FileInfo(file));

    public void TrackFile(FileInfo fileInfo)
    {
        foreach (var observer in Observers)
        {
            observer.OnNext(fileInfo);
        }
    }

    public void End()
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
        {
            if(disposing)
            {
                End();
            }
        }
        _disposedValue = true;
    }
}