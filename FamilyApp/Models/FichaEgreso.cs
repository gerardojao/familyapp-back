using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FamilyApp.Models;

public partial class FichaEgreso
{
    public int Id { get; set; }

    public string Foto { get; set; } = null!;

    //[NotMapped]
    //public IFormFile File { get; set; }


    public DateTime? Fecha { get; set; }

    public string Mes { get; set; } = null!;

    public int NombreEgreso { get; set; }

    public decimal Importe { get; set; }

    //[JsonIgnore]
    //public virtual Egreso NombreEgresoNavigation { get; set; } = null!;
}
