using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class MonitorDetailsViewModel
    {
        public string ScreenSize { get; set; }
        public string Resolution { get; set; }
        public string AspectRatio { get; set; }
        public string Layer { get; set; }
        public string PanelType { get; set; }
        public string PowerConsumption { get; set; }
        public string Output { get; set; }
    }
}