using System.Collections.Generic;

namespace MakeITeasy.QuartzNetAdminUI
{
    public class Const
    {
        public static string QueryAction = "action";
        public static string ActionApiArgementNameGet = "argument";


        public static string ActionRessourceGet = "getRessource";
        public static string ActionRessourceNameGet = "ressourceName";

        public static string ActionApiGet = "api";
        public static string ActionApiNameGet = "apiName";

        public static string APIArgumentName = "argument";

        public static Dictionary<string, string> ConstDictionnary = new()
        {
            { nameof(ActionRessourceGet), ActionRessourceGet },
            { nameof(ActionRessourceNameGet), ActionRessourceNameGet },
            { nameof(ActionApiGet), ActionApiGet },
            { nameof(ActionApiNameGet), ActionApiNameGet }
        };
    }
}
