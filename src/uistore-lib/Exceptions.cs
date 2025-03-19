/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
namespace CloudInteractive.UniFiStore;

public class BuildIdException(string message) : Exception(message);

public class CatalogUpdateException(string message, Exception inner) : Exception(message, inner);