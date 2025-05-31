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
        VideoPannel.IsVisible = false;
    }
    string _mediaName = "";
    string _mediaPath = "";
    string extension = "";
    private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Âûבונטעו פאיכ" });
        if (files.Count() != 0)
        {
            try
            {
                VideoPannel.IsVisible = false;
                ShownImage.IsVisible = false;
                PlayButton.IsVisible = false;
                PlayVideo.IsVisible = false;
                _mediaPath = files[0].Path.LocalPath;
                extension = _mediaPath.Substring(_mediaPath.LastIndexOf('.'), _mediaPath.Length - _mediaPath.LastIndexOf('.'));
                if (extension == ".mp3")
                {
                    PlayButton.IsVisible = true;
                }
                else if (extension == ".mp4")
                {
                    VideoPannel.IsVisible = true;
                    PlayVideo.IsVisible = true;
                    App.AppNativeVideoPlayerService.Play($"{_mediaPath}");
                }
                else
                {
                    ShownImage.Source = new Bitmap(_mediaPath);
                    ShownImage.IsVisible = true;
                }

                _mediaName = $"{Guid.NewGuid()}{extension}";
            }
            catch { }
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
        if (!string.IsNullOrEmpty(_mediaPath))
        {
            if (string.IsNullOrEmpty(Name.Text))
            {
                Helper.CallMessageBox("Âגוהטעו טלÿ פאיכא" ,this);
                return;
            }
            try
            {
                AtachedMedium medium = new AtachedMedium() { TempPath = _mediaPath, Path = _mediaName, Description = Description.Text, Name = Name.Text };
                if (extension == ".mp3")
                    medium.TypeId = 2;
                else if (extension == ".mp4")
                    medium.TypeId = 3;
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

    private void VideoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.AppNativeVideoPlayerService.Play($"{_mediaPath}");
    }
}