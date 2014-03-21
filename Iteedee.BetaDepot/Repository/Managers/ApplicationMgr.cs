using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository.Managers
{
    public static class ApplicationMgr
    {

        public static TeamMember GetSystemCIUser()
        {
            TeamMember retval;
            using (var context = new Repository.BetaDepotContext())
            {
                retval = context.TeamMembers.Where(w => w.UserName == Common.Constants.APPLICATION_TEAM_MEMBER_CI_USER_NAME).FirstOrDefault();
            }
            return retval;
        }
    }
}