using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using MuseumSystem.Models;
using System;
using System.Linq;

namespace MuseumSystem;

public partial class EventWindow : Window
{
    Event @event = new Event();
    public EventWindow()
    {
        InitializeComponent();
        OrgCB.ItemsSource = Helper.Users.Where(u => u.RoleId != 3);
        OrgCB.SelectedItem = Helper.currentUser;
        TypeCB.ItemsSource = Helper.EventTypes;
        StartDate.MinYear = DateTime.Now;
        EndDate.MinYear= DateTime.Now;
    }
    public EventWindow(Event @event)
    {
        this.@event = @event;
        InitializeComponent();
        StartDate.MinYear = DateTime.Now;
        EndDate.MinYear = DateTime.Now;
        OrgCB.ItemsSource = Helper.Users.Where(u => u.RoleId != 3);
        TypeCB.ItemsSource = Helper.EventTypes;
        Title.Text = @event.Title;
        TypeCB.SelectedItem = @event.Type;
        OrgCB.SelectedItem = @event.Organizer;
        Addres.Text = @event.Addres;
        Attendance.Value = @event.MaxAttendees;
        Price.Value = @event.Price;
        Description.Text = @event.Description;
        StartDate.SelectedDate = @event.StartDatetime;
        StartTime.SelectedTime = @event.StartDatetime.TimeOfDay;
        EndDate.SelectedDate = @event.EndDatetime;
        try
        {
            EndTime.SelectedTime = ((DateTime)(@event.EndDatetime)).TimeOfDay;
        }
        catch { }
    }
    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new MainWindow().Show();
        this.Close();
    }
    private void ComfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {


        @event.Title = Title.Name; 
        @event.Description = Description.Name;
        @event.Addres = Addres.Text;
        @event.MaxAttendees = (int?)Attendance.Value;
        @event.Price = Price.Value;
        try
        {
            @event.OrganizerId = (OrgCB.SelectedItem as User).Id;
            @event.TypeId = (TypeCB.SelectedItem as EventType).Id;
            @event.StartDatetime = StartDate.SelectedDate.Value.Date + (TimeSpan)StartTime.SelectedTime;
            @event.EndDatetime = EndDate.SelectedDate.Value.Date + (TimeSpan)EndTime.SelectedTime;
        }
        catch { }
        if (Helper.EventEdit(@event, this))
        {
            new MainWindow().Show();
            this.Close();
        }
    }
    
}