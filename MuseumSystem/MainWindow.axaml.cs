using Avalonia.Controls;
using MsBox.Avalonia;
using MuseumSystem.Models;
using System;
using Tmds.DBus.Protocol;

namespace MuseumSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EventLB.ItemsSource = Helper.ShownEvents;
            TicketLB.ItemsSource = Helper.Tickets;
            ExhibitLB.ItemsSource= Helper.Exhibits;
            BDay.DisplayDateEnd = System.DateTime.Now.AddYears(-12);
            Gender.ItemsSource = Helper.Genders;

            Login.Text = Helper.currentUser.Login;
            FirstName.Text = Helper.currentUser.FirstName;
            LastName.Text = Helper.currentUser.LastName;
            Patronymic.Text = Helper.currentUser.Patronymic;
            Email.Text = Helper.currentUser.Email;
            Phone.Text = Helper.currentUser.PhoneNumber;
            BDay.SelectedDate = Helper.currentUser.Birthday.ToDateTime(new TimeOnly());
            Gender.SelectedItem = Helper.currentUser.Gender;
            Password.Text = Helper.currentUser.Password;
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

        private void EventButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new EventWindow().Show();
            this.Close();
        }

        private void Border_DoubleTapped_1(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (Helper.IsEmployee)
            {
                new EventWindow(EventLB.SelectedItem as Event).Show();
                this.Close();
            }    
        }

        private void ComfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!DateOnly.TryParse(BDay.Text, out DateOnly result))
            {
                Helper.CallMessageBox("Укажите дату рождения в формате день месяц год через точки", this);
                return;
            }
            if (Helper.CanRegister(new User()
            {
                FirstName = FirstName.Text,
                LastName = LastName.Text,
                Patronymic = Patronymic.Text,
                Email = Email.Text,
                Login = Login.Text,
                PhoneNumber = Phone.Text,
                GenderId = (Gender.SelectedItem as Models.Gender).Id,
                Birthday = DateOnly.Parse(BDay.Text),
                Password = Password.Text,
                RoleId = 3
            }, this))
            {
                MessageBoxManager.GetMessageBoxStandard("Готвоо", "Пользователь успешно сохранён").ShowWindowDialogAsync(this);
            }
        }

        private void ExitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new AuthorizationWindow().Show();
            Helper.currentUser = null;
            this.Close();
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            ReadyButton.IsVisible = true;
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            ReadyButton.IsVisible = true;
        }
    }
}