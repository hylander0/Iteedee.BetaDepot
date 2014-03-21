using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Common
{
    public class Constants
    {
        public const string BUILD_PLATFORM_IOS = "IOS";
        public const string BUILD_PLATFORM_ANDROID = "ANDROID";

        public const string BUILD_ENVIRONMENT_DEVELOPMENT = "Development";
        public const string BUILD_ENVIRONMENT_TEST = "Test";
        public const string BUILD_ENVIRONMENT_PRODUCTION = "Production";


        public const string APPLICATION_MEMBER_ROLE_ADMINISTRATOR = "ADMINISTRATOR";
        public const string APPLICATION_MEMBER_ROLE_DEVELOPER = "DEVELOPER";
        public const string APPLICATION_MEMBER_ROLE_TESTER = "TESTER";
        public const string APPLICATION_MEMBER_ROLE_CONTINUOUS_INTEGRATION = "CONTINUOUS_INTEGRATION";

        public const string APPLICATION_TEAM_MEMBER_CI_USER_NAME = "CI@betadepot.iteedee.com";

        public const string APPLICATION_JSON_RESULT_SUCCESS = "Success";
        public const string APPLICATION_JSON_RESULT_ERROR = "Error";

    }
}