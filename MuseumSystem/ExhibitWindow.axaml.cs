using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using System;
using MuseumSystem.Models;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using MsBox.Avalonia;
using Tmds.DBus.Protocol;
using System.Collections.Generic;
using MuseumSystem.Models;
using System.IO;
using Avalonia.Media;

namespace MuseumSystem;

public partial class ExhibitWindow : Window
{
    private LibVLC MainLibVLC { get; set; }
    private MediaPlayer MainMediaPlayer { get; set; }

    private Exhibit exhibit { get; set; }

    List<AtachedMedium> Media = new List<AtachedMedium>();

    public List<AtachedMedium> AudioMedia
    {
        get
        {
            return Media.Where(m => m.TypeId == 2).ToList();
        }
    }
    public List<AtachedMedium> PhotoMedia
    {
        get
        {
            return Media.Where(m => m.TypeId == 1).ToList();
        }
    }
    public List<AtachedMedium> VideoMedia
    {
        get
        {
            return Media.Where(m => m.TypeId == 3).ToList();
        }
    }
    private bool isEdit = Helper.IsEmployee;

    public bool HasComment
    {
        get => Helper.currentUser.ExhibitReviews.Where(r => r.ExhibitId == exhibit.Id).Count() > 0;
    }

    public List<ExhibitReview> reviews
    {
        get => exhibit.ExhibitReviews.Where(r => r.UserId != Helper.currentUser.Id).ToList();
    }
    public ExhibitWindow()
    {
        InitializeComponent();
        EditCategoryCB.ItemsSource = Helper.Categories;
        CommentSection.IsVisible = false;
        AddComment.IsVisible = false;
        RedactComment.IsVisible = false;
        InitMediaPlayer();
    }
    public ExhibitWindow(Exhibit Exhibit)
    {
        exhibit = Exhibit;
        Media = new List<AtachedMedium>((List<AtachedMedium>)Exhibit.AtachedMedia.ToList());
        InitializeComponent();
        EditCategoryCB.ItemsSource = Helper.Categories;
        ShowExhibit();
        ImageShow.Source = exhibit.MainImageBitmap;
        PhotoList.ItemsSource = PhotoMedia;
        AudioList.ItemsSource = AudioMedia;
        VideoList.ItemsSource = VideoMedia;
        AddComment.IsVisible = !HasComment;
        RedactComment.IsVisible = HasComment;
        if (reviews.Count() > 3)
        {
            ReviewLB.ItemsSource = RandomComments(reviews.Count).Select(c => reviews[c]).ToList();
        }
        else
        {
            ReviewLB.ItemsSource = reviews;
        }
        InitMediaPlayer();
    }
    public void ShowExhibit()
    {
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
                case string s when s.Contains("Name"):
                    Element.Text = exhibit.Name;
                    break;
                case string s when s.Contains("IN"):
                    Element.Text = exhibit.InventoryNumber;
                    break;
                case string s when s.Contains("Category"):
                    if (Element.Name.Contains("CB"))
                        Element.SelectedItem = exhibit.Category;
                    else
                        Element.Text = exhibit.Name;
                    break;
                case string s when s.Contains("Cost"):
                    if (Element.Name.Contains("Edit"))
                        Element.Value = exhibit.ApproximateCost;
                    else if (Element.Name.Contains("Show"))
                        Element.Text = exhibit.ApproximateCost.ToString();
                    break;
                case string s when s.Contains("Location"):
                    Element.Text = exhibit.PermanentlyLocated;
                    break;
                case string s when s.Contains("Conditin"):
                    Element.Text = exhibit.Condition;
                    break;
                case string s when s.Contains("Description"):
                    Element.Text = exhibit.Description;
                    break;
            }
        }
    }



    private void InitMediaPlayer()
    {
        MainLibVLC = new(enableDebugLogs: true);
        MainMediaPlayer = new(MainLibVLC);

        Control mediaPlayerControl = App.AppNativeVideoPlayerService.CreateControl();

        mediaPlayerControl.Width = 400;
        mediaPlayerControl.Height = 300;

        VideoContainer.Children.Clear();
        VideoContainer.Children.Add(mediaPlayerControl);
    }
    public void PlayButton_Click(object sender, RoutedEventArgs args)
    {
        AtachedMedium medium = AudioMedia.FirstOrDefault(media => media.Id == int.Parse((sender as Button)!.Tag!.ToString()!))!;
        Media media;
        if (medium.TempPath == null)
        {
            media = new(MainLibVLC, new Uri(Environment.CurrentDirectory + "/Audio/" + medium.Path));
        }
        else
        {
            media = new(MainLibVLC, new Uri(medium.TempPath));
        }
        MainMediaPlayer.Media = media;
        MainMediaPlayer.Play();
    }

    private void ReadyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Exhibit EditedExhibit;
        if (exhibit == null)
            EditedExhibit = new Exhibit();
        else
            EditedExhibit = exhibit;
        if (string.IsNullOrEmpty(EditName.Text))
        {
            Helper.CallMessageBox("Введите название экспоната", this);
            return;
        }
        if (string.IsNullOrEmpty(EditIN.Text))
        {
            Helper.CallMessageBox("Введите инвентраный номер экспоната", this);
            return;
        }
        if (EditCategoryCB.SelectedIndex == -1)
        {
            Helper.CallMessageBox("Выберите категорию экспоната", this);
            return;
        }
        if (EditCost.Value == null)
        {
            Helper.CallMessageBox("Укажите предположительную цену экспоната", this);
            return;
        }
        if (string.IsNullOrEmpty(EditLocation.Text))
        {
            Helper.CallMessageBox("Выберите местоположение экмпоната", this);
            return;
        }
        if (string.IsNullOrEmpty(EditConditin.Text))
        {
            Helper.CallMessageBox("Укажите состояние экспоната", this);
            return;
        }
        if (string.IsNullOrEmpty(EditDescription.Text))
        {
            Helper.CallMessageBox("Опишите экспонат", this);
            return;
        }
        EditedExhibit.Name = EditName.Text;
        EditedExhibit.InventoryNumber = EditIN.Text;
        EditedExhibit.Description = EditDescription.Text;
        EditedExhibit.Condition = EditConditin.Text;
        EditedExhibit.CategoryId = (EditCategoryCB.SelectedItem as Category)!.Id;
        EditedExhibit.ApproximateCost = EditCost.Value;
        EditedExhibit.PermanentlyLocated = EditLocation.Text;
        if (!string.IsNullOrEmpty(_imageName))
        {
            try
            {
                File.Copy(_imagePath, Environment.CurrentDirectory + "/Pictures/" + _imageName);
                EditedExhibit.MainImage = _imageName;
            }
            catch
            { }
        }
        if (Helper.EditExhibits(EditedExhibit))
        {
            var addedMedia = Media.Except(EditedExhibit.AtachedMedia).ToList();
            var removedMedia = EditedExhibit.AtachedMedia.Except(Media).ToList();
            foreach (var media in addedMedia)
            {
                media.ExhibitId = EditedExhibit.Id;
                if (media.TypeId == 1)
                    File.Copy(media.TempPath, Environment.CurrentDirectory + "/Pictures/" + media.Path);
                else if (media.TypeId == 2)
                    File.Copy(media.TempPath, Environment.CurrentDirectory + "/Audio/" + media.Path);
                else if (media.TypeId == 3)
                    File.Copy(media.TempPath, Environment.CurrentDirectory + "/Video/" + media.Path);
                media.TempPath = null;
                Helper.AddMedia(media);
            }
            foreach (var media in removedMedia)
            {
                if (media.TypeId == 1)
                    File.Delete(Environment.CurrentDirectory + "/Pictures/" + media.Path);
                else if (media.TypeId == 2)
                    File.Delete(Environment.CurrentDirectory + "/Audio/" + media.Path);
                else if (media.TypeId == 3)
                    File.Delete(Environment.CurrentDirectory + "/Video/" + media.Path);
                Helper.RemoveMedia(media);
            }
            MessageBoxManager.GetMessageBoxStandard("Успех", "Всё прошло хорошо").ShowWindowDialogAsync(this);
            exhibit = Helper.Exhibits.FirstOrDefault(e => e.Id == EditedExhibit.Id)!;
            Media = (List<AtachedMedium>)exhibit.AtachedMedia;
            CommentSection.IsVisible = true;
            AddComment.IsVisible = !HasComment;
            RedactComment.IsVisible = HasComment;
        }
        else
        {
            Helper.CallMessageBox("Неизвестная ошибка", this);
        }


    }

    private void RedactButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        isEdit = !isEdit;
        ReadyButton.IsVisible = isEdit;
        AddVideo.IsVisible = isEdit;
        AddAudio.IsVisible = isEdit;
        AddPhoto.IsVisible = isEdit;
        ShowExhibit();
    }
    string _imageName = "";
    string _imagePath = "";
    private async void MediaButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        var window = new MediaWindow();
        AtachedMedium result;
        try
        {
            result = await window.ShowDialog<AtachedMedium>(this);
            if (result == null)
            {
                return;
            }
            if (Media.Count() == 0)
                result.Id = 1;
            else
                result.Id = Media.Select(m => m.Id).Order().Last() + 1;
            Media.Add(result);
            PhotoList.ItemsSource = PhotoMedia;
            AudioList.ItemsSource = AudioMedia;
            VideoList.ItemsSource = VideoMedia;
        }
        catch
        {
            return;
        }
    }

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

    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new MainWindow().Show();
        this.Close();
    }

    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Media.Remove(Media.FirstOrDefault(m => m.Id == int.Parse((sender as Button).Tag.ToString())));
        PhotoList.ItemsSource = PhotoMedia;
        AudioList.ItemsSource = AudioMedia;
        VideoList.ItemsSource = VideoMedia;
    }

    private void VideoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AtachedMedium medium = VideoMedia.FirstOrDefault(media => media.Id == int.Parse((sender as Button)!.Tag!.ToString()!))!;
        Media media;
        if (medium.TempPath == null)
        {

            App.AppNativeVideoPlayerService.Play($"{Environment.CurrentDirectory + "/Video/" + medium.Path}");
        }
        else
        {

            App.AppNativeVideoPlayerService.Play($"{medium.TempPath}");
        }
    }

    private void Window_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if ((sender as Window).Width < 960)
        {
            UGrid.Columns = 1;
        }
        else
        {
            UGrid.Columns = 2;
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
        new CommentWindow(reviews.Concat(Helper.currentUser.ExhibitReviews.Where(e => e.ExhibitId == exhibit.Id)).ToList()).ShowDialog(this);
    }

    private void EditCommentButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedValue == 0)
        {
            Helper.CallMessageBox("Выберите оценку", this);
            return;
        }    
        Helper.EditExhibitComment(new ExhibitReview { Raiting = SelectedValue, ExhibitId = exhibit.Id, UserId = Helper.currentUser.Id, Review = ReviewBox.Text });
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


}