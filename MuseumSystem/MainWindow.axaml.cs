using Avalonia.Controls;
using MuseumSystem.Models;

namespace MuseumSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EventLB.ItemsSource = Helper.Events;
            TicketLB.ItemsSource = Helper.Tickets;
            ExhibitLB.ItemsSource= Helper.Exhibits;
        }

        private void AddExhibitionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new ExhibitWindow().Show();
            this.Close();
        }

        private void Border_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            new ExhibitWindow(ExhibitLB.SelectedItem as Exhibit).Show();
            this.Close();
        }

        private void TicketButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new TicketWindow().Show();
            this.Close();
        }
    }
}