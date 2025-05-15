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
            ExhibitLB.ItemsSource = Helper.Exhibits;
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

        private async void ComfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!DateOnly.TryParse(BDay.Text, out DateOnly _))
            {
                Helper.CallMessageBox("Укажите дату рождения в формате день месяц год через точки", this);
                return;
            }
            if (Password.Text != Helper.currentUser.Password)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Предосторежение", "Вы точно хотите изменить пароль?", ButtonEnum.YesNo);
                var result = await box.ShowAsync();
                if (!result.Equals(ButtonResult.Yes))
                {
                    return;
                }
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
            if (UserLB.SelectedItems!.Count > 0)
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
                    UserLB.ItemsSource = Helper.Users.Where(u => u.RoleId != 1); ;
                }
            }
        }

        DateTime startDate = new DateTime();
        DateTime endDate = new DateTime();
        private void ReportCB_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            DateSelector.IsVisible = false;
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    startDate = DateTime.Now.AddDays(-DateTime.Now.Day);
                    endDate = DateTime.Now.AddDays(-DateTime.Now.Day).AddMonths(1);
                    break;
                case 1:
                    startDate = DateTime.Now.AddMonths(-DateTime.Now.Month);
                    endDate = DateTime.Now.AddMonths(-DateTime.Now.Month).AddYears(1);
                    break;
                case 2:
                    DateSelector.IsVisible = true;
                    break;
                default: 
                    return;

            }
        }

        private void MakeReportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ReportCB.SelectedIndex == -1)
            {
                Helper.CallMessageBox("Выберите период отчёта", this);
                return;
            }
            else if (ReportCB.SelectedIndex == 2)
            {
                if (StartDate.SelectedDate == null || StartDate.SelectedDate == null || StartDate.SelectedDate < EndDate.SelectedDate)
                {
                    Helper.CallMessageBox("Выберите корректный период отчёта", this);
                    return;
                }
                startDate = StartDate.SelectedDate.Value.DateTime;
                endDate = EndDate.SelectedDate.Value.DateTime;
            }

        }
    }
}