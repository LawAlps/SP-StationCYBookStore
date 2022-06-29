using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCManukauTech.Models.DB;
using MVCManukauTech.ViewModels;
using Newtonsoft.Json;
//CRC 29/3/20 use this statement for paging
using ReflectionIT.Mvc.Paging;
// YLA 24/04/20 Authorization
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace MVCManukauTech.Controllers
{
    //CRC 17/3/20 authorize code on create and edit methods, only for admin
    //[Authorize(Roles = "Admin")]
    public class CatalogController : Controller
    {

        private readonly StationCYContext _context;
        public CatalogController(StationCYContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CheckUserVerification()
        {
            //CRC 17/3/20 check for user role verification
            if (!User.IsInRole("Admin"))
            {
                //only allows action name and controller name
                return Redirect("~/Identity/Account/Login");
                //return RedirectToAction("Index", "Home");
            }

            var northwindContext = _context.Product.Include(p => p.Category);
            return View(await northwindContext.ToListAsync());
        }

        //CRC 29/3/20 demonstrate displaying products with a maximum of 6 on a page.
        //Seperate number of items from database into multiple pages
        //Reference from https://www.reflectionit.nl/blog/2017/paging-in-asp-net-core-mvc-and-entityframework-core
        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.Product.Include(p => p.Category).AsNoTracking().OrderBy(c => c.Category);
            var model = await PagingList.CreateAsync(query, 6, page);//track how many items are required on a page.
            return View(model);
        }

        //CRC 29/3/20 to avoid clash between "index" name, I changed name below to "MyIndex"
        // GET: Catalog?CategoryName=Psychology
        public IActionResult MyIndex()
        {
            //140903 JPC add CategoryName to SELECT list of fields
            string SQL = "SELECT ProductId, Product.CategoryId AS CategoryId, Name, ImageFileName, UnitCost"
                + ", SUBSTRING(Description, 1, 100) + '...' AS Description, CategoryName "
                + "FROM Product INNER JOIN Category ON Product.CategoryId = Category.CategoryId ";
            string categoryName = Request.Query["CategoryName"];

            if (categoryName != null)
            {
                //140903 JPC security check - if ProductId is dodgy then return bad request and log the fact 
                //  of a possible hacker attack.  Excessive length or containing possible control characters
                //  are cause for concern!  TODO move this into a separate reusable code method with more sophistication.
                if (categoryName.Length > 20 || categoryName.IndexOf("'") > -1 || categoryName.IndexOf("#") > -1)
                {
                    //TODO Code to log this event and send alert email to admin
                    return BadRequest(); // Http status code 400
                }

                //140903 JPC  Passed the above test so extend SQL
                //150807 JPC Security improvement @p0
                SQL += " WHERE CategoryName = @p0";
                //SQL += " WHERE CategoryName = '{0}'";
                //SQL = String.Format(SQL, CategoryName);
                //Send extra info to the view that this is the selected CategoryName
                ViewBag.CategoryName = categoryName;
            }

            //150807 JPC Security improvement implementation of @p0
            var products = _context.CatalogViewModel.FromSqlRaw(SQL, categoryName);
            return View(products.ToList());
        }

        // GET: Catalog/Details?ProductId=1MORE4ME
        public IActionResult Details(string ProductId)
        {
            if (ProductId == null)
            {
                return BadRequest(); // Http status code 400
            }
            //140903 JPC security check - if ProductId is dodgy then return bad request and log the fact 
            //  of a possible hacker attack.  Excessive length or containing possible control characters
            //  are cause for concern!  TODO move this into a separate reusable code method with more sophistication.
            if (ProductId.Length > 20 || ProductId.IndexOf("'") > -1 || ProductId.IndexOf("#") > -1)
            {
                //TODO Code to log this event and send alert email to admin
                return BadRequest(); // Http status code 400
            }

            //150807 JPC Security improvement implementation of @p0
            //20180312 JPC change to query based on class CatalogViewModel
            //  Change above code to give all of the description rather than summarising with the first 100 characters
            string SQL = "SELECT ProductId, Product.CategoryId AS CategoryId, Name, ImageFileName, UnitCost"
            + ", Description, CategoryName "
            + "FROM Product INNER JOIN Category ON Product.CategoryId = Category.CategoryId "
            + " WHERE ProductId = @p0";

            //140904 JPC case of one product to look at the details.
            //  SQL gives some kind of collection where we need to clean that up with ToList() then take element [0]
            //150807 JPC Security improvement implementation of @p0 substitute ProductId
            var product = _context.CatalogViewModel.FromSqlRaw(SQL, ProductId).ToList()[0];
            if (product == null)
            {
                return NotFound(); //Http status code 404
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]

        public async Task<IActionResult> Edit(string id, [Bind("ProductId,CategoryId,Name,ImageFileName,UnitCost,Description,IsDownload,DownloadFileName")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductsExists(product.ProductId))
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        private bool ProductsExists(string id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}