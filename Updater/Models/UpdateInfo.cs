using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.Models.Enums;

namespace Updater.Models
{
    public class UpdateInfo
    {
        public int VersionCode { get; set; }
        [JsonIgnore]
        public string VersionName
        {
            get
            {
                return GetVersionName(VersionCode);
            }
        }
        public int LeastVersion { get; set; }
        [JsonIgnore]
        public UpdateMode UpdateMode
        {
            get
            {
                if (LeastVersion > CurrentVersionCode)
                    return UpdateMode.FullUpdate;
                else
                    return UpdateMode.InnerUpdate;
            }
        }
        public string[] Files { get; set; }

        [JsonIgnore]
        public static int CurrentVersionCode { get; set; } = 20504;
        [JsonIgnore]
        public static string CurrentVersionName
        {
            get
            {
                return GetVersionName(CurrentVersionCode);
            }
        }

        public static string GetVersionName(int versionCode)
        {
            string result = "";
            result = "." + (versionCode % 100);
            versionCode /= 100;
            result = "." + (versionCode % 100) + result;
            versionCode /= 100;
            result = versionCode + result;
            return result;
        }

    }
}
