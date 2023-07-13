using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DB = Grassroots.Identity.Database.Model.DbEntity;


namespace Grassroots.Identity.Database.AccessLayer.Sql.Participant
{
    public interface IParticipantOperations
    {
        /// <summary>
        /// Save the record to PlayHq participant table in the database and returns the PlayHq ParticipantGuid.
        /// </summary>
        /// <param name="participant">PlayHQ Participant object to save.</param>
        /// <returns></returns>
        Task<Guid> SaveParticipant(ParticipantSaveModel participant);

        /// <summary>
        /// Get the participant details based on playhq profile id
        /// </summary>
        /// <param name="playHQProfileId">PlayHQ participant profile id</param>
        /// <returns></returns>
        Task<DB.Participant> GetParticipantByPlayHQProfileId(string playHQProfileId);

        /// <summary>
        /// Soft delete the participant.
        /// </summary>
        /// <param name="participant">PlayHQ Participant object.</param>
        /// <returns></returns>
        Task DeleteParticipant(ParticipantSaveModel participant);

        /// <summary>
        /// Get participant by mycricket id
        /// </summary>
        /// <param name="legacyPlayerId">MyCricketId.</param>
        /// <returns></returns>

        Task<DB.ParticipantDetails> GetParticipantByMyCricketId(int legacyPlayerId);

        /// <summary>
        /// Get participant by participantId id
        /// </summary>
        /// <param name="participantId">participantId.</param>
        /// <returns></returns>

        Task<IEnumerable<DB.ParticipantDetails>> GetParticipantByParticipantId(string participantId);

        /// <summary>
        /// Get participant by participantId id for account unlink
        /// </summary>
        /// <param name="participantId">participantId.</param>
        /// <returns></returns>

        Task<DB.ParticipantDetails> GetParticipantByParticipantIdForUnlink(string participantId);
    }
}
