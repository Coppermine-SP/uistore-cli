# UIStore-CLI
<img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"> <img src="https://img.shields.io/badge/UniFi-0559C9?style=for-the-badge&logo=ubiquiti&logoColor=white">

**C# Library and Simple CLI App for Ubiquiti Store**

### Table of Content
- [Overview](#overview)
- [How to Use (CLI App)](#how-to-use-cli-app)
- [How to Use (Library)](#how-to-use-library)
- [Dependencies](#dependencies)

## Overview

## How to Use (CLI App)

## How to Use (Library)
### Getting Start
- Make a StoreFront instance with your region.
```csharp
var storeFront = new StoreFront(StoreRegion.US)
```

- Intitialize and Fetch item list first time.
```csharp
storeFront.InitializeAsync().Wait();
```
- - -
### Get product instance
- Get a specific product instance by LINQ
```csharp
var product = storeFront.Products.FirstOrDefault(x => x.Name.Equals("UDM-Pro"));
```
- - -
### Update available quantity for a specific product
> [!WARNING]
> **Default available quantity is 0.**
> 
> Update only the items for which you need to retrieve available quantities; Fetching quantities for all store items (400+ items) may impose a heavy load on the GraphQL Server.

- Fetching available quantity for a specific product from Ubiquiti Store.
```csharp
storeFront.UpdateQuantityAsync(product).Wait();
```
- - -
### Update product catalog
- Fetching item list from Ubiquiti Store.
```csharp
storeFront.UpdateCatalogAsync().Wait();
```

## Dependencies
### UIStore-Lib
- **Newtonsoft.Json** - 13.0.3

### UIStore-CLI

