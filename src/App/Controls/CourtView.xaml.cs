using FastBoard.Core.Geometry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using FastBoard.Core.Tools;
using FastBoard.Core.Models;
using System.IO;

namespace FastBoard.Controls;

public sealed partial class CourtView : UserControl
{
    private readonly List<List<(SKPoint pt, float pressure)>> _strokes = new();
    private List<(SKPoint pt, float pressure)>? _current;

    public CourtView()
    {
        this.InitializeComponent();
        this.Loaded += (_,__) => Canvas.Invalidate();
    }

    public bool HalfCourt
    {
        get => (bool)GetValue(HalfCourtProperty);
        set => SetValue(HalfCourtProperty, value);
    }

    public static readonly DependencyProperty HalfCourtProperty =
        DependencyProperty.Register(nameof(HalfCourt), typeof(bool), typeof(CourtView), new PropertyMetadata(true, OnInvalidate));

    private static void OnInvalidate(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CourtView cv)
        {
            cv.Canvas?.Invalidate();
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(34, 139, 34)); // ForestGreen background

        // Determine DPI scale
        float dpi = 96f;
        try
        {
            var xamlRoot = this.XamlRoot;
            if (xamlRoot is not null)
            {
                dpi = (float)(xamlRoot.RasterizationScale * 96.0);
            }
        }
        catch { dpi = 96f; }
        double scale = 1.0;

