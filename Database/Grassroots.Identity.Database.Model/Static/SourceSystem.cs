
namespace Grassroots.Identity.Database.Model.Static
{
    // Different from External Source. This is used in FeedEvent table
    public enum SourceSystem : byte
    {
        Unknown = 0,
        Kondo = 1,
        PlayHQ = 2,
        CRM = 3,
        CDC = 4
    }
}