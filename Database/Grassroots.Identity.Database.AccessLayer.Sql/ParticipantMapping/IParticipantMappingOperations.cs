using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;
using System;
using System.Threading.Tasks;
using DB = Grassroots.Identity.Database.Model.DbEntity;


namespace Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping
{
    public interface IParticipantMappingOperations
    {
        /// <summary>
        /// Save the record to participant mapping table in the database and returns the ParticipantGuid.
        /// </summary>
        /// <param name="participant">PlayHQ Participant Mapping object to save.</param>
        /// <returns></returns>
        Task<Guid> SaveParticipantMapping(ParticipantMappingSaveModel participant);

        /// <summary>
        /// Get the participant mapping details based on participant guid
        /// </summary>
        /// <param name="participantguid">participant guid</param>
        /// <returns></returns>
        Task<DB.ParticipantMapping> GetParticipantMappingByParticipanyGuid(string participantguid);

        ///// <summary>
        ///// Soft delete the participantmapping.
        ///// </summary>
        ///// <param name="request">Participant Mapping Model.</param>
        ///// <returns></returns>
        Task DeleteParticipantMapping(ParticipantMappingSaveModel request);

        ///// <summary>
        ///// Soft delete the participantmapping based on PlayHQProfileId.
        ///// </summary>
        ///// <param name="request">Participant Mapping Model.</param>
        ///// <returns></returns>
        Task DeleteParticipantMappingByPlayHQProfileId(ParticipantMappingSaveModel request);

        ///// <summary>
        ///// Unlink Mycricket Id from participantmapping
        ///// </summary>
        ///// <param name="request">Participant Mapping Model.</param>
        ///// <returns></returns>
        Task UnlinkMyCricketId(ParticipantMappingSaveModel request);
    }
}