        // Compute target court size to fit within control
        var (courtW, courtL) = CourtGeometry.GetCourtSizePx(HalfCourt, dpi, scale);
        float availW = (float)e.Info.Width;
        float availH = (float)e.Info.Height;
        float scaleFactor = Math.Min(availW / (float)courtW, availH / (float)courtL);
        float ox = (availW - (float)courtW * s) / 2f;
        float oy = (availH - (float)courtL * s) / 2f;

        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true, StrokeWidth = 3, Style = SKPaintStyle.Stroke };

        // Outer boundary
        var rect = new SKRect(ox, oy, ox + (float)courtW * scaleFactor, oy + (float)courtL * s);
        canvas.DrawRect(rect, paint);

        // Hoop/backboard positions using feet distances
        float basketToBaselinePx = (float)(CourtGeometry.FtToPx(CourtGeometry.BasketToBaselineFt, dpi, scale) * s);
        float hoopX = rect.MidX;
        float hoopY = rect.Top + basketToBaselinePx;
        canvas.DrawCircle(hoopX, hoopY, 9, paint);
        // backboard is a short line behind the hoop
        canvas.DrawLine(hoopX - 30, hoopY + 12, hoopX + 30, hoopY + 12, paint);

        // Key / lane
        var laneWidth = (float)(CourtGeometry.FtToPx(CourtGeometry.LaneWidthFt, dpi, scale) * s);
        float laneLeft = rect.MidX - laneWidth / 2f;
        float ftFromBackboardPx = (float)(CourtGeometry.FtToPx(CourtGeometry.FreeThrowLineDistFt, dpi, scale) * s);
        float laneTop = hoopY + 12 + 12;
        float ftY = laneTop + ftFromBackboardPx;
        canvas.DrawRect(laneLeft, laneTop, laneWidth, ftFromBackboardPx, paint);
        var ftRadius = (float)(CourtGeometry.FtToPx(6, dpi, scale) * s);
        var ftRect = new SKRect(rect.MidX - ftRadius, ftY - ftRadius, rect.MidX + ftRadius, ftY + ftRadius);
        canvas.DrawArc(ftRect, 0, 180, false, paint);

        // Restricted area arc around hoop
        var restR = (float)(CourtGeometry.RestrictedArcRadiusPx(dpi, scale) * s);
        var restRect = new SKRect(hoopX - restR, hoopY - restR, hoopX + restR, hoopY + restR);
        canvas.DrawArc(restRect, 200, 140, false, paint);

        // Three-point arc centered on hoop with corner lines
        var threeR = (float)(CourtGeometry.ThreePointArcRadiusPx(dpi, scale) * s);
        var threeRect = new SKRect(hoopX - threeR, hoopY - threeR, hoopX + threeR, hoopY + threeR);
        canvas.DrawArc(threeRect, 210, 120, false, paint);
        // corner distance is slightly shorter; draw vertical corner lines until they meet arc
        float cornerDistPx = (float)(CourtGeometry.FtToPx(CourtGeometry.CornerThreeDistFt, dpi, scale) * s);
        float cornerXOffset = (float)(CourtGeometry.FtToPx((CourtGeometry.CourtWidthFt/2) - (CourtGeometry.CornerThreeDistFt), dpi, scale) * s);
        float leftCornerX = rect.MidX - cornerXOffset;
        float rightCornerX = rect.MidX + cornerXOffset;
        canvas.DrawLine(leftCornerX, hoopY + 12, leftCornerX, hoopY + 200, paint);
        canvas.DrawLine(rightCornerX, hoopY + 12, rightCornerX, hoopY + 200, paint);

        // Draw shapes from VM
        if (DataContext is FastBoard.ViewModels.BoardViewModel vm)
        {
            foreach (var shape in vm.Shapes)
            {
                switch (shape)
                {
                    case FastBoard.Core.Models.ArrowShape a:
                        DrawArrow(canvas, a);
                        if (a.Selected) DrawSelection(canvas, new SKRect(Math.Min(a.Start.X,a.End.X)-8, Math.Min(a.Start.Y,a.End.Y)-8, Math.Max(a.Start.X,a.End.X)+8, Math.Max(a.Start.Y,a.End.Y)+8));
                        break;
                    case FastBoard.Core.Models.DashedShape d:
                        DrawDashed(canvas, d);
                        break;
                    case FastBoard.Core.Tools.CurveShape c:
                        DrawCurve(canvas, c);
                        break;
                    case FastBoard.Core.Tools.ScreenShape s:
                        DrawScreen(canvas, s);
                        break;
                    case FastBoard.Core.Models.Token t:
                        DrawToken(canvas, t);
                        break;
                }
            }
        }

        // Draw strokes (ink)
        foreach (var stroke in _strokes)
        {
            for (int i=1;i<stroke.Count;i++)
            {
                var a = stroke[i-1];
                var b = stroke[i];
                using var sp = new SKPaint{ Color = SKColors.Yellow, StrokeCap = SKStrokeCap.Round, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 + 6 * b.pressure };
                canvas.DrawLine(a.pt, b.pt, sp);
            }
        }

        // current stroke
        if (_current != null && _current.Count > 1)
        {
            for (int i=1;i<_current.Count;i++)
            {
                var a = _current[i-1];
                var b = _current[i];
                using var sp = new SKPaint{ Color = SKColors.Orange, StrokeCap = SKStrokeCap.Round, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 + 6 * b.pressure };
                canvas.DrawLine(a.pt, b.pt, sp);
            }
        }
    }

    private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(Canvas);
        float pressure = (float)pt.Properties.Pressure;
        var p = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);
        if (DataContext is FastBoard.ViewModels.BoardViewModel vm && vm.GetActiveDrawingTool() is FastBoard.Core.Tools.IDrawingTool tool)
        {
            tool.Begin(new System.Numerics.Vector2(p.X, p.Y), pressure);
        }
        else
        {
            _current = new List<(SKPoint pt, float pressure)> { (p, pressure) };
        }
        Canvas.CapturePointer(e.Pointer);
        Canvas.Invalidate();
    }

    private void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(Canvas);
        var p = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);
        float pressure = (float)pt.Properties.Pressure;
        if (DataContext is FastBoard.ViewModels.BoardViewModel vm && vm.GetActiveDrawingTool() is FastBoard.Core.Tools.IDrawingTool tool)
        {
            tool.Move(new System.Numerics.Vector2(p.X, p.Y), pressure);
        }
        else if (_current != null)
        {
            _current.Add((p, pressure));
        }
        Canvas.Invalidate();
    }

    private void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(Canvas);
        var p = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);
        float pressure = (float)pt.Properties.Pressure;
        if (DataContext is FastBoard.ViewModels.BoardViewModel vm && vm.GetActiveDrawingTool() is FastBoard.Core.Tools.IDrawingTool tool)
        {
            tool.End(new System.Numerics.Vector2(p.X, p.Y), pressure);
        }
        else if (_current != null)
        {
            _strokes.Add(_current);
            _current = null;
        }
        Canvas.ReleasePointerCapture(e.Pointer);
        Canvas.Invalidate();
    }

    private void DrawArrow(SKCanvas canvas, FastBoard.Core.Models.ArrowShape a)
    {
        using var p = new SKPaint{ Color = SKColors.White, Style=SKPaintStyle.Stroke, StrokeWidth=a.Thickness, IsAntialias=true, StrokeCap=SKStrokeCap.Round };
        canvas.DrawLine(new SKPoint(a.Start.X, a.Start.Y), new SKPoint(a.End.X, a.End.Y), p);
        var (h1,h2) = FastBoard.Core.Geometry.GeometryUtils.ArrowHead(new System.Numerics.Vector2(a.Start.X,a.Start.Y), new System.Numerics.Vector2(a.End.X,a.End.Y), a.HeadLength, a.HeadAngleDeg);
        using var p2 = new SKPaint{ Color = SKColors.White, Style=SKPaintStyle.Stroke, StrokeWidth=a.Thickness, IsAntialias=true, StrokeCap=SKStrokeCap.Round };
        canvas.DrawLine(new SKPoint(h1.X, h1.Y), new SKPoint(a.End.X, a.End.Y), p2);
        canvas.DrawLine(new SKPoint(h2.X, h2.Y), new SKPoint(a.End.X, a.End.Y), p2);
    }

    private void DrawDashed(SKCanvas canvas, FastBoard.Core.Models.DashedShape d)
    {
        if (d.Points.Count < 2) return;
        using var p = new SKPaint{ Color = SKColors.White, Style=SKPaintStyle.Stroke, StrokeWidth=d.Thickness, IsAntialias=true, PathEffect = SKPathEffect.CreateDash(new float[]{d.Dash, d.Gap}, 0)};
        var path = new SKPath();
        path.MoveTo(d.Points[0].X, d.Points[0].Y);
        for (int i=1;i<d.Points.Count;i++) path.LineTo(d.Points[i].X, d.Points[i].Y);
        canvas.DrawPath(path, p);
    }

    private void DrawCurve(SKCanvas canvas, FastBoard.Core.Tools.CurveShape c)
    {
        using var p = new SKPaint{ Color = SKColors.White, Style=SKPaintStyle.Stroke, StrokeWidth=c.Thickness, IsAntialias=true };
        var path = new SKPath();
        path.MoveTo(c.P0.X, c.P0.Y);
        path.QuadTo(c.P1.X, c.P1.Y, c.P2.X, c.P2.Y);
        canvas.DrawPath(path, p);
    }

    private void DrawScreen(SKCanvas canvas, FastBoard.Core.Tools.ScreenShape s)
    {
        using var p = new SKPaint{ Color = SKColors.White, Style=SKPaintStyle.Stroke, StrokeWidth=s.Thickness, IsAntialias=true };
        var half = new SKPoint(s.Size.X/2, s.Size.Y/2);
        var r = new SKRoundRect(new SKRect(s.Center.X-half.X, s.Center.Y-half.Y, s.Center.X+half.X, s.Center.Y+half.Y), s.CornerRadius, s.CornerRadius);
        canvas.DrawRoundRect(r, p);
    }

    private void DrawSelection(SKCanvas canvas, SKRect rect)
    {
        using var p = new SKPaint{ Color = SKColors.Yellow, Style=SKPaintStyle.Stroke, StrokeWidth=2, PathEffect=SKPathEffect.CreateDash(new float[]{6,6},0), IsAntialias=true };
        canvas.DrawRect(rect, p);
    }

    private readonly Dictionary<string, SKBitmap> _bitmapCache = new();
    private void DrawToken(SKCanvas canvas, FastBoard.Core.Models.Token t)
    {
        if (t.Selected)
        {
            DrawSelection(canvas, new SKRect(t.Position.X-24, t.Position.Y-24, t.Position.X+24, t.Position.Y+24));
        }
        if (string.IsNullOrEmpty(t.ImagePath))
        {
            using var p = new SKPaint{ Color = SKColors.Blue, Style=SKPaintStyle.Fill, IsAntialias=true };
            canvas.DrawCircle(t.Position.X, t.Position.Y, 18*t.Scale, p);
            return;
        }
        if (!_bitmapCache.TryGetValue(t.ImagePath, out var bmp))
        {
            try
            {
                using var fs = File.OpenRead(t.ImagePath);
                bmp = SKBitmap.Decode(fs);
                if (bmp != null) _bitmapCache[t.ImagePath] = bmp;
            }
            catch { }
        }
        if (bmp != null)
        {
            var w = bmp.Width * t.Scale; var h = bmp.Height * t.Scale;
            var dest = new SKRect(t.Position.X - w/2, t.Position.Y - h/2, t.Position.X + w/2, t.Position.Y + h/2);
            canvas.DrawBitmap(bmp, dest);
        }
    }
}

