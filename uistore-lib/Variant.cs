/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
namespace CloudInteractive.UniFiStore;

public struct Variant
{
    public string Id { get; internal set; }
    public string Sku { get; internal set; }
    public int Amount { get; internal set; }
    public string Currency { get; internal set; }
    public bool HasUiCare { get; internal set; }
    public int Quantity { get; internal set; }
}