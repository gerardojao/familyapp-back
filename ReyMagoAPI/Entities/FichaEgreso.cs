

using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyApp.Entities;

public partial class FichaEgreso
{
    public int Id { get; set; }

    public string Foto { get; set; } = null!;

    [NotMapped]
    public IFormFile File { get; set; }

    public DateTime? Fecha { get; set; }

    public string Mes { get; set; } = null!;

    public int NombreEgreso { get; set; }

    public decimal Importe { get; set; }
}
