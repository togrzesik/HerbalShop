using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class GraphicsCardViewModel
    {
        public string MemorySize { get; set; }
        public string FrequencyMemory { get; set; }
        public string KindOfMemory { get; set; }
        public string MemoryBus { get; set; }
        public string FrequencyCore { get; set; }
        public string Output { get; set; }
        public string PowerConsumption { get; set; }

    }
}