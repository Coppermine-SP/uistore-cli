/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
using System.Collections.ObjectModel;

namespace CloudInteractive.UniFiStore;

public struct Product()
{
    public string Id { get; internal set; }
    public string Title { get; internal set; }
    public string Category { get; internal set; }
    public string Description { get; internal set; }
    public string ThumbnailUrl { get; internal set; }
    
    internal List<Variant> VariantsList = new List<Variant>();
    public ReadOnlyCollection<Variant> Variants => VariantsList.AsReadOnly();
    

}