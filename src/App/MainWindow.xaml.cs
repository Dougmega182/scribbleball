using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace FastBoard
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            DrawPlaceholderCourt();
        }

        private void DrawPlaceholderCourt()
        {
            // Simple placeholder: draw a rectangle boundary
            var rect = new Rectangle
            {
                Width = 800,
                Height = 500,
                Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.White),
                StrokeThickness = 3
            };
            Canvas.SetLeft(rect, 20);
            Canvas.SetTop(rect, 20);
            CourtCanvas.Children.Add(rect);
        }
    }
}