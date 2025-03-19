namespace CloudInteractive.UniFiStore;

class Program
{
    static void Main(string[] args)
    {
        var storeFront = new StoreFront(StoreRegion.JP);
        storeFront.InitializeAsync().Wait();
    }
}