using System.Collections.Generic;
using System.Threading.Tasks;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public interface IFeedOperations
    {
        Task<long> SaveRawFeed(RawFeedSaveRequestModel feed);
        Task<IEnumerable<RawFeed>> GetRawFeedByMessageID(string messageId);
        /// <summary>
        /// Will set ProcessStatus column value to true in raw feeds table for the given Raw FeedId.
        /// </summary>
        /// <param name="rawFeedId">The rawFeedId of the feed.</param>
        /// <returns></returns>
        Task<long> SetProcessStatusToSuccess(long rawFeedId);

        Task<long> SaveExternalRawFeed(RawFeedSaveRequestModel feed);
        Task<IEnumerable<RawFeed>> GetExternalRawFeedByMessageId(string messageId);
        Task<long> SetExternalFeedProcessStatusToSuccess(long rawFeedId);
    }
}