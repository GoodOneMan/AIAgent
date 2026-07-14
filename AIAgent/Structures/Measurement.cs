using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.Structures
{
    public class Measurement
    {
        public static double GetValue(string type)
        {
            switch (type)
            {
                case "Meters":
                    return Meters;
                case "Centimeters":
                    return Centimeters;
                case "Millimeters":
                    return Millimeters;
                case "Feet":
                    return Feet;
                case "Inches":
                    return Inches;
                case "Yards":
                    return Yards;
                case "Kilometers":
                    return Kilometers;
                case "Miles":
                    return Miles;
                case "Micrometers":
                    return Micrometers;
                case "Mils":
                    return Mils;
                case "Microinches":
                    return Microinches;
                default:
                    return 1;
            }
        }

        private static double Meters = 1000;
        private static double Centimeters = 10;
        private static double Millimeters = 1;
        private static double Feet = 304.8;
        private static double Inches = 25.4;
        private static double Yards = 914.4;
        private static double Kilometers = 1000000;
        private static double Miles = 1609344;
        private static double Micrometers = 0.001;
        private static double Mils = 0.0254;
        private static double Microinches = 2.54;
    }
}
