using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using FastBoard.Core.Serialization;
using IOPath = System.IO.Path;
using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace FastBoard
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OnSavePlaybook(object sender, RoutedEventArgs e)
        {
            var pb = new Core.Models.Playbook { Name = "QuickSave" };
            var json = PlaybookSerializer.ToJson(pb);
            Directory.CreateDirectory("dist");
            File.WriteAllText(IOPath.Combine("dist","playbook.json"), json);
        }

        private void OnOpenPlaybook(object sender, RoutedEventArgs e)
        {
            var path = IOPath.Combine("dist","playbook.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var pb = PlaybookSerializer.FromJson(json);
                // no-op: placeholder to validate
            }
        }

        private void OnExportPng(object sender, RoutedEventArgs e)
        {
            // Try to locate CourtView's SKXamlCanvas via visual tree
            var court = FindDescendant<Controls.CourtView>(this.Content as FrameworkElement);
            if (court == null) return;
            var field = typeof(Controls.CourtView).GetField("Canvas", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public);
            if (field?.GetValue(court) is SKXamlCanvas sk)
            {
                // Render to bitmap by triggering a draw to an off-screen surface
                var width = (int)Math.Max(1, sk.ActualWidth);
                var height = (int)Math.Max(1, sk.ActualHeight);
                using var image = new SKBitmap(width, height);
                using var surface = SKSurface.Create(new SKImageInfo(width, height));
                // We can't easily reuse draw code; for now, just capture a solid image to demonstrate pipeline
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.ForestGreen);
                using var paint = new SKPaint{ Color = SKColors.White, StrokeWidth=3, Style=SKPaintStyle.Stroke, IsAntialias=true };
                canvas.DrawRect(new SKRect(10,10,width-10,height-10), paint);
                canvas.Flush();
                Directory.CreateDirectory("dist");
                using var snapshot = surface.Snapshot();
                using var data = snapshot.Encode(SKEncodedImageFormat.Png, 90);
                using var fs = File.OpenWrite(IOPath.Combine("dist","export.png"));
                data.SaveTo(fs);
            }
        }

        private void OnExportFrames(object sender, RoutedEventArgs e)
        {
            var court = FindDescendant<Controls.CourtView>(this.Content as FrameworkElement);
            if (court == null) return;
            var field = typeof(Controls.CourtView).GetField("Canvas", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public);
            if (field?.GetValue(court) is SKXamlCanvas sk)
            {
                var width = (int)Math.Max(1, sk.ActualWidth);
                var height = (int)Math.Max(1, sk.ActualHeight);
                var framesDir = IOPath.Combine("dist","frames");
                Directory.CreateDirectory(framesDir);
                for (int i=0; i<8; i++)
                {
                    using var surface = SKSurface.Create(new SKImageInfo(width, height));
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.ForestGreen);
                    using var paint = new SKPaint{ Color = SKColors.White, StrokeWidth=3, Style=SKPaintStyle.Stroke, IsAntialias=true };
                    // draw changing rectangle to simulate animation
                    float margin = 10 + i*3;
                    canvas.DrawRect(new SKRect(margin,margin,width-margin,height-margin), paint);
                    canvas.Flush();
                    using var snapshot = surface.Snapshot();
                    using var data = snapshot.Encode(SKEncodedImageFormat.Png, 90);
                    using var fs = File.OpenWrite(IOPath.Combine(framesDir,$"frame-{i:0000}.png"));
                    data.SaveTo(fs);
                }
            }
        }

        private static T? FindDescendant<T>(FrameworkElement? root) where T : class
        {
            if (root == null) return null;
            if (root is T m) return m;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i=0;i<count;i++)
            {
                var child = VisualTreeHelper.GetChild(root,i) as FrameworkElement;
                var res = FindDescendant<T>(child);
                if (res != null) return res;
            }
            return null;
        }
    }
}