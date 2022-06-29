using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MVCManukauTech.Areas.Identity.Data;
using MVCManukauTech.Models.DB;
using Microsoft.Data.SqlClient;
using MVCManukauTech.ViewModels;

namespace MVCManukauTech.Controllers
{
    // Forces log in on all functions in controller.
    [Authorize]
    public class MembershipsController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<MVCManukauTechUser> _userManager;
        public MembershipsController(RoleManager<IdentityRole> roleManager, UserManager<MVCManukauTechUser> userManager)
        {
            this.roleManager = roleManager;
            this._userManager = userManager;
        }

        public IActionResult Index()
        {
            // YLDA 25/04/2020 Grabs currently logged in user's context.
            var userid = _userManager.GetUserId(HttpContext.User);

            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Index(AspNetUsers userDetails)
        //{
        //    foreach (AspNetUsers user)
        //    var user = new AspNetUsers { UserName = userDetails.UserName, Email = userDetails.Email, con };

        //    IdentityResult x = await _userManager.UpdateAsync(userDetails);
        //}

        //public IActionResult toBronze()
        //{
        //    var userid = _userManager.GetUserId(HttpContext.User);
        //    string sql = @"
        //    SELECT * FROM AspNetUsers 
        //    WHERE Id =" + userid +
        //    " UPDATE AspNetUsers  " +
        //    "Set MembershipTypeId = 2";

        //    SqlCommand command = new SqlCommand()
        //    var report = _userManager.
        //    return success;
        //}

        [HttpGet]
        public async Task<IActionResult> MembershipChangeAsync(ConfirmPurchaseViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            var userid = _userManager.GetUserId(HttpContext.User);

            if (role == null)
            {
                ViewBag.ErrorMessage = "Role Not Found";

                return View("NotFound");
            }
            else
            {
                role.Name = model.MembershipName;

                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListAllRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MembershipChangeAsyn(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = "User Not Found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in _userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> MembershipChange(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            var userid = _userManager.GetUserId(HttpContext.User);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

                var user = await _userManager.FindByIdAsync(userid);

                IdentityResult result = null;

                if (!await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }


                if (result.Succeeded)
                {
                        return RedirectToAction("Index", "Memberships");
                }

            return RedirectToAction("Index", "Memberships");
        }
    }
}