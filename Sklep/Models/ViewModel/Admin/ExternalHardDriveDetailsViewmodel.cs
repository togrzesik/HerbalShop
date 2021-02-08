using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class ExternalHardDriveDetailsViewmodel
    {
        public string TypeOfDrive { get; set; }
        public string Capacity { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Interface { get; set; }
        public string Waterproof { get; set; }
    }
}