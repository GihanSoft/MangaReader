using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Controllers
{
    class Uris
    {
        public string Api { get; set; }
        public string Dl { get; set; }
        public string Update { get; set; }
        public string Setup { get; set; }
        public string[] Installers { get; set; }

        public static Uris This { get; set; }

        static Uris()
        {
            var uris = JsonConvert.DeserializeObject<Uris>(File.ReadAllText(@"Ref\Uris.json"));
            This = uris;
        }

        public string GetUpdate()
        {
            return CombineUri(Api, Update);
        }
        public string GetSetup()
        {
            return CombineUri(Dl, Setup);
        }
        public string[] GetInstallers()
        {
            var result = new string[Installers.Length];
            for (int i = 0; i < Installers.Length; i++)
            {
                result[i] = CombineUri(Dl, Setup, Installers[i]);
            }
            return result;
        }

        static string CombineUri(params string[] uris)
        {
            string result = "";
            for (int i = 0; i < uris.Length; i++)
            {
                var item = uris[i];
                if (item.StartsWith("/")) item = item.Substring(1);
                if (!item.EndsWith("/")) item += "/";
                result += item;
            }
            return result;
        }
    }
}
