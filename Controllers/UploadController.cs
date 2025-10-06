using ABCRetailer.Models;
using ABCRetailer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailer.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public UploadController(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET: Upload
        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        // POST: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                {
                    // Upload to blob storage
                    var fileName = await _storageService.UploadFileAsync(model.ProofOfPayment, "payment-proofs");

                    // Also upload to file share for contracts/payments
                    await _storageService.UploadFileShareAsync(model.ProofOfPayment, "contracts", "payments");

                    TempData["Success"] = $"File uploaded successfully! File name: {fileName}";

                    // Reset the form
                    return View(new FileUploadModel());
                }
                else
                {
                    ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
            }

            return View(model);
        }
    }
}
