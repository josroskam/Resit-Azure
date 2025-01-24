using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessWeatherImageJob.Entities
{
    public class WeatherStation
    {
        public string JobId { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; }
        public string Region { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirectionDegrees { get; set; }
    }
}
