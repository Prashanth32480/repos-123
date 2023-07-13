using System.Collections.Generic;
using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.External.Insider.Models
{
    public class InsiderRequestCustomAttributes
    {
        [JsonProperty(PropertyName = "state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty(PropertyName = "postcode", NullValueHandling = NullValueHandling.Ignore)]
        public string Postcode { get; set; }

        [JsonProperty(PropertyName = "favourite_team", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FavouriteTeam { get; set; }

        [JsonProperty(PropertyName = "registration_source", NullValueHandling = NullValueHandling.Ignore)]
        public string RegistrationSource { get; set; }

        [JsonProperty(PropertyName = "cricketid_account_type", NullValueHandling = NullValueHandling.Ignore)]
        public string CricketidAccountType { get; set; }

        [JsonProperty(PropertyName = "subscribe_strikers_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionStrikersGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_strikers_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionStrikersMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_strikers_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionStrikersTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_heat_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionHeatGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_heat_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionHeatMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_heat_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionHeatTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_hurricanes_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionHurricanesGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_hurricanes_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionHurricanesMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_hurricanes_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionHurricanesTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_renegades_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionRenegadesGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_renegades_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionRenegadesMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_renegades_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionRenegadesTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_stars_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionStarsGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_stars_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionStarsMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_stars_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionStarsTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_scorchers_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionScorchersGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_scorchers_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionScorchersMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_scorchers_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionScorchersTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_thunder_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionThunderGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_thunder_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionThunderMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_thunder_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionThunderTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_sixers_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionSixersGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_sixers_membership", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionSixersMembership { get; set; }

        [JsonProperty(PropertyName = "subscribe_sixers_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionSixersTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaGeneraMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_shop", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaShop { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_tickets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaTickets { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_travel_office", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaTravelOffice { get; set; }

        [JsonProperty(PropertyName = "subscribe_act_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionActGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_nsw_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionNswGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_nt_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionNtGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_qld_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionQldGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_sa_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionSaGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_tas_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionTasGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_vic_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionVicGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_wa_general_marketing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionWaGeneralMarketing { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_coaching_newsletter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaCoachingNewsletter { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_umpiring_newsletter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaUmpiringNewsletter { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_schools_newsletter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaSchoolsNewsletter { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_admins_newsletter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaAdminsNewsletter { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_blast_newsletter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaBlastNewsletter { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_the_wicket", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaTheWicket { get; set; }

        [JsonProperty(PropertyName = "subscribe_act_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionActParticipation { get; set; }

        [JsonProperty(PropertyName = "subscribe_ca_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionCaParticipant { get; set; }

        [JsonProperty(PropertyName = "subscribe_nsw_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionNswParticipation { get; set; }

        [JsonProperty(PropertyName = "subscribe_nt_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionNtParticipation { get; set; }

        [JsonProperty(PropertyName = "subscribe_qld_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionQldParticipation { get; set; }

        [JsonProperty(PropertyName = "subscribe_sa_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionSaParticipation { get; set; }

        [JsonProperty(PropertyName = "subscribe_tas_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionTasParticipant { get; set; }

        [JsonProperty(PropertyName = "subscribe_vic_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionVicParticipant { get; set; }

        [JsonProperty(PropertyName = "subscribe_wa_participation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubscriptionWaParticipant { get; set; }
    }
}
