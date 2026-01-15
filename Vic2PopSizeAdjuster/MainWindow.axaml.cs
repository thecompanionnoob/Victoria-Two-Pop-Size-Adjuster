using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System;
using Avalonia.Markup.Xaml;
namespace Vic2PopSizeAdjuster;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
    }
}

