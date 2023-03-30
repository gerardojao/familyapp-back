using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyApp.Models;

public partial class FichaIngreso
{
    public int Id { get; set; }

    public string Foto { get; set; } = null!;

    //[NotMapped]
    //public IFormFile File { get; set; }


    public DateTime? Fecha { get; set; }

    public string Mes { get; set; } = null!;

    public int NombreIngreso { get; set; }

    public decimal Importe { get; set; }
}
