using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Models
{
    public class PdfModel
    {
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Klasse { get; set; }
        public List<bool> Firmen { get; set; }
    }
}
