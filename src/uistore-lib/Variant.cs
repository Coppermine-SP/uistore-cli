/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
namespace CloudInteractive.UniFiStore;

public class Variant
{
    internal Variant() {}
    public string Id { get; internal set; }
    public string Sku { get; internal set; }
    public uint Amount { get; internal set; }
    public string Currency { get; internal set; }
    public bool HasUiCare { get; internal set; }
    
    public uint Quantity { get; internal set; }
}