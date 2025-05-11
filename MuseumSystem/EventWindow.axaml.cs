using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MuseumSystem;

public partial class EventWindow : Window
{
    readonly Event @event = new();
    public EventWindow()
    {
        InitializeComponent();
        ExhibitLB.ItemsSource = Helper.Exhibits;
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
        ExhibitLB.ItemsSource = Helper.Exhibits;
        ExhibitLB.SelectedItems = Helper.Exhibits.Where(e => @event.IncludedExhibits.Contains(e.Id)).ToList();
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
        @event.Title = Title.Text; 
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
            if (@event.Id == 0)
            {
                @event.Id = Helper.Events.Select(e => e.Id).Order().Last() + 1;
            }
            var selectedItems = (ExhibitLB.SelectedItems as List<Exhibit>);
            var wasItems = @event.IncludedItems.Select(e => e.Exhibit);
            var addedItems = selectedItems.Except(wasItems).ToList();
            var removedItems = wasItems.Except(selectedItems).ToList();
            foreach (var item in addedItems)
            {
                Helper.AddEventEhibits(new IncludedItem() { EventId = @event.Id, ExhibitId = item.Id });
            }
            foreach (var item in removedItems)
            {
                Helper.RemoveEventEhibits(new IncludedItem() { EventId = @event.Id, ExhibitId = item.Id });
            }
            new MainWindow().Show();
            this.Close();
        }
    }
    
}