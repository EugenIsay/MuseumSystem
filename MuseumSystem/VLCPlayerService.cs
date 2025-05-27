using Avalonia.Controls;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using MuseumSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumSystem
{
    public class VLCPlayerService : INativeMediaPlayerService
    {
        private LibVLC MainLibVLC { get; set; }
        private MediaPlayer MainMediaPlayer { get; set; }

        public Control CreateControl()
        {
            // Create player
            MainLibVLC = new LibVLC(enableDebugLogs: true);

            // Create player view
            MainMediaPlayer = new(MainLibVLC);

            // Create player control
            VideoView videoView = new()
            {
                MediaPlayer = MainMediaPlayer
            };

            return videoView;
        }

        public void Play(string uri)
        {
            // Create media
            var media = new Media(MainLibVLC, new Uri(uri));

            // Play media
            MainMediaPlayer.Media = media;
            MainMediaPlayer.Play();
        }

    }
}
