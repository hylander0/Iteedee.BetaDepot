using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Iteedee.BetaDepot.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Iteedee.BetaDepot.Controllers
{
    public class SharedController : Controller
    {
        public PartialViewResult TopNav()
        {
            Models.TopMenuViewModel mdl = new Models.TopMenuViewModel();
            String userName = User.Identity.GetUserName().ToLower();
            var userMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var user = userMgr.FindByName(userName);


            using (var context = new Iteedee.BetaDepot.Repository.BetaDepotContext())
            {
                var member = context.TeamMembers.Where(w => w.UserName.ToLower() == userName).FirstOrDefault();
                if (member != null)
                    mdl.IsInSystemRole = userMgr.IsInRole(user.Id, Common.Constants.SYSTEM_ROLE_ADMINISTRATOR);
            }

            return PartialView(mdl);

        }
	}
}