
using EcommerceMVC.Models.Data;
using EcommerceMVC.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace EcommerceMVC.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare list of PageVM
            List<PageVM> pagesList;

            using (Db db = new Db())
            {
                //Init the list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            //Return the view with list
            return View(pagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        [HttpPost]
        // POST: Admin/Pages/AddPage
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {

            
            //declare slug
            string slug;
            //init PageDTO
            PageDTO dto = new PageDTO();
            //DTO Title
            dto.Title = model.Title;
            //check for and set slug if need be
            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                slug = model.Title.Replace(" ", "-").ToLower();
            }
            else
            {
                slug = model.Slug.Replace(" ", "-").ToLower();
            }
            //make sure title and slug are unique
            if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists");
                        return View(model);
                }
                // DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                // Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            // set temp data message
            TempData["SM"] = "You have added a new page!";
            //redirect
            return RedirectToAction("AddPage");

        
        }
        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // Declare pageVM
            PageVM model;
            using( Db db = new Db())
           {
                // Get the page
                PageDTO dto = db.Pages.Find(id);
                // Confirm Page exists
                if (dto == null)
                {
                    return Content("THe page does not exist");
                }
                // Init pageVM
                model = new PageVM(dto);

            }
            // Return View with model
            return View(model);
        }
        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using( Db db = new Db())
            {
            // Get page id
            int id = model.Id;

            // Declare and Innit the slug
            string slug ="home";
            // Get the page
            PageDTO dto = db.Pages.Find(id);
                // DTO the tile
                dto.Title = model.Title;
             // Check for slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    { 
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                // Make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists");
                }
                // DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                // Save the DTO
                db.SaveChanges();
            }
            // Set template message
            TempData["SM"] = "You have edited the page!";
            // Redirect
            return RedirectToAction("EditPage");
        }
        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            // Declare PageVM
            PageVM model;
                using (Db db = new Db())
            {
                // Get the page
                PageDTO dto = db.Pages.Find(id);

                // Confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }
                // Init pageVM
                model = new PageVM(dto);
            }
            // Return view with model
            return View(model);
        }
        // GET:: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                 // Get the page
            PageDTO dto = db.Pages.Find(id);
            // Remove the page
            db.Pages.Remove(dto);
            // Save
            db.SaveChanges();
            }
            // Redirect
            return RedirectToAction("index");
        }
        // POST: Admin/Pages/ReorderPages/id
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                // Set initial count
                int count = 1;
                // Declere pageDTO
                PageDTO dto;
                // Set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }

            }
          


        }
        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            // Declare the model
            SidebarVM model;
            using (Db db = new Db())
            {
                // get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);
                // Init model
                model = new SidebarVM(dto);
            }
                // Return view with model
                return View(model);
        }
        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                // Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);
                //DTO the body
                dto.Body = model.Body;
                // Save
                db.SaveChanges();
            }
            // Set Template message
            TempData["SM"] = "You have edited the sidebar";
            // Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}