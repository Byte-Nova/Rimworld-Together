using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronous.Objects
{
    public class PlayerWeather
    {
        public int MapTile { get; set; } = -1;

        public byte WeatherByte { get; set; } = byte.MaxValue;
    }
}
