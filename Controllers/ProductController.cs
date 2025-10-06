using Microsoft.AspNetCore.Mvc;
using ABCRetailer.Models;
using ABCRetailer.Services;


namespace ABCRetailer.Controllers
{
    public class ProductController : Controller
    {
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IAzureStorageService storageService, ILogger<ProductController> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _storageService.GetAllEntitiesAsync<Product>();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            try
            {
                // Manual price parsing
                if (Request.Form.TryGetValue("Price", out var priceFormValue))
                {
                    _logger.LogInformation("Raw price from form: '{PriceFormValue}'", priceFormValue.ToString());

                    if (double.TryParse(priceFormValue, out var parsedPrice))
                    {
                        product.Price = parsedPrice;
                        _logger.LogInformation("Successfully parsed price: {Price}", parsedPrice);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse price: {PriceFormValue}", priceFormValue.ToString());
                    }
                }

                _logger.LogInformation("Final product price: {Price}", product.Price);

                if (ModelState.IsValid)
                {
                    if (product.Price <= 0)
                    {
                        ModelState.AddModelError("Price", "Price must be greater than $0.00");
                        return View(product);
                    }

                    // Upload image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, "product-images");
                        product.ImageUrl = imageUrl;
                    }

                    await _storageService.AddEntityAsync(product);
                    TempData["Success"] = $"Product '{product.ProductName}' created successfully with price {product.Price:C}!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await _storageService.GetEntityAsync<Product>("Product", id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            try
            {
                // Manual price parsing for edit
                if (Request.Form.TryGetValue("Price", out var priceFormValue))
                {
                    if (decimal.TryParse(priceFormValue, out var parsedPrice))
                    {
                        product.Price = (double)parsedPrice;
                        _logger.LogInformation("Edit successfully parsed price: {Price}", parsedPrice);
                    }
                }

                if (ModelState.IsValid)
                {
                    // Get the original product (to preserve ETag)
                    var originalProduct = await _storageService.GetEntityAsync<Product>("Product", product.RowKey);
                    if (originalProduct == null)
                        return NotFound();

                    originalProduct.ProductName = product.ProductName;
                    originalProduct.Description = product.Description;
                    originalProduct.Price = product.Price;
                    originalProduct.StockAvailable = product.StockAvailable;

                    // Upload new image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _storageService.UploadImageAsync(imageFile, "product-images");
                        originalProduct.ImageUrl = imageUrl;
                    }

                    await _storageService.UpdateEntityAsync(originalProduct);
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _storageService.DeleteEntityAsync<Product>("Product", id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
