using Tasvir.SharedKernel;

namespace Tasvir.Modules.Content.Domain;

public sealed class FaqItem : Entity, IOrdered
{
    private FaqItem() { }

    public FaqItem(string question, string answer, int sortOrder)
    {
        Question = Guard.NotEmpty(question, nameof(question));
        Answer = Guard.NotEmpty(answer, nameof(answer));
        SortOrder = sortOrder;
    }

    public string Question { get; private set; } = null!;
    public string Answer { get; private set; } = null!;
    public int SortOrder { get; private set; }
}
