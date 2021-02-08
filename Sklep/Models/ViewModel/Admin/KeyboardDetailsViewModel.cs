using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class KeyboardDetailsViewModel
    {
        public string TypeOfKeyboard { get; set; }
        public string Backlight { get; set; }
        public string Touchpad { get; set; }
        public string Color { get; set; }
        public string Interface { get; set; }
        public string LengthCable { get; set; }
    }
}