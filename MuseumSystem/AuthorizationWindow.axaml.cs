using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using SkiaSharp;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MuseumSystem;

public partial class AuthorizationWindow : Window
{
    public AuthorizationWindow()
    {
        InitializeComponent();
        BDay.DisplayDateEnd = System.DateTime.Now.AddYears(-12);
        Gender.ItemsSource = Helper.Genders;
        Gender.SelectedIndex = 2;
        //Helper.DBContext.MediaTypes.Add(new Models.MediaType() { Id = 3, Name = "Видео"});
        //Helper.DBContext.SaveChanges();
        //Helper.DBContext.AtachedMedia.Add(new Models.AtachedMedium() { Id = 3, ExhibitId = 1, Path = "sample-5s.mp4", Description="aaa", TypeId = 3 });
        //Helper.DBContext.SaveChanges();
    }
    // Действия при авторизации
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
    // Переключение авторизации на регистрацию и обратно
    private void Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RightTop.IsVisible = !RightTop.IsVisible;
        LeftTop.IsVisible = !LeftTop.IsVisible;
        Auth.IsVisible = !Auth.IsVisible;
        Reg.IsVisible = !Reg.IsVisible;
    }
    // Действия при регистрации
    private async void Register_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (RegPassword.Text != RegPasswordCheck.Text)
        {
            Helper.CallMessageBox("Проверьте правильность пароля и попробуйте заново" ,this);
            return;
        }

        if (!DateOnly.TryParse(BDay.Text, out _))
        {
            Helper.CallMessageBox("Укажите дату рождения в формате день месяц год через точки", this);
            return;
        }
        var result = await Helper.CanRegister(new Models.User()
        {
            FirstName = FirstName.Text,
            LastName = LastName.Text,
            Patronymic = Patronymic.Text,
            Email = Email.Text,
            Login = Login.Text,
            PhoneNumber = Phone.Text,
            GenderId = (Gender.SelectedItem as Models.Gender).Id,
            Birthday = DateOnly.Parse(BDay.Text),
            Password = RegPassword.Text,
            RoleId = 3
        }, this);
        if (result)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
    // ЧТобы вмещался текст в окно
    private void Window_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        Window window = sender as Window;
        if (window.Height < 550 || window.Width < 1125)
        {
            MainText1.Text = "Зарегестрируйтесь, или авторизируйтесь по кнопке";
            MainText2.Text = "Авторизируйтесь, или зарегестрируйтесь по кнопке";
        }
        else
        {
            MainText1.Text = "Пожалуйста, зарегестрируйтесь для продолжения, либо если вы уже регестрировали его, то авторизуйтесь";
            MainText2.Text = "Пожалуйста, авторизуйтесь для продолжения, либо если в нашей системе нет вашего аккаунта, то зарегестрируйте его";
        }
    }



}