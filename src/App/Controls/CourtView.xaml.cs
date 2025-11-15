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
    public float AnimationT { get; set; } = 0f;
    private readonly List<List<(SKPoint pt, float pressure)>> _strokes = new();
    private List<(SKPoint pt, float pressure)>? _current;
    private float _zoom = 1f;
    private SKPoint _pan = new SKPoint(0, 0);
    private bool _isPanning = false;
    private SKPoint _lastPanPt;
    private bool _isManipulating = false;
    private float _startZoom;
    private SKPoint _manipulationCenter;
    private FastBoard.Core.Tools.ToolType? _prevToolWhenEraser;
    private readonly Dictionary<string, SKBitmap> _bitmapCache = new();

    public CourtView(){ this.InitializeComponent(); this.Loaded += (_,__) => SkCanvas.Invalidate(); }

    public bool HalfCourt
    {
        get => (bool)GetValue(HalfCourtProperty);
        set => SetValue(HalfCourtProperty, value);
    }

    public static readonly DependencyProperty HalfCourtProperty =
        DependencyProperty.Register(nameof(HalfCourt), typeof(bool), typeof(CourtView), new PropertyMetadata(true, OnInvalidate));

    private static void OnInvalidate(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CourtView cv) cv.SkCanvas?.Invalidate();
    }

    private void DrawBoard(SKCanvas canvas, SKImageInfo info)
    {
        canvas.Clear(new SKColor(34, 139, 34));
        float dpi = 96f;
        try
        {
            var xamlRoot = this.XamlRoot;
            if (xamlRoot is not null) dpi = (float)(xamlRoot.RasterizationScale * 96.0);
        }
        catch { dpi = 96f; }

        double scale = 1.0;
        var (courtW, courtL) = CourtGeometry.GetCourtSizePx(HalfCourt, dpi, scale);
        float availW = (float)info.Width;
        float availH = (float)info.Height;
        float scaleFactor = Math.Min(availW / (float)courtW, availH / (float)courtL);
        float ox = (availW - (float)courtW * scaleFactor) / 2f;
        float oy = (availH - (float)courtL * scaleFactor) / 2f;
        float lineWidth = (float)(CourtGeometry.FtToPx(0.1667, dpi, scale) * scaleFactor);

        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true, StrokeWidth = Math.Max(1f, lineWidth), Style = SKPaintStyle.Stroke };
        var rect = new SKRect(ox, oy, ox + (float)courtW * scaleFactor, oy + (float)courtL * scaleFactor);
        canvas.DrawRect(rect, paint);

        float basketToBaselinePx = (float)(CourtGeometry.FtToPx(CourtGeometry.BasketToBaselineFt, dpi, scale) * scaleFactor);
        float hoopX = rect.MidX;
        float hoopY = rect.Top + basketToBaselinePx;
        canvas.DrawCircle(hoopX, hoopY, 9, paint);
        canvas.DrawLine(hoopX - 30, hoopY + 12, hoopX + 30, hoopY + 12, paint);

        var laneWidth = (float)(CourtGeometry.FtToPx(CourtGeometry.LaneWidthFt, dpi, scale) * scaleFactor);
        float laneLeft = rect.MidX - laneWidth / 2f;
        float ftFromBackboardPx = (float)(CourtGeometry.FtToPx(CourtGeometry.FreeThrowLineDistFt, dpi, scale) * scaleFactor);
        float laneTop = hoopY + 12 + 12;
        float ftY = laneTop + ftFromBackboardPx;
        canvas.DrawRect(laneLeft, laneTop, laneWidth, ftFromBackboardPx, paint);

        var ftRadius = (float)(CourtGeometry.FtToPx(6, dpi, scale) * scaleFactor);
        using (var ftPaint = new SKPaint { Color = paint.Color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = paint.StrokeWidth, PathEffect = SKPathEffect.CreateDash(new float[] { 12, 8 }, 0) })
            canvas.DrawCircle(rect.MidX, ftY, ftRadius, ftPaint);

        if (!HalfCourt)
        {
            canvas.DrawLine(rect.Left, rect.MidY, rect.Right, rect.MidY, paint);
            var ccR = (float)(CourtGeometry.CenterCircleRadiusPx(dpi, scale) * scaleFactor);
            using (var dashed = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = paint.StrokeWidth, PathEffect = SKPathEffect.CreateDash(new float[] { 12, 8 }, 0) })
                canvas.DrawCircle(rect.MidX, rect.MidY, ccR, dashed);
        }

        var restR = (float)(CourtGeometry.RestrictedArcRadiusPx(dpi, scale) * scaleFactor);
        var restRect = new SKRect(hoopX - restR, hoopY - restR, hoopX + restR, hoopY + restR);
        canvas.DrawArc(restRect, 210, 120, false, paint);

        var threeR = (float)(CourtGeometry.ThreePointArcRadiusPx(dpi, scale) * scaleFactor);
        var threeRect = new SKRect(hoopX - threeR, hoopY - threeR, hoopX + threeR, hoopY + threeR);
        float cornerOffset = (float)(CourtGeometry.FtToPx(CourtGeometry.CornerThreeDistFt, dpi, scale) * scaleFactor);
        float leftCornerX = hoopX - cornerOffset;
        float rightCornerX = hoopX + cornerOffset;

        var (lY1, lY2) = FastBoard.Core.Geometry.CourtGeometry.CircleVerticalLineIntersectionY(hoopX, hoopY, threeR, leftCornerX);
        var (rY1, rY2) = FastBoard.Core.Geometry.CourtGeometry.CircleVerticalLineIntersectionY(hoopX, hoopY, threeR, rightCornerX);
        float leftIntersectY = float.IsNaN((float)lY1) && float.IsNaN((float)lY2) ? hoopY : (float)Math.Max(lY1, lY2);
        float rightIntersectY = float.IsNaN((float)rY1) && float.IsNaN((float)rY2) ? hoopY : (float)Math.Max(rY1, rY2);

        float cornerBottomPadding = 0f;
        float lineStartY = hoopY + 12 + cornerBottomPadding;
        canvas.DrawLine(leftCornerX, lineStartY, leftCornerX, leftIntersectY, paint);
        canvas.DrawLine(rightCornerX, lineStartY, rightCornerX, rightIntersectY, paint);

        static float DegNorm(float a) { a %= 360f; if (a < 0) a += 360f; return a; }
        float AngleAt(float x, float y, float cx, float cy)
        {
            float ang = (float)(Math.Atan2(y - cy, x - cx) * 180.0 / Math.PI);
            return DegNorm(ang);
        }
        float leftAng = AngleAt(leftCornerX, leftIntersectY, hoopX, hoopY);
        float rightAng = AngleAt(rightCornerX, rightIntersectY, hoopX, hoopY);
        float cwSweep = DegNorm(rightAng - leftAng);
        float sweep = (cwSweep >= 80f && cwSweep <= 200f) ? cwSweep : DegNorm(leftAng - rightAng);
        float start = (sweep == cwSweep) ? leftAng : rightAng;
        canvas.DrawArc(threeRect, start, sweep, false, paint);

        if (!HalfCourt)
        {
            float hoopY2 = rect.Bottom - basketToBaselinePx;
            canvas.DrawCircle(hoopX, hoopY2, 9, paint);
            canvas.DrawLine(hoopX - 30, hoopY2 - 12, hoopX + 30, hoopY2 - 12, paint);
            var laneWidth2 = laneWidth;
            float laneLeft2 = rect.MidX - laneWidth2 / 2f;
            float laneTop2 = hoopY2 - 12 - 12 - ftFromBackboardPx;
            canvas.DrawRect(laneLeft2, laneTop2, laneWidth2, ftFromBackboardPx, paint);
            float ftY2 = laneTop2 + ftFromBackboardPx;
            using (var ftPaint = new SKPaint { Color = paint.Color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = paint.StrokeWidth, PathEffect = SKPathEffect.CreateDash(new float[] { 12, 8 }, 0) })
                canvas.DrawCircle(rect.MidX, ftY2, ftRadius, ftPaint);
            var restRect2 = new SKRect(hoopX - restR, hoopY2 - restR, hoopX + restR, hoopY2 + restR);
            canvas.DrawArc(restRect2, 30, 120, false, paint);
            var threeRect2 = new SKRect(hoopX - threeR, hoopY2 - threeR, hoopX + threeR, hoopY2 + threeR);
            float leftCornerX2 = leftCornerX;
            float rightCornerX2 = rightCornerX;
            var (l2Y1, l2Y2) = FastBoard.Core.Geometry.CourtGeometry.CircleVerticalLineIntersectionY(hoopX, hoopY2, threeR, leftCornerX2);
            var (r2Y1, r2Y2) = FastBoard.Core.Geometry.CourtGeometry.CircleVerticalLineIntersectionY(hoopX, hoopY2, threeR, rightCornerX2);
            float leftIntersectY2 = float.IsNaN((float)l2Y1) && float.IsNaN((float)l2Y2) ? hoopY2 : (float)Math.Min(l2Y1, l2Y2);
            float rightIntersectY2 = float.IsNaN((float)r2Y1) && float.IsNaN((float)r2Y2) ? hoopY2 : (float)Math.Min(r2Y1, r2Y2);
            float lineStartY2 = hoopY2 - 12 - cornerBottomPadding;
            canvas.DrawLine(leftCornerX2, lineStartY2, leftCornerX2, leftIntersectY2, paint);
            canvas.DrawLine(rightCornerX2, lineStartY2, rightCornerX2, rightIntersectY2, paint);
            float leftAng2 = AngleAt(leftCornerX2, leftIntersectY2, hoopX, hoopY2);
            float rightAng2 = AngleAt(rightCornerX2, rightIntersectY2, hoopX, hoopY2);
            float cwSweep2 = DegNorm(rightAng2 - leftAng2);
            float sweep2 = (cwSweep2 >= 80f && cwSweep2 <= 200f) ? cwSweep2 : DegNorm(leftAng2 - rightAng2);
            float start2 = (sweep2 == cwSweep2) ? leftAng2 : rightAng2;
            canvas.DrawArc(threeRect2, start2, sweep2, false, paint);
        }

        canvas.Save();
        canvas.Translate(_pan.X, _pan.Y);
        canvas.Scale(_zoom);

        if (DataContext is FastBoard.ViewModels.BoardViewModel vm)
        {
            var shapes = vm.CurrentFrameIndex >= 0 && vm.CurrentFrameIndex < vm.Frames.Count - 1
                ? vm.GetFrameShapes(Math.Min(1f, AnimationT))
                : vm.Shapes;

            foreach (var shape in shapes)
            {
                switch (shape)
                {
                    case FastBoard.Core.Models.ArrowShape a:
                        DrawArrow(canvas, a);
                        if (a.Selected)
                        {
                            DrawSelection(canvas, new SKRect(Math.Min(a.Start.X, a.End.X) - 8, Math.Min(a.Start.Y, a.End.Y) - 8, Math.Max(a.Start.X, a.End.X) + 8, Math.Max(a.Start.Y, a.End.Y) + 8));
                            DrawArrowHandles(canvas, a);
                        }
                        break;
                    case FastBoard.Core.Models.DashedShape d:
                        DrawDashed(canvas, d);
                        break;
                    case FastBoard.Core.Tools.CurveShape c:
                        DrawCurve(canvas, c);
                        if (c.Selected) DrawCurveHandles(canvas, c);
                        break;
                    case FastBoard.Core.Tools.ScreenShape s:
                        DrawScreen(canvas, s);
                        if (s.Selected) DrawScreenHandles(canvas, s);
                        break;
                    case FastBoard.Core.Models.Token t:
                        DrawToken(canvas, t);
                        break;
                    case FastBoard.Core.Tools.ShotArcShape sa:
                        DrawShotArc(canvas, sa);
                        break;
                    case FastBoard.Core.Models.StrokeShape st:
                        DrawStroke(canvas, st);
                        break;
                }
            }
        }

        foreach (var stroke in _strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
            {
                var a = stroke[i - 1];
                var b = stroke[i];
                using var sp = new SKPaint { Color = SKColors.Yellow, StrokeCap = SKStrokeCap.Round, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 + 6 * b.pressure };
                canvas.DrawLine(a.pt, b.pt, sp);
            }
        }

        if (_current != null && _current.Count > 1)
        {
            for (int i = 1; i < _current.Count; i++)
            {
                var a = _current[i - 1];
                var b = _current[i];
                using var sp = new SKPaint { Color = SKColors.Orange, StrokeCap = SKStrokeCap.Round, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 + 6 * b.pressure };
                canvas.DrawLine(a.pt, b.pt, sp);
            }
        }

        canvas.Restore();
    }

    private SKPoint ScreenToWorld(SKPoint s) => new SKPoint((s.X - _pan.X) / _zoom, (s.Y - _pan.Y) / _zoom);

    public System.Numerics.Vector2 ScreenToWorld(double x, double y)
    {
        var p = ScreenToWorld(new SKPoint((float)x, (float)y));
        return new System.Numerics.Vector2(p.X, p.Y);
    }

    public void ExportPng(string path, int width, int height)
    {
        var info = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        DrawBoard(canvas, info);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(fs);
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        DrawBoard(e.Surface.Canvas, e.Info);
    }

    private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(SkCanvas);
        float pressure = (float)pt.Properties.Pressure;
        var pScreen = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);

        if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Pen && pt.Properties.IsBarrelButtonPressed)
        {
            if (DataContext is FastBoard.ViewModels.BoardViewModel vmEraser && vmEraser.ActiveTool != FastBoard.Core.Tools.ToolType.Eraser)
            {
                _prevToolWhenEraser = vmEraser.ActiveTool;
                vmEraser.ActiveTool = FastBoard.Core.Tools.ToolType.Eraser;
            }
        }

        if (pt.Properties.IsRightButtonPressed)
        {
            _isPanning = true; _lastPanPt = pScreen; return;
        }

        var p = ScreenToWorld(pScreen);

        if (DataContext is FastBoard.ViewModels.BoardViewModel vm && vm.GetActiveDrawingTool() is FastBoard.Core.Tools.IDrawingTool tool)
        {
            tool.Begin(new System.Numerics.Vector2(p.X, p.Y), pressure);
        }
        else
        {
            _current = new List<(SKPoint pt, float pressure)> { (p, pressure) };
        }

        SkCanvas.CapturePointer(e.Pointer);
        SkCanvas.Invalidate();
    }

    private void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(SkCanvas);
        var pScreen = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);

        if (_isPanning)
        {
            var delta = new SKPoint(pScreen.X - _lastPanPt.X, pScreen.Y - _lastPanPt.Y);
            _pan = new SKPoint(_pan.X + delta.X, _pan.Y + delta.Y);
            _lastPanPt = pScreen;
            SkCanvas.Invalidate();
            return;
        }

        var p = ScreenToWorld(pScreen);
        float pressure = (float)pt.Properties.Pressure;

        if (DataContext is FastBoard.ViewModels.BoardViewModel vm && vm.GetActiveDrawingTool() is FastBoard.Core.Tools.IDrawingTool tool)
        {
            tool.Move(new System.Numerics.Vector2(p.X, p.Y), pressure);
        }
        else if (_current != null)
        {
            _current.Add((p, pressure));
        }

        SkCanvas.Invalidate();
    }

    private void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(SkCanvas);
        var pScreen = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);

        if (_isPanning) { _isPanning = false; return; }

        if (_prevToolWhenEraser.HasValue && DataContext is FastBoard.ViewModels.BoardViewModel vm0)
        {
            vm0.ActiveTool = _prevToolWhenEraser.Value;
            _prevToolWhenEraser = null;
        }

        var p = ScreenToWorld(pScreen);
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

        SkCanvas.ReleasePointerCapture(e.Pointer);
        SkCanvas.Invalidate();
    }

    private void DrawArrow(SKCanvas canvas, FastBoard.Core.Models.ArrowShape a)
    {
        using var p = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = a.Thickness, IsAntialias = true, StrokeCap = SKStrokeCap.Round };
        canvas.DrawLine(new SKPoint(a.Start.X, a.Start.Y), new SKPoint(a.End.X, a.End.Y), p);
        var (h1, h2) = FastBoard.Core.Geometry.GeometryUtils.ArrowHead(new System.Numerics.Vector2(a.Start.X, a.Start.Y), new System.Numerics.Vector2(a.End.X, a.End.Y), a.HeadLength, a.HeadAngleDeg);
        using var p2 = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = a.Thickness, IsAntialias = true, StrokeCap = SKStrokeCap.Round };
        canvas.DrawLine(new SKPoint(h1.X, h1.Y), new SKPoint(a.End.X, a.End.Y), p2);
        canvas.DrawLine(new SKPoint(h2.X, h2.Y), new SKPoint(a.End.X, a.End.Y), p2);
    }

    private void DrawDashed(SKCanvas canvas, FastBoard.Core.Models.DashedShape d)
    {
        if (d.Points.Count < 2) return;
        using var p = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = d.Thickness, IsAntialias = true, PathEffect = SKPathEffect.CreateDash(new float[] { d.Dash, d.Gap }, 0) };
        var path = new SKPath();
        path.MoveTo(d.Points[0].X, d.Points[0].Y);
        for (int i = 1; i < d.Points.Count; i++) path.LineTo(d.Points[i].X, d.Points[i].Y);
        canvas.DrawPath(path, p);
    }

    private void DrawCurve(SKCanvas canvas, FastBoard.Core.Tools.CurveShape c)
    {
        using var p = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = c.Thickness, IsAntialias = true };
        var path = new SKPath();
        path.MoveTo(c.P0.X, c.P0.Y);
        path.QuadTo(c.P1.X, c.P1.Y, c.P2.X, c.P2.Y);
        canvas.DrawPath(path, p);
    }

    private void DrawScreen(SKCanvas canvas, FastBoard.Core.Tools.ScreenShape s)
    {
        using var p = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = s.Thickness, IsAntialias = true };
        var half = new SKPoint(s.Size.X / 2, s.Size.Y / 2);
        var rect = new SKRect(-half.X, -half.Y, half.X, half.Y);
        canvas.Save();
        canvas.Translate(s.Center.X, s.Center.Y);
        canvas.RotateDegrees(s.Rotation);
        var r = new SKRoundRect(rect, s.CornerRadius, s.CornerRadius);
        canvas.DrawRoundRect(r, p);
        canvas.Restore();
    }

    private void DrawShotArc(SKCanvas canvas, FastBoard.Core.Tools.ShotArcShape s)
    {
        using var p = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = s.Thickness, IsAntialias = true };
        var rect = new SKRect(s.Center.X - s.Radius, s.Center.Y - s.Radius, s.Center.X + s.Radius, s.Center.Y + s.Radius);
        canvas.DrawArc(rect, s.StartAngleDeg, s.SweepDeg, false, p);
    }

    private void DrawStroke(SKCanvas canvas, FastBoard.Core.Models.StrokeShape st)
    {
        if (st.Points == null || st.Points.Count < 2)
            return;

        using var p = new SKPaint
        {
            Color = SKColors.White,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
        };

        for (int i = 1; i < st.Points.Count; i++)
        {
            canvas.DrawLine(
                new SKPoint(st.Points[i - 1].X, st.Points[i - 1].Y),
                new SKPoint(st.Points[i].X, st.Points[i].Y),
                p);
        }
    }

    private void DrawSelection(SKCanvas canvas, SKRect rect)
    {
        using var p = new SKPaint { Color = SKColors.Yellow, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new float[] { 6, 6 }, 0), IsAntialias = true };
        canvas.DrawRect(rect, p);
    }

    private static void DrawHandle(SKCanvas canvas, SKPoint p)
    {
        using var fill = new SKPaint { Color = SKColors.Yellow, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var stroke = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        var r = new SKRect(p.X - 5, p.Y - 5, p.X + 5, p.Y + 5);
        canvas.DrawRect(r, fill);
        canvas.DrawRect(r, stroke);
    }

    private void DrawArrowHandles(SKCanvas canvas, FastBoard.Core.Models.ArrowShape a)
    {
        DrawHandle(canvas, new SKPoint(a.Start.X, a.Start.Y));
        DrawHandle(canvas, new SKPoint(a.End.X, a.End.Y));
    }

    private void DrawCurveHandles(SKCanvas canvas, FastBoard.Core.Tools.CurveShape c)
    {
        DrawHandle(canvas, new SKPoint(c.P0.X, c.P0.Y));
        DrawHandle(canvas, new SKPoint(c.P1.X, c.P1.Y));
        DrawHandle(canvas, new SKPoint(c.P2.X, c.P2.Y));
    }

        private void DrawScreenHandles(SKCanvas canvas, FastBoard.Core.Tools.ScreenShape s)
    {
        var half = new SKPoint(s.Size.X / 2, s.Size.Y / 2);

        // Compute the four corners BEFORE rotation
        var nw = new SKPoint(-half.X, -half.Y);
        var ne = new SKPoint( half.X, -half.Y);
        var sw = new SKPoint(-half.X,  half.Y);
        var se = new SKPoint( half.X,  half.Y);

        // Apply rotation + translation
        SKPoint Rotate(SKPoint p, float deg)
        {
            float rad = deg * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            return new SKPoint(
                p.X * cos - p.Y * sin + s.Center.X,
                p.X * sin + p.Y * cos + s.Center.Y
            );
        }

        var rNW = Rotate(nw, s.Rotation);
        var rNE = Rotate(ne, s.Rotation);
        var rSW = Rotate(sw, s.Rotation);
        var rSE = Rotate(se, s.Rotation);

        DrawHandle(canvas, rNW);
        DrawHandle(canvas, rNE);
        DrawHandle(canvas, rSW);
        DrawHandle(canvas, rSE);
    }


    private void DrawTokenHandles(SKCanvas canvas, FastBoard.Core.Models.Token t, float radius)
    {
        DrawHandle(canvas, new SKPoint(radius, radius));
        DrawHandle(canvas, new SKPoint(0, -radius - 20));
    }

    private void DrawToken(SKCanvas canvas, FastBoard.Core.Models.Token t)
    {
        var tokenColor = new SKColor((byte)((t.Color >> 16) & 0xFF), (byte)((t.Color >> 8) & 0xFF), (byte)(t.Color & 0xFF), (byte)((t.Color >> 24) & 0xFF));

        canvas.Save();
        canvas.Translate(t.Position.X, t.Position.Y);
        canvas.RotateDegrees(t.Rotation);

        if (string.IsNullOrEmpty(t.ImagePath) && (t.ImageBytes == null || t.ImageBytes.Length == 0))
        {
            using var p = new SKPaint { Color = tokenColor, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawCircle(0, 0, 20 * t.Scale, p);
            if (t.Selected) DrawTokenHandles(canvas, t, 20 * t.Scale);
            canvas.Restore();
            return;
        }

        string key = !string.IsNullOrEmpty(t.ImagePath) ? t.ImagePath : $"mem:{t.ImageBytes?.Length}:{t.ImageBytes?.GetHashCode()}";

        if (!_bitmapCache.TryGetValue(key, out var bmp))
        {
            try
            {
                if (!string.IsNullOrEmpty(t.ImagePath))
                {
                    using var fs = File.OpenRead(t.ImagePath);
                    bmp = SKBitmap.Decode(fs);
                }
                else if (t.ImageBytes != null)
                {
                    bmp = SKBitmap.Decode(t.ImageBytes);
                }
                if (bmp != null) _bitmapCache[key] = bmp;
            }
            catch { }
        }

        if (bmp != null)
        {
            var w = bmp.Width * t.Scale;
            var h = bmp.Height * t.Scale;
            var dest = new SKRect(-w / 2, -h / 2, w / 2, h / 2);
            canvas.DrawBitmap(bmp, dest);
            if (t.Selected) DrawTokenHandles(canvas, t, Math.Max(w, h) / 2);
        }

        canvas.Restore();
    }

    public void ZoomIn()  { _zoom *= 1.1f; SkCanvas.Invalidate(); }
public void ZoomOut() { _zoom /= 1.1f; SkCanvas.Invalidate(); }
public void ZoomReset() { _zoom = 1f; _pan = new SKPoint(0,0); SkCanvas.Invalidate(); }
private void OnManipulationStarted(object sender, Microsoft.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
    {
        _isManipulating = true;
        _startZoom = _zoom;

        _manipulationCenter = new SKPoint(
            (float)e.Position.X - _pan.X,
            (float)e.Position.Y - _pan.Y
        );
    }

    private void OnManipulationDelta(object sender, Microsoft.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
    {
        if (!_isManipulating)
            return;

        if (e.Delta.Scale != 0)
        {
            float oldZoom = _zoom;
            _zoom = Math.Clamp(_zoom * (float)e.Delta.Scale, 0.2f, 5f);

            var mx = (float)e.Position.X;
            var my = (float)e.Position.Y;

            _pan.X = mx - (_manipulationCenter.X * _zoom);
            _pan.Y = my - (_manipulationCenter.Y * _zoom);
        }

        _pan.X += (float)e.Delta.Translation.X;
        _pan.Y += (float)e.Delta.Translation.Y;

        SkCanvas.Invalidate();
    }

    private void OnManipulationCompleted(object sender, Microsoft.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
    {
        _isManipulating = false;
    }

    private void OnDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        // Reset zoom/pan on double tap
        _zoom = 1f;
        _pan = new SKPoint(0, 0);
        SkCanvas?.Invalidate();
    }

    private void OnPointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(SkCanvas);
        int delta = pt.Properties.MouseWheelDelta;
        if (delta == 0)
            return;

        float factor = delta > 0 ? 1.1f : (1f / 1.1f);

        // Zoom about pointer position
        var mx = (float)pt.Position.X;
        var my = (float)pt.Position.Y;

        var wx = (mx - _pan.X) / _zoom;
        var wy = (my - _pan.Y) / _zoom;

        _zoom = Math.Clamp(_zoom * factor, 0.2f, 5f);

        _pan.X = mx - wx * _zoom;
        _pan.Y = my - wy * _zoom;

        SkCanvas?.Invalidate();
    }

    private SKBitmap? LoadBitmap(string key, string path)
    {
        if (_bitmapCache.TryGetValue(key, out var bmp))
            return bmp;

        if (!File.Exists(path))
            return null;

        using var fs = File.OpenRead(path);
        var loaded = SKBitmap.Decode(fs);

        if (loaded != null)
            _bitmapCache[key] = loaded;

        return loaded;
    }
}
