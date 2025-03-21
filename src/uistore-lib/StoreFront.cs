/*
 *  Ubiquiti Store Library / uistore-lib
 *  Copyright (C) 2025 Coppermine-SP
 */
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CloudInteractive.UniFiStore;

public enum StoreRegion { US, CA, EU, JP, TW, SG, ME }

public class ProductEventArgs(Product product) : EventArgs
{
    public Product ChangedProduct { get; } = product;
}
public class ProductOutOfStockEventArgs(Product product) : ProductEventArgs(product);
public class ProductRestockEventArgs(Product product) : ProductEventArgs(product);

public class BuildIdNotFoundException : Exception;

public class StoreFront : IDisposable
{
    private const string GraphQlApiEndPoint = "https://ecomm.svc.ui.com/graphql";
    private static readonly Dictionary<StoreRegion, (string, string)> StoreRegionTable = new()
        {
            { StoreRegion.US, ("https://store.ui.com", "us")},
            { StoreRegion.CA, ("https://ca.store.ui.com", "ca")},
            { StoreRegion.EU, ("https://eu.store.ui.com", "eu")},
            { StoreRegion.JP, ("https://jp.store.ui.com", "jp")},
            { StoreRegion.TW, ("https://tw.store.ui.com", "tw")},
            { StoreRegion.SG, ("https://sg.store.ui.com", "sg")},
            { StoreRegion.ME, ("https://me.store.ui.com", "me")}
        };
    private static readonly Dictionary<string, ProductStatus> StatusStringTable = new()
    {
        { "Available", ProductStatus.Available },
        { "SoldOut", ProductStatus.SoldOut },
        { "ComingSoon", ProductStatus.ComingSoon }
    };
    private static readonly string[] CategoryList =
    [
        "all-cloud-gateways",
        "all-switching",
        "all-wifi",
        "all-cameras-nvrs",
        "all-door-access",
        "all-integrations",
        "all-advanced-hosting",
        "accessories-cables-dacs",
        "all-60ghz-wireless",
        "all-wireless",
        "all-fiber",
        "all-wired",
        "all-accessory-tech",
        "all-amplifi-alien",
        "all-amplifi-mesh"
        
    ];
    
    private readonly HttpClient _client;
    private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1,1);
    private string _buildId = "";
    private readonly List<Product> _productList = new List<Product>();
    
    public StoreRegion Region { get; }
    public ReadOnlyCollection<Product> Products => _productList.AsReadOnly();
    public event EventHandler? OnProductOutOfStocked;
    public event EventHandler? OnProductRestocked;
    
    private async Task _setBuildIdAsync()
    {
        var response = await _client.GetAsync($"{StoreRegionTable[Region].Item1}/{StoreRegionTable[Region].Item2}/en", HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var regex = new Regex("\"buildId\"\\s*:\\s*\"(?<buildId>[^\"]+)\"");
        var match = regex.Match(body);

        if (!match.Success) throw new BuildIdNotFoundException();
        _buildId = match.Groups["buildId"].Value;
    }

    private async Task<bool> _checkBuildIdAsync()
    {
        using var response = await _client.GetAsync($"{StoreRegionTable[Region].Item1}/_next/static/{_buildId}/_ssgManifest.js");
        return response.IsSuccessStatusCode;
    }
    
    public StoreFront(StoreRegion region)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36");
        this.Region = region;
    }

    public async Task InitializeAsync()
    {
        await _setBuildIdAsync();
        await UpdateCatalogAsync();
    }
    
    public async Task UpdateCatalogAsync()
    {
        await _updateLock.WaitAsync();
        try
        {
            if (!await _checkBuildIdAsync()) await _setBuildIdAsync();
            foreach (string category in CategoryList)
            {
                using var response = await _client.GetAsync(
                    $"{StoreRegionTable[Region].Item1}/_next/data/{_buildId}/{StoreRegionTable[Region].Item2}/en.json?category={category}&language=en",
                    HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode) continue;

                using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
                await using var textReader = new JsonTextReader(streamReader);
                var serializer = new JsonSerializer();

                dynamic json = serializer.Deserialize(textReader)!;
                if (json.pageProps.subCategories is null) continue;
                foreach (var subCategory in json.pageProps.subCategories)
                {
                    foreach (var product in subCategory.products)
                    {
                        var p = _productList.FirstOrDefault(y => y.Id.Equals(product.id.ToString()));
                        bool isNewObject = p is null;
                        if (isNewObject)
                        {
                            p = new Product
                            {
                                Id = product.id,
                                Name = product.name,
                                Title = product.title,
                                Category = subCategory.id,
                                Description = product.shortDescription,
                                ThumbnailUrl = product.thumbnail.url
                            };

                            foreach (var variant in product.variants)
                            {
                                p.VariantsList.Add(new Variant
                                {
                                    Id = variant.id,
                                    Sku = variant.sku,
                                    HasUiCare = variant.hasUiCare,
                                    Amount = variant.displayPrice.amount,
                                    Currency = variant.displayPrice.currency ?? "NULL"
                                });
                            }
                            _productList.Add(p);
                        }

                        ProductStatus status = StatusStringTable.TryGetValue(product.status.ToString(), out ProductStatus x) ? x : ProductStatus.ComingSoon;
                        if (!isNewObject)
                        {
                            if(p?.Status == ProductStatus.SoldOut && status == ProductStatus.Available)
                                OnProductRestocked?.Invoke(this, new ProductRestockEventArgs(p));
                            else if(p?.Status == ProductStatus.Available && status == ProductStatus.SoldOut)
                                OnProductOutOfStocked?.Invoke(this, new ProductOutOfStockEventArgs(p));
                        }

                        p!.Status = status;
                    }
                }
            }
        }
        finally
        {
            _updateLock.Release();
        }
    }

    public async Task UpdateQuantityAsync(Product p)
    {
        var serializer = new JsonSerializer();
        foreach (var v in p.VariantsList)
        {
            var request =
                $"{{\"operationName\":\"ValidateCart\",\"variables\":{{\"storeId\":\"{StoreRegionTable[Region].Item2}\",\"items\":[{{\"storeProductVariantId\":\"{v.Id}\",\"quantity\":100000}}]}},\"query\":\"query ValidateCart($storeId: StoreId!, $items: [CartItem!]!, $checkoutId: UUID) {{\\n  validateCart(storeId: $storeId, items: $items, checkoutId: $checkoutId)\\n}}\"}}";
            using var response = await _client.PostAsync(GraphQlApiEndPoint, new StringContent(request, Encoding.UTF8, "application/json"));
            
            using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            await using var textReader = new JsonTextReader(streamReader);
            
            dynamic json = serializer.Deserialize(textReader)!;
            if(json.errors[0] is null || !json.errors[0].message.ToString().Equals("Cart item(s) failed limitation(s)")) continue;

            uint quantityAllowed = Convert.ToUInt32(json.errors[0].extensions.limitationReasons[0].quantityAllowed);
            v.Quantity = quantityAllowed;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _updateLock.Dispose();
    }
}