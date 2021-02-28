using MangaReader.PagesViewer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using static System.Windows.Data.BindingOperations;

namespace Gihan.Manga.Views.Custom
{
    /// <summary>
    /// Interaction logic for RailDouble.xaml
    /// </summary>
    public partial class RailDouble : PagesViewer
    {
        public override double Zoom
        {
            get => (ImagesRailGrd.RowDefinitions.FirstOrDefault()?
                        .GetBindingExpression(HeightProperty)?
                        .ParentBinding.Converter as ZaribConverter)?.Zarib ?? 1;
            set
            {
                var page = Page;
                var offset = Offset;
                double? dZoom = null;
                foreach (var rowDef in ImagesRailGrd.RowDefinitions)
                {
                    if (rowDef is null) continue;
                    if (dZoom is null)
                        dZoom = value / (GetBinding(rowDef, HeightProperty).Converter as ZaribConverter).Zarib;
                    (GetBinding(rowDef, MaxHeightProperty)
                        .Converter as ZaribConverter).Zarib = value;
                    (GetBinding(rowDef, HeightProperty)
                        .Converter as ZaribConverter).Zarib = value;
                    rowDef.SetBinding(MaxHeightProperty, GetBinding(rowDef, MaxHeightProperty));
                    rowDef.SetBinding(HeightProperty, GetBinding(rowDef, HeightProperty));
                }
                Page = page;
                Offset = offset * (dZoom ?? 1);
            }
        }
        public override double Offset
        {
            get => Sv.VerticalOffset % Sv.ViewportHeight;
            set
            {
                var result = (Page - 1) * Sv.ViewportHeight + value;
                Sv.ScrollToVerticalOffset(result);
            }
        }
        public override int Page
        {
            get => (int)Math.Round(Sv.VerticalOffset / Sv.ViewportHeight > 0 ? Sv.ViewportHeight : 1) + 1;
            set
            {
                if (value > _images.Length || value < 1) return;

            }
        }

        public RailDouble()
        {
            InitializeComponent();
        }

        protected override void SetSourceStreams(IEnumerable<Stream> streams)
        {
            base.SetSourceStreams(streams);
            for (int i = 0; i < Math.Floor(_sourceStreams.Length / 2.0); i++)
            {
                var heightBinding = new Binding(nameof(Sv.ViewportHeight))
                {
                    Source = Sv,
                    Converter = new ZaribConverter { Zarib = Zoom },
                };
                var rowDef = new RowDefinition();
                rowDef.SetBinding(HeightProperty, heightBinding);
                rowDef.SetBinding(MaxHeightProperty, heightBinding);
                ImagesRailGrd.RowDefinitions.Add(rowDef);
            }
            for (int i = 0; i < _sourceStreams.Length; i++)
            {
                _images[i] = new Image();
            }
        }

        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }
    }
}
