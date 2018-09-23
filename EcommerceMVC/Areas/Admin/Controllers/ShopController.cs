using EcommerceMVC.Models.Data;
using EcommerceMVC.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace EcommerceMVC.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // Declare a list of models
            List<CategoryVM> categoryVMList;
            using(Db db = new Db())
            {
                // Init the list
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryVM(x))
                                .ToList();
            }

            return View(categoryVMList);
        }
        //POST: Admin/Shop/AddNewCategories
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Declare Id
            string id;

            using( Db db = new Db())
            {
                // Check that the category is unique
                if (db.Categories.Any( x => x.Name == catName))
                {
                    return "titletaken";
                }
                // Init DTO
                CategoryDTO dto = new CategoryDTO();

                // Add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                // Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                // Get the Id
                id = dto.Id.ToString();
            }
            // Return the ID
            return id;
           
        }
        // POST: Admin/Shop/ReorderPages/id
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // Set initial count
                int count = 1;
                // Declere pageDTO
                CategoryDTO dto;
                // Set sorting for each category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }

            }



        }
        // GET:: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // Get the page
                CategoryDTO dto = db.Categories.Find(id);
                // Remove the page
                db.Categories.Remove(dto);
                // Save
                db.SaveChanges();
            }
            // Redirect
            return RedirectToAction("Categories");
           

        }
        // POST:: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName,int id)
        {
            using (Db db = new Db())
            {
                // Check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";
                // Get the DTO
                CategoryDTO dto = db.Categories.Find(id);
                // Edit the DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ","-").ToLower();
                // Save
                db.SaveChanges();
            }
            // Return random string
            return "ok";
        }
        // GET:: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Init model
            ProductVM model = new ProductVM();

            //Add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }
            // Return view with model
            return View(model);
        }
        // POST:: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                using (Db db =new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                    return View(model);
                }
               
            }
            // Make sure the product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x=> x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }
            //  Declare product Id
            int id;
            // Init and save productDTO
            using (Db db = new Db())
            {
                ProductsDTO product = new ProductsDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;
                db.Products.Add(product);
                db.SaveChanges();

                //Get the id
                id = product.id;
            }

            // Set TempData message
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            // Create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            // Check if a  filie was uploaded
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() );
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);
            // Check if a file was uploaded
            if (file!= null && file.ContentLength > 0)
            {

            // Get the file extension
            string ext = file.ContentType.ToLower();

            // Verify extension
            if (ext != "image/jpg" &&
                ext != "image/jpeg" &&
                ext != "image/pjpeg" &&
                ext != "image/gif" &&
                 ext != "image/x-png" &&
                ext != "image/png" )
            { 
                 using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                    ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                    return View(model);   
                }
            }
            // Init image name
            string imageName = file.FileName;

            //Save image name to DTO
            using (Db db = new Db())
            {
                ProductsDTO dto = db.Products.Find(id);
                dto.ImageName = imageName;
                db.SaveChanges();
            }

            // set original and thumb image paths
            var path = string.Format("{0}\\{1}", pathString2, imageName);
            var path2 = string.Format("{0}\\{1}", pathString3, imageName);


            // Save original
            file.SaveAs(path);

            // Create and save thumb

            WebImage img = new WebImage(file.InputStream);
            img.Resize(200, 200);
            img.Save(path2);
            }
            #endregion

            // Redirect
            return RedirectToAction("AddProduct");
        }
        // GET:: Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            // Declare a list of ProductVM
            List<ProductVM> listOfProductVM;

            // Set page number
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            using( Db db = new Db())
            {
                // Init the list
                listOfProductVM = db.Products.ToArray()
                                  .Where(x => catId == null || x.CategoryId == catId)
                                  .Select(x => new ProductVM(x))
                                  .ToList();
                // Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }
            // Set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3); // will only contain 25 products max because of the pageSize
            ViewBag.OnePageOfProducts = onePageOfProducts;
            //Return view with list
            return View(listOfProductVM);
        }
        [HttpGet]
        // GET:: Admin/Shop/EditProduct/id
        public ActionResult EditProduct(int id)
        {
            // Declare the productVM
            ProductVM model;
            using(Db db = new Db())
            {
                // Get the product
                ProductsDTO dto = db.Products.Find(id);

                //Make sure the product exists
                if (dto == null)
                {
                    return Content("That product does not exist.");
                }
                // init the model
                model = new ProductVM(dto);

                // Make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");

                // Get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));

            }

            // Return View with model
            return View(model);
        }
        [HttpPost]
        // POST:: Admin/Shop/EditProduct/id
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Get product id
            int id = model.id;
            // Populate categories select list and gallery images
            using  (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));

            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Make sure product name is unique
            using (Db db = new Db())
            {
                if ( db.Products.Where(x => x.id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }
            // Update product
            using (Db db = new Db())
            {
                ProductsDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;
                db.SaveChanges();
            }
            // Set TempData message
            TempData["SM"] = "You have edited the product!";

            #region Image Upload

            // Check for file upload
            if (file != null && file.ContentLength > 0)
            {
                // Get extension
                string ext = file.ContentType.ToLower();

                // Verify Extension
                if (ext != "image/jpg" &&
               ext != "image/jpeg" &&
               ext != "image/pjpeg" &&
               ext != "image/gif" &&
                ext != "image/x-png" &&
               ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }
                // Set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                // Check if a  filie was uploaded

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
                // Delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);
                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();
                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();
                // Save Image aname
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductsDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                // Save original and thum images
                          var path = string.Format("{0}\\{1}", pathString1, imageName);
            var path2 = string.Format("{0}\\{1}", pathString2, imageName);
            // Save original
            file.SaveAs(path);

            // Create and save thumb

            WebImage img = new WebImage(file.InputStream);
            img.Resize(200, 200);
            img.Save(path2);
            }
            #endregion

            // Redirect
            return RedirectToAction("EditProduct");
        }
        // GET:: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            // Delete product from DB
            using (Db db = new Db())
            {
                ProductsDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            // Delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            if (Directory.Exists(pathString))
                Directory.Delete(pathString,true);
            // Redirect
            return RedirectToAction("Products");
        }
        // POST:: Admin/Shop/SaveGalleryImages
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {
            // loop through the files
            foreach(string fileName in Request.Files)
            {
                // Init the file
                HttpPostedFileBase file = Request.Files[fileName];
                // Check if its not null
                if ( file != null && file.ContentLength > 0)
                {
                    // Set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");
                    // Set image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);
                    // Save original and thumb
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }



            }
            return View();
        }
        // POST:: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string ImageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + ImageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + ImageName);

          
            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

             if (System.IO.File.Exists(fullPath2))
             System.IO.File.Delete(fullPath2);
 
        }
    }
}