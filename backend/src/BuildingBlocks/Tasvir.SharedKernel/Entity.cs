namespace Tasvir.SharedKernel;

/// <summary>Barcha modullardagi domen obyektlari uchun asosiy sinf.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

/// <summary>Ro‘yxatlarda tartib bilan ko‘rsatiladigan obyektlar.</summary>
public interface IOrdered
{
    int SortOrder { get; }
}
