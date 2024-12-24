

namespace Common.Entities
{
    public class Success
    {
        private DateTimeOffset dateMatchedUTC;

        public Guid Id { get; set; }

        public Guid FileInfoId { get; set; }

        [NonKeyProperty]
        public Guid BatchId { get; set; }

        [NonKeyProperty]
        public string? UserName { get; set; }

        [NonKeyProperty]
        public DateTimeOffset DateMatched { get { return dateMatchedUTC; } set { dateMatchedUTC = value; } }

        public Success()
        {

        }
    }
}
