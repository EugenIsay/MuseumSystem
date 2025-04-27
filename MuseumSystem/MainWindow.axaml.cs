using Avalonia.Controls;

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

        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new ExhibitWindow().Show();
            this.Close();
        }
    }
}