using System;
using System.Collections.Generic;
using System.Text;

namespace Grassroots.Identity.Functions.External.Common.Model
{
    public class CdcGetAccountInfoResponse
    {
        public string CallId { get; set; }
        public int StatusCode { get; set; }
        public string StatusReason { get; set; }
        public string Uid { get; set; }
        public CdcGetAccountInfoData Data { get; set; }
        public CdcGetAccountInfoProfile Profile { get; set; }
        public CdcGetAccountInfoPreferences Preferences { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public string RegSource { get; set; }
        public dynamic Subscriptions { get; set; }

    }
    public class CdcGetAccountInfoProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Zip { get; set; }
        public int? BirthYear { get; set; }
        public string Gender { get; set; }
        public string State { get; set; }
        public CdcGetAccountInfoPhones Phones { get; set; }
    }

    public class CdcGetAccountInfoPhones
    {
        public string Number { get; set; }
    }

    public class CdcGetAccountInfoData
    {
        public List<CdcFavTeamModel> FavTeam { get; set; }
        public bool? SyncInsiderPanels { get; set; }
    }

    public class CdcFavTeamModel
    {
        public bool IsSelected { get; set; }
        public string Name { get; set; }
    }

    public class CdcGetAccountInfoPreferences
    {
        public CdcGetAccountInfoPreferencesPanel Panel { get; set; }
        public CdcGetAccountInfoPreferencesPanel Subscribe { get; set; }
    }

    public class CdcGetAccountInfoPreferencesPanel
    {
        public CdcGetAccountInfoPreferencesPanelDetails CricketAu { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails PlayCricketAu { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails ShopCricketAu { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails AdelaideStrikers { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails BrisbaneHeat { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails HobartHurricanes { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails MelbourneRenegades { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails MelbourneStars { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails PerthScorchers { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails SydneySixers { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails SydneyThunder { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails CricketAct { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails CricketNsw { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails QldCricket { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails SacaAu { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails CricketTas { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails CricketVic { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails WaCricket { get; set; }
        public CdcGetAccountInfoPreferencesPanelDetails NtCricket { get; set; }
    }
    public class CdcGetAccountInfoPreferencesPanelDetails
    {
        public bool IsConsentGranted { get; set; }
    }
}
