using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MuseumSystem.Models;
using System;
using System.Linq;

namespace MuseumSystem;

public partial class TicketWindow : Window
{
    decimal[] FullPrice = new decimal[2];
    int TicketLasts = 0;
    DateTimeOffset Start = DateTimeOffset.MinValue;
    Guid Guid = new Guid();
    public TicketWindow()
    {
        InitializeComponent();
        EventLB.ItemsSource = Helper.Events.Where(e => e.StartDatetime > DateTime.Now && e.EventRegistrations.Count < e.MaxAttendees);
        TypeCB.ItemsSource = Helper.TicketTypes;
        StartDate.MinYear = DateTime.Now;
        Number.Text = Guid.NewGuid().ToString();
        UserCB.ItemsSource = Helper.Users;
        UserCB.SelectedItem = Helper.currentUser;
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        FullPrice[0] = (decimal)(sender as TicketType).Price!;
        TicketLasts = (int)(sender as TicketType).ValidityDays!;
        Check();
    }

    private void DatePicker_SelectedDateChanged(object? sender, Avalonia.Controls.DatePickerSelectedValueChangedEventArgs e)
    {
        Start = (DateTimeOffset)StartDate.SelectedDate;
        Check();
    }
    public void Check()
    {
        if (TicketLasts != 0 && Start != null)
        {
            EndDate.SelectedDate = Start.AddDays(TicketLasts);
        }
        Price.Text = FullPrice.Sum().ToString();
    }

    private void ListBox_SelectionChanged_1(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        FullPrice[1] = 0;
        foreach (var item in EventLB.SelectedItems) 
        {
            FullPrice[1] += (decimal)(item as Event).Price!;
        }
        Check();
    }
    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new MainWindow().Show();
        this.Close();
    }

    private void ComfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TypeCB.SelectedIndex == -1)
        {
            Helper.CallMessageBox("Выберите тип билета", this);
        }
        if (StartDate.SelectedDate == null)
        {
            Helper.CallMessageBox("Выберите дату начала", this);
        }
        if(EventLB.SelectedItems.Count == 0)
        {
            Helper.CallMessageBox("Выберите мероприятия которые хотите посетить", this);
        }
        if (Helper.AddTickets(new Ticket { Number = Number.Text, TypeId = (TypeCB.SelectedItem as TicketType).Id, ValidFrom = DateOnly.FromDateTime(StartDate.SelectedDate.Value.Date), ValidTo = DateOnly.FromDateTime(EndDate.SelectedDate.Value.Date), Price = FullPrice.Sum() }))
        {
            new MainWindow().Show();
            this.Close();
        }
        else
        {
            Helper.CallMessageBox("Что-то пошло не так, повторите попытку", this);
        }
    }

}