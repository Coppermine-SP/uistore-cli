using System.Collections.ObjectModel;

namespace CloudInteractive.UniFiStore;

public class Product(string id, string title)
{
    public string Id { get; internal set; } = id;
    public string Title { get; internal set; } = title;
    public string Category { get; internal set; }
    public string Description { get; internal set; }
    public string ThumbnailUrl { get; internal set; }
    
    internal List<Variant> VariantsList = new List<Variant>();
    public ReadOnlyCollection<Variant> Variants => VariantsList.AsReadOnly();
    

}