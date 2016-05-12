namespace NServiceBus.InMemory
{
    public class RunWhenBusStartsAndStops : IWantToRunWhenBusStartsAndStops
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Start()
        {
            InMemoryDatabase.StartServer();
        }
        public void Stop()
        {
            InMemoryDatabase.StopServer();
        }
    }
}
