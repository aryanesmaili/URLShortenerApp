namespace URLShortener.Domain.Entities.ServiceTariffs
{
    public sealed class ServiceTariffModel
    {
        public long ID { get; set; }
        public required string Name { get; set; }
        public long Price { get; set; }
        public string? Description { get; set; }
    }
}
