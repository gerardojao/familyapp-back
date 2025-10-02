using System;
using System.Collections.Generic;

namespace FamilyApp.Domain.Entities;

public partial class Ingreso
{
    public int Id { get; set; }

    public string NombreIngreso { get; set; } = null!;
}
