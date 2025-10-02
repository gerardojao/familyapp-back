// Common/Auditable.cs
public abstract class Auditable
{
    public bool Activo { get; set; } = true;

    public string? UsuarioCreacion { get; set; }
    public DateTime FechaCreacion { get; set; }

    public string? UsuarioModificacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}

// Common/IHasKey.cs
public interface IHasKey { int Id { get; set; } }

// Common/ISoftDeletable.cs
public interface ISoftDeletable
{
    bool Eliminado { get; set; }
    DateTime? FechaEliminacion { get; set; }
}
