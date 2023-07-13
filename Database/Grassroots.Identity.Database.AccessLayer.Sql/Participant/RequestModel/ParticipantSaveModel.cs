using DB = Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel
{
    public class ParticipantSaveModel : DB.Participant
    {
        public long FeedId { get; set; }
    }
}
