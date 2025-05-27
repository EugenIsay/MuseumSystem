using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumSystem.Interfaces
{
    public interface INativeMediaPlayerService
    {
        Control CreateControl();
        void Play(string uri);
    }
}
