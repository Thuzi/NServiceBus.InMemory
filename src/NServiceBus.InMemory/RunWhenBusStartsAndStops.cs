namespace NServiceBus.InMemory
{
    public class RunWhenBusStartsAndStops : IWantToRunWhenBusStartsAndStops
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Start()
        {
            InMemoryDatabase.StartServer(false);
        }
        public void Stop()
        {
            InMemoryDatabase.StopServer();
        }
    }
}
