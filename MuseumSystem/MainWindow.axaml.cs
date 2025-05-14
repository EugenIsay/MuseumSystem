using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            UserLB.ItemsSource = Helper.Users.Where(u => u.RoleId != 1);

            Login.Text = Helper.currentUser.Login;
            FirstName.Text = Helper.currentUser.FirstName;
            LastName.Text = Helper.currentUser.LastName;
            Patronymic.Text = Helper.currentUser.Patronymic;
            Email.Text = Helper.currentUser.Email;
            Phone.Text = Helper.currentUser.PhoneNumber;
            BDay.SelectedDate = Helper.currentUser.Birthday.ToDateTime(new TimeOnly());
            Gender.SelectedItem = Helper.currentUser.Gender;
            Password.Text = Helper.currentUser.Password;
            MainTab.SelectedIndex = Helper.Page;
            ReadyButton.IsVisible = false;
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
                if (EventLB.SelectedItem as Event == null)
                {
                    return;
                }
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
            Helper.Page = 0;
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

        private void TabControl_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (MainTab != null)
            {
                Helper.Page = MainTab.SelectedIndex;
                ReadyButton.IsVisible = false;
            }
        }

        private void UserLB_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (UserLB.SelectedItems.Count > 0)
                BlockButton.IsVisible = true;
            else
                BlockButton.IsVisible = false;
        }

        private async void BlockButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var box = MessageBoxManager
            .GetMessageBoxStandard("Предосторежение", "Вы точно хотите разблокировать/заблокировать данных пользователей?",
                ButtonEnum.YesNo);
            var result = await box.ShowAsync();
            if (result.Equals(ButtonResult.Yes))
            {
                var Users = UserLB.SelectedItems;
                foreach (var user in Users)
                {
                    Helper.ChangeUserBool(user as User);
                }
            }
        }
    }
}