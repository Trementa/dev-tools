using System;
using System.Collections.Generic;
using System.IO;

namespace Trementa.OpenApi.Generator.Tracking;

public class FileArtifactTracker : IObservable<FileInfo>, IDisposable
{
    public FileArtifactTracker(IEnumerable<IObserver<FileInfo>> fileObservers)
        => Observers.AddRange(fileObservers);

    protected List<IObserver<FileInfo>> Observers { get; } = new List<IObserver<FileInfo>>();

    public IDisposable Subscribe(IObserver<FileInfo> fileObserver)
    {
        if (!Observers.Contains(fileObserver))
            Observers.Add(fileObserver);
        return new FileUnsubcriber(Observers, fileObserver);
    }

    public void TrackFile(string file) =>
        TrackFile(new FileInfo(file));

    public void TrackFile(FileInfo fileInfo)
    {
        foreach (var observer in Observers)
            observer.OnNext(fileInfo);
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

    public class FileUnsubcriber : Unsubscriber<FileInfo>
    {
        public FileUnsubcriber(IList<IObserver<FileInfo>> fileObservers, IObserver<FileInfo> fileObserver) : base(fileObservers, fileObserver)
        { }
    }
}
