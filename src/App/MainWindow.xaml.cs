using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using FastBoard.Core.Serialization;
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
    }
}