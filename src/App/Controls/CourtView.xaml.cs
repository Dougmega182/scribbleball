using FastBoard.Core.Geometry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace FastBoard.Controls;

public sealed partial class CourtView : UserControl
{
    public CourtView()
    {
        this.InitializeComponent();
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
        float s = Math.Min(availW / (float)courtW, availH / (float)courtL);
        float offsetX = (availW - (float)courtW * s) / 2f;
        float offsetY = (availH - (float)courtL * s) / 2f;

        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true, StrokeWidth = 3, Style = SKPaintStyle.Stroke };

        // Outer boundary
        var rect = new SKRect(offsetX, offsetY, offsetX + (float)courtW * s, offsetY + (float)courtL * s);
        canvas.DrawRect(rect, paint);

        // Center line for full court
        if (!HalfCourt)
        {
            canvas.DrawLine(rect.MidX, rect.Top, rect.MidX, rect.Bottom, paint);
        }

        // Hoop and backboard on one end (top for half court)
        float hoopX = rect.MidX;
        float hoopY = HalfCourt ? rect.Top + 60 : rect.Top + 60; // placeholder offset
        canvas.DrawCircle(hoopX, hoopY, 9, paint);
        canvas.DrawLine(hoopX - 30, hoopY + 12, hoopX + 30, hoopY + 12, paint);

        // Free-throw semicircle and key
        var laneWidth = (float)(CourtGeometry.FtToPx(CourtGeometry.LaneWidthFt, dpi, scale) * s);
        float laneLeft = rect.MidX - laneWidth / 2f;
        float laneRight = rect.MidX + laneWidth / 2f;
        float laneTop = hoopY + 12 + 12; // baseline to key approximate
        float ftDist = (float)(CourtGeometry.FtToPx(CourtGeometry.FreeThrowLineDistFt, dpi, scale) * s);
        float ftY = laneTop + ftDist;
        canvas.DrawRect(laneLeft, laneTop, laneWidth, ftDist, paint);
        var ftRadius = (float)(CourtGeometry.FtToPx(6, dpi, scale) * s);
        var ftRect = new SKRect(rect.MidX - ftRadius, ftY - ftRadius, rect.MidX + ftRadius, ftY + ftRadius);
        canvas.DrawArc(ftRect, 0, 180, false, paint);

        // Restricted area arc
        var restR = (float)(CourtGeometry.RestrictedArcRadiusPx(dpi, scale) * s);
        var restRect = new SKRect(hoopX - restR, hoopY - restR, hoopX + restR, hoopY + restR);
        canvas.DrawArc(restRect, 200, 140, false, paint);

        // 3-pt arc and corners
        var threeR = (float)(CourtGeometry.ThreePointArcRadiusPx(dpi, scale) * s);
        var threeRect = new SKRect(hoopX - threeR, hoopY - threeR, hoopX + threeR, hoopY + threeR);
        canvas.DrawArc(threeRect, 210, 120, false, paint);
        // Corner lines (approx)
        canvas.DrawLine(laneLeft - 40, hoopY + 12, laneLeft - 40, hoopY + 200, paint);
        canvas.DrawLine(laneRight + 40, hoopY + 12, laneRight + 40, hoopY + 200, paint);
    }
}
