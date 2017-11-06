public interface IObserver
{

    void Notify(EventType e);

}

public enum EventType
{
    MUTE, UNMUTE
}
