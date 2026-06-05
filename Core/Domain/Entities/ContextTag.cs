namespace MindLog.Api.Core.Domain.Entities
{
    public class ContextTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<EntryContext> EntryContexts { get; set; } = new List<EntryContext>();
    }
}