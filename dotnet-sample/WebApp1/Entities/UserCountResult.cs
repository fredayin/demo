

namespace Common.Entities
{
    public class UserCountResult
    {
        public DateTime Date => DateTime.UtcNow;
        public int Count { get; set; }
        public string? Description { get; set; }
        public Guid TenantId { get; set; }
        public string? Filter { get; set; }
        public Dictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>
            {
                [nameof(Date)] = Date,
                [nameof(Count)] = Count,
                [nameof(Description)] = Description ?? string.Empty,
                [nameof(TenantId)] = TenantId,
                [nameof(Filter)] = Filter ?? string.Empty,
            };

        }
    }
}
