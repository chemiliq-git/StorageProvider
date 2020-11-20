using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataContext.DataContext;
using DataContext.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using DataContext;
using System.Net.Http;
using ServiceLayer;
using Microsoft.Extensions.Configuration;
using ServiceLayer.RemoteStorage;
using System.Drawing.Imaging;
using System.Drawing;
using ServiceLayer.ImageValidate;

namespace BlobStorage_File_Upload.Controllers
{
    public class ProductsController : Controller
    {
        private UnitOfWork _context;

        private readonly IRemoteStorageService storegeServer;
        private readonly IImageValidatorService imageValidator;

        public ProductsController(IRemoteStorageService storegeServer, IImageValidatorService imageValidator)
        {
            this._context = new UnitOfWork();
            this.storegeServer = storegeServer;
            this.imageValidator = imageValidator;
        }

        // GET: Products
        public IActionResult Index()
        {
            return View(_context.ProductRepo.GetAll());
        }

        // GET: Products/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.ProductRepo.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {

                #region Read File Content
                if (product.File != null)
                {

                    ImageValidateResult result = imageValidator.ValidateImageFile(product.File);
                    if(!result.IsValid)
                    {
                        ModelState.AddModelError(nameof(Product.File), "Not valid image format");
                        return View(product);
                    }
                    string fileName = Path.GetFileName(product.File.FileName);
                    byte[] fileData;
                    using (var target = new MemoryStream())
                    {
                        product.File.CopyTo(target);
                        fileData = target.ToArray();
                    }

                    //var fileStream = new FileStream(Path.Combine(uploads, product.File.FileName), FileMode.Create);
                    string mimeType = product.File.ContentType;
                    //= new byte[product.File.Length];

                    StoreFileInfo info = await storegeServer.UploadFile(new UploadDataInfo(product.File, @"WebPictures", "ProductsIcon"));
                    if (info.BoolResult)
                    {
                        product.ImagePath = info.FileAddress;
                        product.ImageId = info.FileId;
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(product.File), "Error uploading file to storage");
                        return View(product);
                    }
                }
                #endregion

                _context.ProductRepo.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.ProductRepo.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ProductId,Name,UnitPrice,Description,ImageName,ImagePath,CreatedDate,UpdatedDate")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ProductRepo.Modify(product);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.ProductRepo.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = _context.ProductRepo.GetById(id);
            await storegeServer.DeleteData(new StoreFileInfo(product.ImagePath, product.ImageId));
            _context.ProductRepo.Delete(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.ProductRepo.GetAll().Any(e => e.ProductId == id);
        }
    }
}
