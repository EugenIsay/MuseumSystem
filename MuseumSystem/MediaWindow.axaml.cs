using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using LibVLCSharp.Shared;
using MuseumSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MuseumSystem;

public partial class MediaWindow : Window
{
    private LibVLC MainLibVLC { get; set; }
    private MediaPlayer MainMediaPlayer { get; set; }
    public MediaWindow()
    {
        InitializeComponent();
        InitMediaPlayer();
    }
    string _mediaName = "";
    string _mediaPath = "";
    string extension = "";
    private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Âûבבונטעו פמעמדנאפט‏" });
        if (files.Count() != 0)
        {
            try
            {
                ShownImage.IsVisible = false;
                PlayButton.IsVisible = false;
                _mediaPath = files[0].Path.LocalPath;
                extension = _mediaPath.Substring(_mediaPath.LastIndexOf('.'), _mediaPath.Length - _mediaPath.LastIndexOf('.'));
                if (extension == ".mp3")
                {

                    PlayButton.IsVisible = true;
                    _mediaName = $"{Guid.NewGuid()}{extension}";

                }
                else
                {
                    ShownImage.Source = new Bitmap(_mediaPath);
                    ShownImage.IsVisible = true;
                    _mediaName = $"{Guid.NewGuid()}{extension}";
                }
            }
            catch { }
        }
    }

    private void InitMediaPlayer()
    {
        MainLibVLC = new(enableDebugLogs: true);
        MainMediaPlayer = new(MainLibVLC);
    }
    public void PlayButton_Click(object sender, RoutedEventArgs args)
    {
        try
        {
            Media media = new(MainLibVLC, new Uri(_mediaPath));
            MainMediaPlayer.Media = media;
            MainMediaPlayer.Play();
        }
        catch
        {

        }

    }

    private void ReadyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if ( !string.IsNullOrEmpty(_mediaPath))
        {
            try
            {
                AtachedMedium medium = new AtachedMedium() { TempPath = _mediaPath, Path = _mediaName, Description = Description.Text };
                if (extension == ".mp3")
                    medium.TypeId = 2;
                else
                    medium.TypeId = 1;
                Close(medium);
            }
            catch
            {

            }
        }
        else
        {
            Close();
        }

    }
}