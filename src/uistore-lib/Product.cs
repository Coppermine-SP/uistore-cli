/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
using System.Collections.ObjectModel;
using System.Text;

namespace CloudInteractive.UniFiStore;
public enum ProductStatus { Available, SoldOut, ComingSoon }
public class Product
{
    internal Product() { }

    public string Id { get; internal init; }
    public string Name { get; internal init; }
    public string Title { get; internal init; }
    public string Category { get; internal init; }
    public string Description { get; internal init; }
    public string ThumbnailUrl { get; internal init; }
    
    public ProductStatus Status { get; internal set; }

    internal readonly List<Variant> VariantsList = new List<Variant>();
    public ReadOnlyCollection<Variant> Variants => VariantsList.AsReadOnly();

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder($"{Title}\n");
        foreach (Variant v in VariantsList)
            builder.Append(
                $"   - {v.Sku}, AvailableQuantity:{v.Quantity}, Price:{(v.Amount / 100.0).ToString("F2")}{v.Currency}, HasUICare:{v.HasUiCare.ToString()}\n");
        return builder.ToString();
    }
}