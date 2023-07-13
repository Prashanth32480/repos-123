namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public static class DatabaseStoreProcedures
    {
        public const string VersionGet = "usp_Version_Get";
        
        // Feeds
        public const string FeedInsert = "usp_Feed_Insert";
        public const string FeedSetProcessStatusToSuccess = "usp_Feed_SetSuccessProcessStatus";
        public const string FeedGet = "usp_Feed_GetByMessageId";
        public const string FeedEventGet = "usp_FeedEvent_Get";
        public const string FeedEventSave = "usp_FeedEvent_Save";
        public const string FeedEventDelete = "usp_FeedEvent_Delete";

        // External Feeds
        public const string ExternalFeedInsert = "usp_ExternalFeed_Insert";
        public const string ExternalFeedSetProcessStatusToSuccess = "usp_ExternalFeed_SetSuccessProcessStatus";
        public const string ExternalFeedGet = "usp_ExternalFeed_GetByMessageId";
        public const string ExternalFeedEventGet = "usp_ExternalFeedEvent_Get";
        public const string ExternalFeedEventSave = "usp_ExternalFeedEvent_Save";
        public const string ExternalFeedEventDelete = "usp_ExternalFeedEvent_Delete";

        //Participant
        public const string ParticipantSave = "usp_Participant_Save";
        public const string ParticipantGet = "usp_Participant_Get";
        public const string ParticipantDelete = "usp_Participant_Delete";
        public const string ParticipantMappingSave = "usp_ParticipantMapping_Save";
        public const string ParticipantMappingGet = "usp_ParticipantMapping_Get";
        public const string ParticipantMappingDelete = "usp_ParticipantMapping_Delete";
        public const string ParticipantMappingDeleteByPlayHQProfileId = "usp_ParticipantMappingByPlayHQProfileId_Delete";
        public const string ParticipantGetByMyCricketId = "usp_Participant_GetByMyCricketId";
        public const string UnlinkMyCricketId = "usp_Participant_UnlinkLegacyPlayerId";
        public const string ParticipantGetByParticipantId = "usp_Participant_GetByParticipantId";
        public const string ParticipantGetByParticipantIdForUnlink = "usp_Participant_GetByParticipantIdForUnlink";
    }
}