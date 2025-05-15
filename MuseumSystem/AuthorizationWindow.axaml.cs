using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using System;
using System.Linq;

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
    // �������� ��� �����������
    private void Login_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(FirstRow.Text) || string.IsNullOrEmpty(Password.Text))
        {
            Helper.CallMessageBox("����������, ��������� ������ ��� �����", this);
        }
        if (Helper.IsExist(FirstRow.Text, Password.Text))
        {
            new MainWindow().Show();
            this.Close();
        }
        else
        {
            Helper.CallMessageBox("���-�� ����� �� ���. ���������� ��������� ����� � ������ � ���������� ������", this);
        }
    }
    // ������������ ����������� �� ����������� � �������
    private void Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RightTop.IsVisible = !RightTop.IsVisible;
        LeftTop.IsVisible = !LeftTop.IsVisible;
        Auth.IsVisible = !Auth.IsVisible;
        Reg.IsVisible = !Reg.IsVisible;
    }
    // �������� ��� �����������
    private void Register_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (RegPassword.Text != RegPasswordCheck.Text)
        {
            Helper.CallMessageBox("��������� ������������ ������ � ���������� ������" ,this);
        }

        if (!DateOnly.TryParse(BDay.Text, out _))
        {
            Helper.CallMessageBox("������� ���� �������� � ������� ���� ����� ��� ����� �����", this);
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
    // ����� �������� ����� � ����
    private void Window_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        Window window = sender as Window;
        if (window.Height < 550 || window.Width < 1125)
        {
            MainText1.Text = "�����������������, ��� ��������������� �� ������";
            MainText2.Text = "���������������, ��� ����������������� �� ������";
        }
        else
        {
            MainText1.Text = "����������, ����������������� ��� �����������, ���� ���� �� ��� �������������� ���, �� �������������";
            MainText2.Text = "����������, ������������� ��� �����������, ���� ���� � ����� ������� ��� ������ ��������, �� ��������������� ���";
        }
    }
}