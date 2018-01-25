using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader.Models
{
    [Serializable]
    public class MangaInfo
    {
        public int ID { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
        public String CoverAddress { get; set; }
        public int CurrentChapter { get; set; }
        public double CurrentPlace { get; set; }
    }
}
