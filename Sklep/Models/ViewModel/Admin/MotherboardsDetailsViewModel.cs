using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class MotherboardsDetailsViewModel
    {
        public string Standard { get; set; }
        public string Chipset { get; set; }
        public string Socket { get; set; }
        public int MaksimumNumOfProcessors { get; set; }
        public string KindOfMemoryRAM { get; set; }
        public int NumOfSlotsMemoryRAM { get; set; }
        public string MaksimumCapaticyMemoryRAM { get; set; }
        public string IntegratedGraphicsCard { get; set; }
        public string SoundChipset { get; set; }
        public string IntegratedNetworkCard { get; set; }
        public string Output { get; set; }
    }
}