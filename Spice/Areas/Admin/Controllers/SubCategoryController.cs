﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; }
        public SubCategoryController(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }
        //GET Index
        public async Task<IActionResult> Index()
        {
            var subcategory = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subcategory);
        }

        
        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subcategory = await (from SubCategory in _db.SubCategory
                                             where SubCategory.CategoryId == id
                                             select SubCategory).ToListAsync();
            return Json(new SelectList(subcategory, "Id", "Name"));
        }

        //GET CREATE

        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel subcategoryandcategory = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name)
                .Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(subcategoryandcategory);
        }

        //POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subcategoryexist = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId);
                if (subcategoryexist.Count() > 0)
                {
                    StatusMessage = "Error : Sub category already exist with the name " + model.SubCategory.Name + " .Please use other sub catergory name.";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel vm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(vm);

        }

        //GET DETAILS

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var editesubcategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if (editesubcategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel subcategoryandcategory = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = editesubcategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name)
                .Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(subcategoryandcategory);
        }

        //GET Delete

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var editesubcategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if (editesubcategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel subcategoryandcategory = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = editesubcategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name)
                .Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(subcategoryandcategory);
        }
        //GET Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var editesubcategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if (editesubcategory == null)
            {
                return NotFound();
            }
            _db.SubCategory.Remove(editesubcategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET EDIT

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var editesubcategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if(editesubcategory==null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel subcategoryandcategory = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = editesubcategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name)
                .Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(subcategoryandcategory);
        }

        //POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subcategoryexist = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId);
                if (subcategoryexist.Count() > 0)
                {
                    StatusMessage = "Error : Sub category already exist with the name " + model.SubCategory.Name + " .Please use other sub catergory name.";
                }
                else
                {
                    var updatedsubcategory = await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    updatedsubcategory.Name = model.SubCategory.Name;
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                    
                }
            }
            SubCategoryAndCategoryViewModel vm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(vm);

        }
    }
}