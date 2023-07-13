using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Common;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DB = Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Database.AccessLayer.Sql.Participant
{
    public class ParticipantOperations : IParticipantOperations
    {
        private readonly IDatabaseConnectionFactory _factory;
        public ParticipantOperations(IDatabaseConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Guid> SaveParticipant(ParticipantSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();

            var participant = await connection.ExecuteResultAsync<DB.Participant, ParticipantSaveModel>(DatabaseStoreProcedures.ParticipantSave, request);

            if (participant == null)
                throw new ApplicationException(ApplicationStrings.ErrorInSavePlayHQParticipant);

            return participant.ParticipantGuid;
        }

        public async Task DeleteParticipant(ParticipantSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            await connection.ExecuteNonQueryAsync(DatabaseStoreProcedures.ParticipantDelete,
                new { request.ParticipantGuid, request.FeedId });
        }

        public async Task<DB.Participant> GetParticipantByPlayHQProfileId(string playHQProfileId)
        {
            if (new Guid(playHQProfileId) == Guid.Empty)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(playHQProfileId));

            using var connection = _factory.CreateConnection();

            var request = new ParticipantRequestModel
            {
                PlayHQProfileId = new Guid(playHQProfileId)
            };

            return await connection.ExecuteResultAsync<DB.Participant, ParticipantRequestModel>(DatabaseStoreProcedures.ParticipantGet, request);
        }

        public async Task<DB.ParticipantDetails> GetParticipantByMyCricketId(int legacyPlayerId)
        {
            if (legacyPlayerId == 0)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(legacyPlayerId));

            using var connection = _factory.CreateConnection();

            var request = new GetParticipantByMyCricketIdRequestModel
            {
                MyCricketId = legacyPlayerId
            };

            return await connection.ExecuteResultAsync<DB.ParticipantDetails, GetParticipantByMyCricketIdRequestModel>(DatabaseStoreProcedures.ParticipantGetByMyCricketId, request);
        }

        public async Task<IEnumerable<DB.ParticipantDetails>> GetParticipantByParticipantId(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId))
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(participantId));

            using var connection = _factory.CreateConnection();

            var request = new GetParticipantByParticipantIdRequestModel()
            {
                ParticipantGuid = new Guid(participantId)
            };

            return await connection.ExecuteResultCollectionAsync<DB.ParticipantDetails, GetParticipantByParticipantIdRequestModel>(DatabaseStoreProcedures.ParticipantGetByParticipantId, request);
        }

        public async Task<DB.ParticipantDetails> GetParticipantByParticipantIdForUnlink(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId))
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(participantId));

            using var connection = _factory.CreateConnection();

            var request = new GetParticipantByParticipantIdRequestModel
            {
                ParticipantGuid = new Guid(participantId)
            };

            return await connection.ExecuteResultAsync<DB.ParticipantDetails, GetParticipantByParticipantIdRequestModel>(DatabaseStoreProcedures.ParticipantGetByParticipantIdForUnlink, request);
        }
    }
}
