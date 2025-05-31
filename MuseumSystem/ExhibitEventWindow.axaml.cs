using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MuseumSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace MuseumSystem;

public partial class ExhibitEventWindow : Window
{
    List<Exhibit> returnExhibits = new List<Exhibit>();
    public List<Exhibit> Exhibits { get => returnExhibits.ToList(); }
    public ExhibitEventWindow()
    {
        InitializeComponent();
    }
    public ExhibitEventWindow(List<Exhibit> UsedExhibits)
    {
        returnExhibits = new List<Exhibit>(UsedExhibits);
        InitializeComponent();
        MainExhibitLB.ItemsSource = Helper.Exhibits.Except(Exhibits);
        ExhibitLB.ItemsSource = Exhibits;
    }
    private void Border_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {

        ((sender as Border).Child as Button).IsVisible = true;
        (sender as Border).Background = Brushes.Gray;
        (sender as Border).Opacity = 0.5;
        ((sender as Border).Child as Button).Opacity = 1;
    }

    private void Border_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {

        ((sender as Border).Child as Button).IsVisible = false;
        (sender as Border).Background = Brushes.Transparent;
        (sender as Border).Opacity = 1;
    }

    private void Border_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        returnExhibits.Add(((sender as Border).Tag as Exhibit));
        MainExhibitLB.ItemsSource = Helper.Exhibits.Except(Exhibits);
        ExhibitLB.ItemsSource = Exhibits.ToList();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        returnExhibits.Remove(((sender as Button).Tag as Exhibit));
        MainExhibitLB.ItemsSource = Helper.Exhibits.Except(Exhibits);
        ExhibitLB.ItemsSource = Exhibits;
    }

    private void ReadyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(Exhibits);
    }
}