using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModel;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;
        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db, IHostingEnvironment host)
        {
            _db = db;
            _hostingEnvironment = host;
            MenuItemVM = new MenuItemViewModel()
            {
                MenuDetails = new Models.MenuDetails(),
                Category = _db.Category
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuitems = await _db.MenuDetails.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuitems);
        }

        //GET CREATE
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        //Create POST
        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuDetails.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }
            _db.MenuDetails.Add(MenuItemVM.MenuDetails);
            await _db.SaveChangesAsync();

            string webrootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var manuItemfromDb = await _db.MenuDetails.FindAsync(MenuItemVM.MenuDetails.Id);
            if (files.Count > 0)
            {
                var uploads = Path.Combine(webrootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);
                using (var filestream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuDetails.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                manuItemfromDb.Image = @"\images\" + MenuItemVM.MenuDetails.Id + extension;
            }
            else
            {
                var uploads = Path.Combine(webrootPath, @"images\" + SD.DefaultMenuDetailsImage);
                System.IO.File.Copy(uploads, webrootPath + @"\images\" + MenuItemVM.MenuDetails.Id + ".png");
                manuItemfromDb.Image = @"\images\" + MenuItemVM.MenuDetails.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET EID
        public async Task<IActionResult> Edit(int? id)
        {
            MenuItemVM.MenuDetails = await _db.MenuDetails.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuDetails.CategoryId).ToListAsync();

            if (MenuItemVM.MenuDetails == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //Edit POST
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuDetails.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuDetails.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            //Work on the image saving section

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuDetails.FindAsync(MenuItemVM.MenuDetails.Id);

            if (files.Count > 0)
            {
                //New Image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original file
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //we will upload the new file
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuDetails.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuDetails.Id + extension_new;
            }

            menuItemFromDb.Name = MenuItemVM.MenuDetails.Name;
            menuItemFromDb.Description = MenuItemVM.MenuDetails.Description;
            menuItemFromDb.Price = MenuItemVM.MenuDetails.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuDetails.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuDetails.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuDetails.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET : Details MenuItem
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuDetails = await _db.MenuDetails.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuDetails == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //GET : Delete MenuItem
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuDetails = await _db.MenuDetails.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuDetails == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //POST Delete MenuItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            var menuItem = await _db.MenuDetails.FindAsync(id);

            if (menuItem != null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuDetails.Remove(menuItem);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }
    }
}