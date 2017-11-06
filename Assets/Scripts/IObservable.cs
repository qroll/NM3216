public interface IObservable
{

    void Subscribe(IObserver observer);

    void NotifyAll(EventType e);

}
