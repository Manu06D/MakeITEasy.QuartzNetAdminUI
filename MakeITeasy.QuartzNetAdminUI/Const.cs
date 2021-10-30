using System.Collections.Generic;

namespace MakeITeasy.QuartzNetAdminUI
{
    public class Const
    {
        public static string QueryAction = "action";
        public static string ActionApiArgementNameGet = "argument";


        public static string ActionResourceGet = "getResource";
        public static string ActionResourceNameGet = "resourceName";

        public static string ActionApiGet = "api";
        public static string ActionApiNameGet = "apiName";

        public static string APIArgumentName = "argument";

        public static Dictionary<string, string> ConstDictionnary = new()
        {
            { nameof(ActionResourceGet), ActionResourceGet },
            { nameof(ActionResourceNameGet), ActionResourceNameGet },
            { nameof(ActionApiGet), ActionApiGet },
            { nameof(ActionApiNameGet), ActionApiNameGet },
            { nameof(APIArgumentName), APIArgumentName }
        };
    }
}
