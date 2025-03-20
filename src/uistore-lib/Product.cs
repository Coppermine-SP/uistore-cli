/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Text;

namespace CloudInteractive.UniFiStore;

public class Product
{
    internal Product()
    {
    }

    public string Id { get; internal set; }
    public string Name { get; internal set; }
    public string Title { get; internal set; }
    public string Category { get; internal set; }
    public string Description { get; internal set; }
    public string ThumbnailUrl { get; internal set; }

    internal List<Variant> VariantsList = new List<Variant>();
    public ReadOnlyCollection<Variant> Variants => VariantsList.AsReadOnly();

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder($"{Title}\n");
        foreach (Variant v in VariantsList)
            builder.Append(
                $"   - {v.Sku}, Avaliable:{v.Quantity}, Price:{(v.Amount / 100.0).ToString("F2")}{v.Currency}, HasUICare:{v.HasUiCare.ToString()}\n");
        builder.Append("\n");

        return builder.ToString();
    }
}