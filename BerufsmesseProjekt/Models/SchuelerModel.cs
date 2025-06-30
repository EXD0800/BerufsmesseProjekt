using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerufsmesseProjekt.Models;

public class SchuelerModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Nachname { get; set; }
    public int id_klasse { get; set; }
}
