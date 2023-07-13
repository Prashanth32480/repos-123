using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Common;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;
using System;
using System.Threading.Tasks;
using DB = Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping
{
    public class ParticipantMappingOperations : IParticipantMappingOperations
    {
        private readonly IDatabaseConnectionFactory _factory;
        public ParticipantMappingOperations(IDatabaseConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Guid> SaveParticipantMapping(ParticipantMappingSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();

            var participant = await connection.ExecuteResultAsync<DB.ParticipantMapping, ParticipantMappingSaveModel>(DatabaseStoreProcedures.ParticipantMappingSave, request);

            if (participant == null)
                throw new ApplicationException(ApplicationStrings.ErrorInSavePlayHQParticipantMapping);

            return participant.ParticipantGuid;
        }

        public async Task<DB.ParticipantMapping> GetParticipantMappingByParticipanyGuid(string participantguid)
        {
            if (new Guid(participantguid) == Guid.Empty)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(participantguid));

            using var connection = _factory.CreateConnection();

            var request = new ParticipantMappingRequestModel
            {
                ParticipantGuid = new Guid(participantguid)
            };

            return await connection.ExecuteResultAsync<DB.ParticipantMapping, ParticipantMappingRequestModel>(DatabaseStoreProcedures.ParticipantMappingGet, request);
        }

        public async Task DeleteParticipantMapping(ParticipantMappingSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            await connection.ExecuteNonQueryAsync(DatabaseStoreProcedures.ParticipantMappingDelete,
                new { request.ParticipantGuid, request.FeedId });
        }

        public async Task DeleteParticipantMappingByPlayHQProfileId(ParticipantMappingSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            await connection.ExecuteNonQueryAsync(DatabaseStoreProcedures.ParticipantMappingDeleteByPlayHQProfileId,
                new { request.PlayHQProfileId, request.ParticipantGuid, request.FeedId });
        }

        public async Task UnlinkMyCricketId(ParticipantMappingSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            await connection.ExecuteNonQueryAsync(DatabaseStoreProcedures.UnlinkMyCricketId,
                new { request.ParticipantGuid, request.FeedId, request.LegacyPlayerId });
        }
    }
}
