using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public enum UpdateMode
    {
        InnerUpdate, FullUpdate
    }

    public class UpdateData
    {
        public int VersionCode { get; set; }
        public int LeastVersion { get; set; }
        public UpdateMode UpdateMode { get; set; }
        public List<string> SourceFiles { get; set; }
        public static int ProgramVersionCode { get; } = 20404;
    }
}
