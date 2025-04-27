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

namespace MuseumSystem;

public partial class ExhibitWindow : Window
{
    private LibVLC MainLibVLC { get; set; }
    private MediaPlayer MainMediaPlayer { get; set; }
    public ExhibitWindow()
    {
        InitializeComponent();
        InitMediaPlayer();
    }
    public ExhibitWindow(int Id)
    {
        InitializeComponent();
        InitMediaPlayer();
    }
    private void InitMediaPlayer()
    {
        MainLibVLC = new(enableDebugLogs: true);

        MainMediaPlayer = new(MainLibVLC);
        MainMediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
    }

    public void ClickHandler(object sender, RoutedEventArgs args)
    {
        Media media = new(MainLibVLC, new Uri(Environment.CurrentDirectory + "/Audio/sample-3s.mp3"));
        MainMediaPlayer.Media = media;
        MainMediaPlayer.Play();
    }

    private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(
            new Action(
                () =>
                {
                    PlaybackStatus.Text = $"{MainMediaPlayer.Time / 1000.0} / {MainMediaPlayer.Length / 1000.0}";
                }
            )
        );
    }
}