using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class MemoryRAMDetails
    {
        public string TypeOfMemory { get; set; }
        public string Capacity { get; set; }
        public string Frequency { get; set; }
        public string CycleLatency { get; set; }
        public string Voltage { get; set; }
    }
}