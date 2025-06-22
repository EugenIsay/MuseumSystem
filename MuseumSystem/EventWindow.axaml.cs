using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MuseumSystem;

public partial class EventWindow : Window
{
    Event @event = new();
    List<Exhibit> Exhibits = new List<Exhibit>();
    public List<Exhibit> GetExhibits
    {
        get => Exhibits.ToList();
    }
    public bool isEdit = Helper.IsEmployee;
    public List<EventReview> reviews
    {
        get => @event.EventReviews.Where(r => r.UserId != Helper.currentUser.Id).ToList();
    }
    public bool HasComment
    {
        get => Helper.currentUser.EventReviews.Where(r => r.EventId == @event.Id).Count() > 0;
    }
    public EventWindow()
    {
        InitializeComponent();
        OrgCBEdit.ItemsSource = Helper.Users.Where(u => u.RoleId != 3);
        OrgCBEdit.SelectedItem = Helper.currentUser;
        TypeCBEdit.ItemsSource = Helper.EventTypes;
        CommentSection.IsVisible = false;
        AddComment.IsVisible = false;
        RedactComment.IsVisible = false;
        Redbutton.IsVisible = false;
    }
    public EventWindow(Event @event)
    {
        this.@event = @event;
        InitializeComponent();
        OrgCBEdit.ItemsSource = Helper.Users.Where(u => u.RoleId != 3);
        Exhibits = @event.IncludedItems.Select(e => e.Exhibit).ToList();
        TypeCBEdit.ItemsSource = Helper.EventTypes;
        AddComment.IsVisible = !HasComment;
        RedactComment.IsVisible = HasComment;
        CommentSection.IsVisible = @event.StartDatetime < DateTime.Now;
        CheckIfRedact();
        if (reviews.Count() > 3)
        {
            ReviewLB.ItemsSource = RandomComments(reviews.Count).Select(c => reviews[c]).ToList();
        }
        else
        {
            ReviewLB.ItemsSource = reviews;
        }
        ExhibitLB.ItemsSource = Exhibits;
        ImageShow.Source = @event.MainImageBitmap;
    }
    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new MainWindow().Show();
        this.Close();
    }
    private void ComfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        @event.Title = Title.Text;
        @event.Description = Description.Text;
        @event.Addres = AddresEdit.Text;
        @event.MaxAttendees = (int?)AttendanceEdit.Value;
        @event.Price = PriceEdit.Value;
        try
        {
            @event.OrganizerId = (OrgCBEdit.SelectedItem as User).Id;
            @event.TypeId = (TypeCBEdit.SelectedItem as EventType).Id;
            @event.StartDatetime = StartDate.SelectedDate.Value.Date + (TimeSpan)StartTime.SelectedTime;
            @event.EndDatetime = EndDate.SelectedDate.Value.Date + (TimeSpan)EndTime.SelectedTime;
        }
        catch { }

        if (!string.IsNullOrEmpty(_imageName))
        {
            try
            {
                File.Copy(_imagePath, Environment.CurrentDirectory + "/Pictures/" + _imageName);
                @event.ImageName = _imageName;
            }
            catch
            { }
        }
        if (Helper.EventEdit(@event, this))
        {
            if (@event.Id == 0)
            {
                @event.Id = Helper.Events.Select(e => e.Id).Order().Last() + 1;
            }
            var selectedItems = GetExhibits;
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
            @event = Helper.Events.FirstOrDefault(e => e.Id == @event.Id);
            Redbutton.IsVisible = true;
            MessageBoxManager.GetMessageBoxStandard("Успех", "Всё прошло хорошо").ShowWindowDialogAsync(this);
        }
    }

    private async void AddExhibit_CLick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        var window = new ExhibitEventWindow(Exhibits);
        List<Exhibit> result;
        try
        {
            result = await window.ShowDialog<List<Exhibit>>(this);
            if (result == null)
            {
                return;
            }
            if (result.Count != null)
            {
                Exhibits = result;
                ExhibitLB.ItemsSource = GetExhibits;
            }
        }
        catch
        {
            return;
        }
    }
    string _imageName = "";
    string _imagePath = "";
    private async void PhotoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Выбберите фотографию" });
        if (files.Count() != 0)
        {
            try
            {
                _imagePath = files[0].Path.LocalPath;
                ImageShow.Source = new Bitmap(_imagePath);
                _imageName = $"{Guid.NewGuid()}{_imagePath.Substring(_imagePath.LastIndexOf('.'), _imagePath.Length - _imagePath.LastIndexOf('.'))}";
            }
            catch { }
        }
    }

    private void Border_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (isEdit)
        {
            ((sender as Border).Child as Button).IsVisible = true;
            (sender as Border).Background = Brushes.Gray;
            (sender as Border).Opacity = 0.5;
            ((sender as Border).Child as Button).Opacity = 1;
        }
    }

    private void Border_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (isEdit)
        {
            ((sender as Border).Child as Button).IsVisible = false;
            (sender as Border).Background = Brushes.Transparent;
            (sender as Border).Opacity = 1;
        }
    }
    private void RedactButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        isEdit = !isEdit;
        CheckIfRedact();
    }
    public void CheckIfRedact()
    {
        AddExhibit.IsVisible = isEdit; 
        Title.Text = @event.Title;
        TitleTB.Text = @event.Title;
        Description.Text = @event.Description;
        DescriptionTB.Text = @event.Description;
        StartDate.SelectedDate = @event.StartDatetime;
        StartTime.SelectedTime = @event.StartDatetime.TimeOfDay;
        StartTimeShow.Text = @event.StartDatetime.ToString();
        EndTimeShow.Text = @event.EndDatetime.ToString();
        EndDate.SelectedDate = @event.EndDatetime;
        if (@event.EndDatetime != null)
        {
            EndTime.SelectedTime = @event.EndDatetime.Value.TimeOfDay;
        }
        TitleTB.IsVisible = !isEdit;
        Title.IsVisible = isEdit;
        DescriptionTB.IsVisible = !isEdit;
        Description.IsVisible = isEdit;
        foreach (dynamic Element in InfoGrid.Children)
        {
            if (string.IsNullOrEmpty(Element.Name))
                continue;
            if (Element.Name.Contains("Edit"))
                Element.IsVisible = isEdit;
            else if (Element.Name.Contains("Show"))
                Element.IsVisible = !isEdit;
            switch (Element.Name)
            {
                case string s when s.Contains("Type"):
                    if (Element.Name.Contains("CB"))
                        Element.SelectedItem = @event.Type;
                    else
                        Element.Text = @event.Type.Name;
                    break;
                case string s when s.Contains("Org"):
                    if (Element.Name.Contains("CB"))
                        Element.SelectedItem = @event.Organizer;
                    else
                        Element.Text = @event.Organizer.FullName;
                    break;
                case string s when s.Contains("Addres"):
                    Element.Text = @event.Addres;
                    break;
                case string s when s.Contains("Attendance"):
                    if (Element.Name.Contains("Edit"))
                        Element.Value = @event.MaxAttendees;
                    else if (Element.Name.Contains("Show"))
                        Element.Text = @event.MaxAttendees.ToString();
                    break;
                case string s when s.Contains("Price"):
                    if (Element.Name.Contains("Edit"))
                        Element.Value = @event.Price;
                    else if (Element.Name.Contains("Show"))
                        Element.Text = @event.Price.ToString();
                    break;
            }

        }
    }

    public List<int> RandomComments(int maxValue)
    {
        Random random = new Random();
        return Enumerable.Range(0, maxValue + 1)
                        .OrderBy(x => random.Next())
                        .Take(3)
                        .ToList();
    }

    private void CommentButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new CommentWindow(reviews.Concat(Helper.currentUser.EventReviews.Where(e => e.EventId == @event.Id)).ToList()).ShowDialog(this);
    }

    private void EditCommentButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedValue == 0)
        {
            Helper.CallMessageBox("Выберите оценку", this);
            return;
        }
        Helper.EditEventComment(new EventReview { Raiting = SelectedValue, EventId = @event.Id, UserId = Helper.currentUser.Id, Review = ReviewBox.Text });
        AddComment.IsVisible = false;
        RedactComment.IsVisible = false;
    }
    int SelectedValue = 0;
    private void RadioButton_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton)
        {
            SelectedValue = int.Parse(radioButton.Tag?.ToString());
        }
    }

}