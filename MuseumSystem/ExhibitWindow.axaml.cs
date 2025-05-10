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

namespace MuseumSystem;

public partial class ExhibitWindow : Window
{
    private LibVLC MainLibVLC { get; set; }
    private MediaPlayer MainMediaPlayer { get; set; }

    private Exhibit exhibit { get; set; }

    List<AtachedMedium> Media = new List<AtachedMedium>();

    private bool isEdit = Helper.IsEmployee;
    public ExhibitWindow()
    {
        InitializeComponent();
        EditCategoryCB.ItemsSource = Helper.Categories;
    }
    public ExhibitWindow(Exhibit Exhibit)
    {
        exhibit = Exhibit;
        Media = (List<AtachedMedium>)Exhibit.AtachedMedia;
        InitializeComponent();
        EditCategoryCB.ItemsSource = Helper.Categories;
        ShowExhibit();
        ImageShow.Source = exhibit.MainImageBitmap;
        PhotoList.ItemsSource = exhibit.SpecificMdeia(1);
        AudioList.ItemsSource = exhibit.SpecificMdeia(2);
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
    }
    public void PlayButton_Click(object sender, RoutedEventArgs args)
    {
        string name =  exhibit.SpecificMdeia(2).FirstOrDefault(media => media.Id == int.Parse((sender as Button).Tag.ToString())).Path;
        Media media = new(MainLibVLC, new Uri(Environment.CurrentDirectory + "/Audio/" + name));
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
        if (string.IsNullOrEmpty( EditName.Text))
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
        EditedExhibit.CategoryId = (EditCategoryCB.SelectedItem as Category).Id;
        EditedExhibit.ApproximateCost = EditCost.Value;
        EditedExhibit.PermanentlyLocated = EditLocation.Text;
        if (Helper.EditExhibits(EditedExhibit))
        {
            MessageBoxManager.GetMessageBoxStandard("Успех", "Всё прошло хорошо").ShowWindowDialogAsync(this);
            exhibit = EditedExhibit;
        }
        else
        {
            Helper.CallMessageBox("Опишите экспонат", this);
        }


    }

    private void RedactButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        isEdit = !isEdit;
        ReadyButton.IsVisible = isEdit;
        ShowExhibit();
    }
    string _imageName = "";
    string _imagePath = "";
    private async void MediaButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //var topLevel = TopLevel.GetTopLevel(this);
        //var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Выбберите фотографию" });
        //if (files.Count() != 0)
        //{
        //    try
        //    {
        //        _imagePath = files[0].Path.LocalPath;
        //        Image.Source = new Bitmap(_imagePath);
        //        _imageName = $"Pictures/{Guid.NewGuid()}{_imagePath.Substring(_imagePath.LastIndexOf('.'), _imagePath.Length - _imagePath.LastIndexOf('.'))}";
        //    }
        //    catch { }
        //}
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
                _imageName = $"Pictures/{Guid.NewGuid()}{_imagePath.Substring(_imagePath.LastIndexOf('.'), _imagePath.Length - _imagePath.LastIndexOf('.'))}";
            }
            catch { }
        }
    }

    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new MainWindow().Show();
        this.Close();
    }
}