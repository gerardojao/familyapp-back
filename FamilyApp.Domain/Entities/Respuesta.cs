namespace FamilyApp.Domain.Entities;

public class Respuesta<T>
{
    public int Ok { get; set; }

    public List<T> Data { get; set; } = new();
    public string? Message { get; set; }
}

public class Enlace
{
    public string Url { get; set; } = string.Empty;
}
