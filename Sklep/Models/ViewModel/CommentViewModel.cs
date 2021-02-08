using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class CommentViewModel
    {
        public int CommentID { get; set; }
        public int ProductID { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}