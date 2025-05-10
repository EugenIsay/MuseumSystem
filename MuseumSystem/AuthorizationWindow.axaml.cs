using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using System;

namespace MuseumSystem;

public partial class AuthorizationWindow : Window
{
    public AuthorizationWindow()
    {
        InitializeComponent();
        BDay.DisplayDateEnd = System.DateTime.Now.AddYears(-12);
        Gender.ItemsSource = Helper.Genders;
        Gender.SelectedIndex = 2;
    }

    private void Login_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(FirstRow.Text) || string.IsNullOrEmpty(Password.Text))
        {
            Helper.CallMessageBox("Пожалуйста, заполните данные для входа", this);
        }
        if (Helper.IsExist(FirstRow.Text, Password.Text))
        {
            new MainWindow().Show();
            this.Close();
        }
        else
        {
            Helper.CallMessageBox("Что-то пошло не так. Пожалуйста проверьте логин и пароль и попробуйте заново", this);
        }
    }

    private void Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RightTop.IsVisible = !RightTop.IsVisible;
        LeftTop.IsVisible = !LeftTop.IsVisible;
        
    }

    private void Register_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (RegPassword.Text != RegPasswordCheck.Text)
        {
            Helper.CallMessageBox("Проверьте правильность пароля и попробуйте заново" ,this);
        }
        if (!DateOnly.TryParse(BDay.Text, out DateOnly result))
        {
            Helper.CallMessageBox("Укажите дату рождения в формате день месяц год через точки", this);
            return;
        }
        if (Helper.CanRegister(new Models.User() { FirstName = FirstName.Text,
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
            new MainWindow().Show();
            this.Close();
        }
    }
}