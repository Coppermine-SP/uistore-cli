using System.Diagnostics;

namespace CloudInteractive.UniFiStore.Commands;

public static partial class Commands
{
    [Command("update", Description = "Update product state from Ubiquiti Store.")]
    public static void Update()
    {
        if (Program.StoreFront is null)
        {
            Console.WriteLine("StoreFront is not initialized.");
            return;
        }
        
        Console.WriteLine($"Fetching product catalog from {Program.StoreFront.Region.ToString()} Ubiquiti Store..");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            Program.StoreFront.UpdateCatalogAsync().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in UpdateCatalogAsync(): " + e);
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