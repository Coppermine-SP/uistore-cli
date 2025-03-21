using System.Diagnostics;

namespace CloudInteractive.UniFiStore.Commands;

public static partial class Commands
{
    private static readonly Dictionary<string, StoreRegion> RegionTable = new()
    {
        { "US", StoreRegion.US },
        { "CA", StoreRegion.CA },
        { "EU", StoreRegion.EU },
        { "JP", StoreRegion.JP },
        { "TW", StoreRegion.TW },
        { "SG", StoreRegion.SG },
        { "ME", StoreRegion.ME }
    };
    
    [Command("init", Description = "Initializes the StoreFront for the specified region.")]
    public static void Init(string region)
    {
        if (!RegionTable.ContainsKey(region.ToUpper()))
        {
            Console.WriteLine("Invalid store region. (Available regions: US, CA, EU, JP, TW, SG, ME)");
            return;
        }
        
        if(Program.StoreFront is not null) Program.StoreFront.Dispose();
        Program.StoreFront = new StoreFront(RegionTable[region]);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine($"Fetching product catalog from {region.ToUpper()} Ubiquiti Store..");
        try
        {
            Program.StoreFront.InitializeAsync().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in InitializeAsync(): " + e);
            Program.StoreFront.Dispose();
            Program.StoreFront = null;
        }
        finally
        {
            stopwatch.Stop();
        }

        Console.WriteLine($"OK. ({Program.StoreFront?.Products.Count} items, took {stopwatch.ElapsedMilliseconds}ms.)");
    }
}