using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcGetAccountInfoResponse
    {
        public string CallId { get; set; }
        public int StatusCode { get; set; }
        public string StatusReason { get; set; }
        public string UID { get; set; }
        public CdcGetAccountInfoData Data { get; set; }
        public CdcGetAccountInfoProfile Profile { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Created { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastUpdated { get; set; }

    }
    public class CdcGetAccountInfoProfile
    {
        [JsonProperty( NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Zip { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int? BirthYear { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string MobileNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }
    }

    public class CdcGetAccountInfoData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CdcFavTeamModel> FavTeam { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public List<CdcFollowedTeamModel> FollowedTeam { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string MyCricketFollowedClubs { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string MyCricketFollowedPlayers { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SyncInsiderPanels { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CdcIdFields Id { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string CrmId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MyCricketId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PlayHqId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CdcChild> Child { get; set; }

    }
}
