public interface IObservable
{

    void Subscribe(IObserver observer);

    void Unsubscribe(IObserver observer);

    void NotifyAll(EventType e);

}
