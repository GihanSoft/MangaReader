using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gihan.Manga.Views.Custom
{
    public static class PagesViewerFactory
    {
        public static PagesViewer GetPagesViewer(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.PageSingle:
                    return new PageSingle();
                case ViewMode.PageDouble:
                    return new PageDouble();
                case ViewMode.RailSingle:
                    return new RailSingle();
                case ViewMode.RailDouble:
                    return new RailDouble();
                case ViewMode.Webtoon:
                    return new Webtoon();
                default:
                    return null;
            }
        }
    }
}
