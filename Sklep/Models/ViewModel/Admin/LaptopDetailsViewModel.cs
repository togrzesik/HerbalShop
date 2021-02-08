using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class LaptopDetailsViewModel
    {
        public string Processor { get; set; }
        public string GraphicCard { get; set; }
        public string RAMSize {get;set;}
        public string HDDSize { get; set; }
        public string SSDSize { get; set; }
        public string OpticalDrive { get; set; }
 
        public string DisplayResolution { get; set; }
        public string DisplaySize { get; set; }
        public string Color { get; set; }
        public string Battery { get; set; }
        public string Weight { get; set; }
        public string OperatingSystem { get; set; }
        public string OtherSpec { get; set; }
    }
}