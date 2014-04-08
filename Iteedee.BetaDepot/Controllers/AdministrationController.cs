using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Iteedee.BetaDepot.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Iteedee.BetaDepot.Controllers
{
    [Authorize(Roles=Common.Constants.SYSTEM_ROLE_ADMINISTRATOR)]
    public class AdministrationController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Users()
        {
            Administration.UsersViewModel mdl = new Administration.UsersViewModel();

            List<IdentityUser> userLogins;
            List<Repository.TeamMember> memberships;
            using(var usrContext = new IdentityDbContext())
            {
                userLogins = usrContext.Users.Include("Roles.Role").ToList();
                
            }

            using(var context = new Repository.BetaDepotContext())
            {
                memberships = context.TeamMembers.Include("UserMemberships").Where(w => w.IsSystemUser == false).ToList();
            }


            var rm = new RoleManager<IdentityRole>(
               new RoleStore<IdentityRole>(new ApplicationDbContext()));
            foreach (var u in userLogins)
            {
                Repository.TeamMember member = memberships.Where(w => w.UserName == u.UserName).FirstOrDefault();
                if(member != null)
                {
                    mdl.AppUsers.Add(new Administration.UsersViewModel.Users()
                    {
                        AppMemberCount = member.UserMemberships.Count(),
                        EmailAddress = member.EmailAddress,
                        UserName = member.UserName,
                        SystemRole = u.Roles.Select(s => s.Role.Name).Contains(Common.Constants.SYSTEM_ROLE_ADMINISTRATOR) ? Common.Constants.SYSTEM_ROLE_ADMINISTRATOR : Common.Constants.SYSTEM_ROLE_USER,
                        Name = string.Format("{0} {1}", member.FirstName, member.LastName),
                        GravitarUrl = string.Format("http://www.gravatar.com/avatar/{0}?s=36", Common.Functions.GenerateMD5Hash(member.EmailAddress.ToLower()))
                    }); 
                }
            }

            return View(mdl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateSystemRole(string userName, string roleToUpdate)
        {
            var userMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            userMgr.UserValidator = new UserValidator<ApplicationUser>(userMgr) { AllowOnlyAlphanumericUserNames = false };
            var user = userMgr.FindByName(userName);
            userMgr.RemoveFromRole(user.Id, Common.Constants.SYSTEM_ROLE_USER);
            userMgr.RemoveFromRole(user.Id, Common.Constants.SYSTEM_ROLE_ADMINISTRATOR);
            var roleresult = userMgr.AddToRole(user.Id, roleToUpdate);
            if (roleresult.Succeeded)
            {
                return Json(new
                {
                    Status = "OK",
                    Msg = ""
                });
            }
            else
            {
                return Json(new { Status = "ERROR", Message = "Unable to add user to role" });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePassword(string userName, string NewPassword)
        {
            var userMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            userMgr.UserValidator = new UserValidator<ApplicationUser>(userMgr) { AllowOnlyAlphanumericUserNames = false };
            var user = userMgr.FindByName(userName);
            String hash = userMgr.PasswordHasher.HashPassword(NewPassword);
            //UserStore<ApplicationUser> store = new UserStore<ApplicationUser>();
            user.PasswordHash = hash;
            userMgr.Update(user);
            //store.UpdateAsync(user).Wait();

            return Json(new
            {
                Status = "OK",
                Msg = ""
            });
        }

	}
}