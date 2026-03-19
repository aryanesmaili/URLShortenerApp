namespace URLShortener.Domain.Entities.ServiceTariffs
{
    public sealed class ServiceTariffModel
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public long Price { get; set; }
        public string? Description { get; set; }
    }
}
