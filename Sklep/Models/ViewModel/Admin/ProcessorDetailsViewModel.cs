using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class ProcessorDetailsViewModel
    {
        public string ProcessorModel { get; set; }
        public string Socket { get; set; }
        public string Frequency { get; set; }
        public int AmountCores { get; set; }
        public int AmountThreads { get; set; }
        public string ProcessorGraphics { get; set; }
        public string Cached { get; set; }
    }
}