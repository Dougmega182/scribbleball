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
        private readonly ViewModels.BoardViewModel _vm = new();
        public MainWindow()
        {
            this.InitializeComponent();
            Court.DataContext = _vm;
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
            var court = FindDescendant<Controls.CourtView>(this.Content as FrameworkElement) ?? Court;
            if (court == null) return;
            var width = (int)Math.Max(800, Court.ActualWidth);
            var height = (int)Math.Max(600, Court.ActualHeight);
            Directory.CreateDirectory("dist");
            var path = IOPath.Combine("dist","export.png");
            court.ExportPng(path, width, height);
        }

        private void OnAddSampleToken(object sender, RoutedEventArgs e)
        {
            var p = new System.Numerics.Vector2(200,200);
            var sample = IOPath.Combine("sample_data","placeholder1.png");
            _vm.Shapes.Add(new Core.Models.Token{ Name="P1", ImagePath=sample, Position=p, Scale=0.75f });
            Court.InvalidateArrange();
        }

        private void OnCaptureFrame(object sender, RoutedEventArgs e)
        {
            _vm.CaptureFrame();
        }

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            _playTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _playTimer.Tick += OnPlayTick;
            _lastTick = DateTime.UtcNow;
            _t = 0f;
            _playTimer.Start();
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            _playTimer?.Stop();
            _t = 0f;
            Court.AnimationT = 0f;
        }

        private void OnNext(object sender, RoutedEventArgs e)
        {
            if (_vm.Frames.Count > 0 && _vm.CurrentFrameIndex < _vm.Frames.Count - 1)
                _vm.CurrentFrameIndex++;
        }

        private void OnPrev(object sender, RoutedEventArgs e)
        {
            if (_vm.CurrentFrameIndex > 0) _vm.CurrentFrameIndex--;
        }

        private void OnPlayTick(object? sender, object e)
        {
            var now = DateTime.UtcNow;
            var dt = (float)(now - _lastTick).TotalSeconds;
            _lastTick = now;
            _t += dt; // seconds
            if (_t >= 1f)
            {
                _t = 0f;
                if (_vm.CurrentFrameIndex < _vm.Frames.Count - 2)
                    _vm.CurrentFrameIndex++;
                else
                    _playTimer?.Stop();
            }
            Court.AnimationT = _t;
            Court.InvalidateArrange();
        }

        private void OnZoomIn(object sender, RoutedEventArgs e) => Court.ZoomIn();
        private void OnZoomOut(object sender, RoutedEventArgs e) => Court.ZoomOut();
        private void OnZoomReset(object sender, RoutedEventArgs e) => Court.ZoomReset();

        private DispatcherTimer? _playTimer;
        private DateTime _lastTick;
        private float _t;

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

        private void OnDeleteSelected(object sender, RoutedEventArgs e)
        {
            for (int i=_vm.Shapes.Count-1;i>=0;i--) if (_vm.Shapes[i].Selected) _vm.Shapes.RemoveAt(i);
            Court.InvalidateArrange();
        }

        private void OnToolToggle(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarToggleButton t)
            {
                var tag = t.Tag as string;
                _vm.ActiveTool = tag switch
                {
                    "Arrow" => Core.Tools.ToolType.Arrow,
                    "Dribble" => Core.Tools.ToolType.Dribble,
                    _ => Core.Tools.ToolType.None
                };
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