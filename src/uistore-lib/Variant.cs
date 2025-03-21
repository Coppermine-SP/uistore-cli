/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
namespace CloudInteractive.UniFiStore;

public class Variant
{
    internal Variant() {}
    public string Id { get; internal init; }
    public string Sku { get; internal init; }
    public uint Amount { get; internal init; }
    public string Currency { get; internal init; }
    public bool HasUiCare { get; internal init; }
    
    public uint Quantity { get; internal set; }
}