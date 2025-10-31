namespace Quasar.Samples.BasicApi;

public sealed class CounterReadModel
{
    public Guid Id { get; set; }
    public int Count { get; set; }
}

public sealed class CartReadModel
{
    public Guid Id { get; set; }
    public int TotalItems { get; set; }
}

public sealed class CartProductLine
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
