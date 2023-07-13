using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Grassroots.Identity.Functions.Cdc.Test.Cdc
{
    public class CdcIdentityOrchestratorFunctionTests
    {

        [Fact]
        public async void CdcIdentityOrchestratorFunction_LinkParentMyCricketid_ShouldCall_KondoActivityFunction()
        {
            var _telemetry = new Mock<ITelemetryHandler>();

            //Arrange
            var durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>(); //MOQ
            var cdcEvent = new CdcEvent()
            { 
                Type = "accountUpdated",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                Data = new CdcEventData()
                { 
                    AccountType = "full",
                    UId = "5df2e1cf6a92415394466354f3cb827c"
                },
                Id = "83238969-04ae-442e-a507-6816347d878d",
                Timestamp = 1653302340,
                Version = "2.0"
            };

            var followedTeamList = new List<CdcFollowedTeamModel>();
            CdcFollowedTeamModel followedTeam = new CdcFollowedTeamModel()
            {
                Id = "1",
                Name = "Abc"
            };
            followedTeamList.Add(followedTeam);

            var favTeamList = new List<CdcFavTeamModel>();
            CdcFavTeamModel favTeam = new CdcFavTeamModel()
            {
               Name = "Abc",
               IsSelected = true
            };
            favTeamList.Add(favTeam);


            var userDetails = new CdcGetAccountInfoResponse()
            {

                UID = "5df2e1cf6a92415394466354f3cb827c",
                CallId = "Test",
                Data = new CdcGetAccountInfoData()
                { 
                    //CrmId = null,
                    //FavTeam = favTeamList,
                    //FollowedTeam = followedTeamList,
                    Id = new CdcIdFields()
                    { 
                        //CrowdCrm = null,
                        MyCricket = "123",
                        Participant = "401e5b6e-1e1d-46f6-a40c-44497be8c929",
                        PlayHq = null
                    },
                    //MyCricketDashboardCards = null,
                    //MyCricketFollowedClubs = null,
                    //MyCricketFollowedPlayers = null,
                    MyCricketId = null,
                    PlayHqId = null

                },
                Profile = new CdcGetAccountInfoProfile()
                { 
                    //BirthYear = 0,
                    //Country = null,
                    Email = "",
                    FirstName = "Test F",
                    LastName = "Test L",
                    //MobileNumber = null,
                    //Zip = null

                },
                StatusCode = 200,
                StatusReason = "Test",
                ErrorCode = 0,
                ErrorDetails = null,
                ErrorMessage = null
                
            };

            var auditresult = new List<CdcAuditSearchResult>();
            var searchRes = new CdcAuditSearchResult()
            { 
                Apikey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallID = "842a3f1a453649bba3cf8fa05c2e7303",
                Endpoint = "",
                ErrCode = "0",
                ErrMessage = "",
                Params = new CdcAuditSearchParams()
                { 
                    ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                    Data = "\n{\n\t\"id\": {\n\t\t\"myCricket\" : \"123\"\n\t}\n  }",
                    Profile = "",
                    Uid = "5df2e1cf6a92415394466354f3cb827c"
                },
            };
            auditresult.Add(searchRes);

            var auditResponse = new CdcAuditSearchResponse()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ErrorCode = 0,
                ObjectsCount = 1,
                Results = auditresult,
                StatusCode = 200,
                StatusReason = "",
                //Time = DateTime.Now,
                TotalCount = 1                
            };

            CdcAuditSearchRequest auditRequest = new CdcAuditSearchRequest()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I"
            };

            durableOrchestrationContextMock.Setup(x => x.GetInput<CdcEvent>()).Returns(cdcEvent);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcGetAccountInfoResponse>("CdcGetAccountInfoActivityFunction", "5df2e1cf6a92415394466354f3cb827c")).ReturnsAsync(userDetails);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcAuditSearchResponse>("CdcGetAuditDetailsActivityFunction", auditRequest)).ReturnsAsync(auditResponse);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<int>("KondoFeedActivityFunction", cdcEvent)).ReturnsAsync(1);


            //Act
            var function = new CdcIdentityOrchestratorFunction(_telemetry.Object);
            var result = await function.Run(durableOrchestrationContextMock.Object);

            //Assert
            Assert.Equal(0, result);

        }

        [Fact]
        public async void CdcIdentityOrchestratorFunction_UnLinkParentMyCricketid_ShouldCall_KondoActivityFunction()
        {
            var _telemetry = new Mock<ITelemetryHandler>();

            //Arrange
            var durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>(); //MOQ
            var cdcEvent = new CdcEvent()
            {
                Type = "accountUpdated",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                Data = new CdcEventData()
                {
                    AccountType = "full",
                    UId = "5df2e1cf6a92415394466354f3cb827c"
                },
                Id = "83238969-04ae-442e-a507-6816347d878d",
                Timestamp = 1653302340,
                Version = "2.0"
            };

            var followedTeamList = new List<CdcFollowedTeamModel>();
            CdcFollowedTeamModel followedTeam = new CdcFollowedTeamModel()
            {
                Id = "1",
                Name = "Abc"
            };
            followedTeamList.Add(followedTeam);

            var favTeamList = new List<CdcFavTeamModel>();
            CdcFavTeamModel favTeam = new CdcFavTeamModel()
            {
                Name = "Abc",
                IsSelected = true
            };
            favTeamList.Add(favTeam);


            var userDetails = new CdcGetAccountInfoResponse()
            {

                UID = "5df2e1cf6a92415394466354f3cb827c",
                CallId = "Test",
                Data = new CdcGetAccountInfoData()
                {
                    //CrmId = null,
                    //FavTeam = favTeamList,
                    //FollowedTeam = followedTeamList,
                    Id = new CdcIdFields()
                    {
                        //CrowdCrm = null,
                        MyCricket = "",
                        Participant = "401e5b6e-1e1d-46f6-a40c-44497be8c929",
                        PlayHq = null
                    },
                    //MyCricketDashboardCards = null,
                    //MyCricketFollowedClubs = null,
                    //MyCricketFollowedPlayers = null,
                    MyCricketId = null,
                    PlayHqId = null

                },
                Profile = new CdcGetAccountInfoProfile()
                {
                    //BirthYear = 0,
                    //Country = null,
                    Email = "",
                    FirstName = "Test F",
                    LastName = "Test L",
                    //MobileNumber = null,
                    //Zip = null

                },
                StatusCode = 200,
                StatusReason = "Test",
                ErrorCode = 0,
                ErrorDetails = null,
                ErrorMessage = null

            };

            var auditresult = new List<CdcAuditSearchResult>();
            var searchRes = new CdcAuditSearchResult()
            {
                Apikey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallID = "842a3f1a453649bba3cf8fa05c2e7303",
                Endpoint = "",
                ErrCode = "0",
                ErrMessage = "",
                Params = new CdcAuditSearchParams()
                {
                    ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                    Data = "\n{\n\t\"id\": {\n\t\t\"myCricket\" : \"\"\n\t}\n  }",
                    Profile = "",
                    Uid = "5df2e1cf6a92415394466354f3cb827c"
                },
            };
            auditresult.Add(searchRes);

            var auditResponse = new CdcAuditSearchResponse()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ErrorCode = 0,
                ObjectsCount = 1,
                Results = auditresult,
                StatusCode = 200,
                StatusReason = "",
                Time = DateTime.Now,
                TotalCount = 1
            };

            CdcAuditSearchRequest auditRequest = new CdcAuditSearchRequest()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I"
            };

            durableOrchestrationContextMock.Setup(x => x.GetInput<CdcEvent>()).Returns(cdcEvent);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcGetAccountInfoResponse>("CdcGetAccountInfoActivityFunction", "5df2e1cf6a92415394466354f3cb827c")).ReturnsAsync(userDetails);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcAuditSearchResponse>("CdcGetAuditDetailsActivityFunction", auditRequest)).ReturnsAsync(auditResponse);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<int>("KondoFeedActivityFunction", cdcEvent)).ReturnsAsync(1);

            //Act
            var function = new CdcIdentityOrchestratorFunction(_telemetry.Object);
            var result = await function.Run(durableOrchestrationContextMock.Object);

            //Assert
            Assert.Equal(0, result);

        }
        [Fact]
        public async void CdcIdentityOrchestratorFunction_LinkChildMyCricketid_ShouldCall_KondoActivityFunction()
        {
            var _telemetry = new Mock<ITelemetryHandler>();

            //Arrange
            var durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>(); //MOQ
            var cdcEvent = new CdcEvent()
            {
                Type = "accountUpdated",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                Data = new CdcEventData()
                {
                    AccountType = "full",
                    UId = "5df2e1cf6a92415394466354f3cb827c"
                },
                Id = "83238969-04ae-442e-a507-6816347d878d",
                Timestamp = 1653302340,
                Version = "2.0"
            };

            var followedTeamList = new List<CdcFollowedTeamModel>();
            CdcFollowedTeamModel followedTeam = new CdcFollowedTeamModel()
            {
                Id = "1",
                Name = "Abc"
            };
            followedTeamList.Add(followedTeam);

            var favTeamList = new List<CdcFavTeamModel>();
            CdcFavTeamModel favTeam = new CdcFavTeamModel()
            {
                Name = "Abc",
                IsSelected = true
            };
            favTeamList.Add(favTeam);


            var userDetails = new CdcGetAccountInfoResponse()
            {

                UID = "5df2e1cf6a92415394466354f3cb827c",
                CallId = "Test",
                Data = new CdcGetAccountInfoData()
                {
                    //CrmId = null,
                    //FavTeam = favTeamList,
                    //FollowedTeam = followedTeamList,
                    Id = new CdcIdFields()
                    {
                        //CrowdCrm = null,
                        MyCricket = "Test",
                        Participant = null,
                        PlayHq = null
                    },
                    //MyCricketDashboardCards = null,
                    //MyCricketFollowedClubs = null,
                    //MyCricketFollowedPlayers = null,
                    MyCricketId = null,
                    PlayHqId = null

                },
                Profile = new CdcGetAccountInfoProfile()
                {
                    //BirthYear = 0,
                    //Country = null,
                    Email = "",
                    FirstName = "Test F",
                    LastName = "Test L",
                    //MobileNumber = null,
                    //Zip = null

                },
                StatusCode = 200,
                StatusReason = "Test",
                ErrorCode = 0,
                ErrorDetails = null,
                ErrorMessage = null

            };

            var auditresult = new List<CdcAuditSearchResult>();
            var searchRes = new CdcAuditSearchResult()
            {
                Apikey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallID = "842a3f1a453649bba3cf8fa05c2e7303",
                Endpoint = "",
                ErrCode = "0",
                ErrMessage = "",
                Params = new CdcAuditSearchParams()
                {
                    ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                    //Data = "\n{\n\t\"id\": {\n\t\t\"myCricket\" : \"VT 123\"\n\t}\n  }",
                    Data = "{\"child\":[{\"childId\":1,\"firstName\":\"Child\",\"lastName\":\"Kohli\",\"id\":{\"participant\":\"A0A72BDC-8F5D-495E-8979-4A9971CD7496\",\"myCricket\":\"123\"}}]}",
                    Profile = "",
                    Uid = "5df2e1cf6a92415394466354f3cb827c"
                },
            };
            auditresult.Add(searchRes);

            var auditResponse = new CdcAuditSearchResponse()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ErrorCode = 0,
                ObjectsCount = 1,
                Results = auditresult,
                StatusCode = 200,
                StatusReason = "",
                Time = DateTime.Now,
                TotalCount = 1
            };

            CdcAuditSearchRequest auditRequest = new CdcAuditSearchRequest()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I"
            };

            durableOrchestrationContextMock.Setup(x => x.GetInput<CdcEvent>()).Returns(cdcEvent);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcGetAccountInfoResponse>("CdcGetAccountInfoActivityFunction", "5df2e1cf6a92415394466354f3cb827c")).ReturnsAsync(userDetails);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcAuditSearchResponse>("CdcGetAuditDetailsActivityFunction", auditRequest)).ReturnsAsync(auditResponse);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<int>("KondoFeedActivityFunction", cdcEvent)).ReturnsAsync(1);


            //Act
            var function = new CdcIdentityOrchestratorFunction(_telemetry.Object);
            var result = await function.Run(durableOrchestrationContextMock.Object);

            //Assert
            Assert.Equal(0, result);

        }
        [Fact]
        public async void CdcIdentityOrchestratorFunction_UnlinkChildMyCricketid_ShouldCall_KondoActivityFunction()
        {
            var _telemetry = new Mock<ITelemetryHandler>();

            //Arrange
            var durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>(); //MOQ
            var cdcEvent = new CdcEvent()
            {
                Type = "accountUpdated",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                Data = new CdcEventData()
                {
                    AccountType = "full",
                    UId = "5df2e1cf6a92415394466354f3cb827c"
                },
                Id = "83238969-04ae-442e-a507-6816347d878d",
                Timestamp = 1653302340,
                Version = "2.0"
            };

            var followedTeamList = new List<CdcFollowedTeamModel>();
            CdcFollowedTeamModel followedTeam = new CdcFollowedTeamModel()
            {
                Id = "1",
                Name = "Abc"
            };
            followedTeamList.Add(followedTeam);

            var favTeamList = new List<CdcFavTeamModel>();
            CdcFavTeamModel favTeam = new CdcFavTeamModel()
            {
                Name = "Abc",
                IsSelected = true
            };
            favTeamList.Add(favTeam);


            var userDetails = new CdcGetAccountInfoResponse()
            {

                UID = "5df2e1cf6a92415394466354f3cb827c",
                CallId = "Test",
                Data = new CdcGetAccountInfoData()
                {
                    //CrmId = null,
                    //FavTeam = favTeamList,
                    //FollowedTeam = followedTeamList,
                    Id = new CdcIdFields()
                    {
                        //CrowdCrm = null,
                        MyCricket = "Test",
                        Participant = null,
                        PlayHq = null
                    },
                    //MyCricketDashboardCards = null,
                    //MyCricketFollowedClubs = null,
                    //MyCricketFollowedPlayers = null,
                    MyCricketId = null,
                    PlayHqId = null

                },
                Profile = new CdcGetAccountInfoProfile()
                {
                    //BirthYear = 0,
                    //Country = null,
                    Email = "",
                    FirstName = "Test F",
                    LastName = "Test L",
                    //MobileNumber = null,
                    //Zip = null

                },
                StatusCode = 200,
                StatusReason = "Test",
                ErrorCode = 0,
                ErrorDetails = null,
                ErrorMessage = null

            };

            var auditresult = new List<CdcAuditSearchResult>();
            var searchRes = new CdcAuditSearchResult()
            {
                Apikey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallID = "842a3f1a453649bba3cf8fa05c2e7303",
                Endpoint = "",
                ErrCode = "0",
                ErrMessage = "",
                Params = new CdcAuditSearchParams()
                {
                    ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                    //Data = "\n{\n\t\"id\": {\n\t\t\"myCricket\" : \"VT 123\"\n\t}\n  }",
                    Data = "{\"child\":[{\"childId\":1,\"firstName\":\"Child\",\"lastName\":\"Kohli\",\"id\":{\"participant\":\"A0A72BDC-8F5D-495E-8979-4A9971CD7496\",\"playHQ\":\"ADD1E48C-694A-4D15-B18D-2BBD18AFA60E\"}}]}",
                    Profile = "",
                    Uid = "5df2e1cf6a92415394466354f3cb827c"
                },
            };
            auditresult.Add(searchRes);

            var auditResponse = new CdcAuditSearchResponse()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ErrorCode = 0,
                ObjectsCount = 1,
                Results = auditresult,
                StatusCode = 200,
                StatusReason = "",
                Time = DateTime.Now,
                TotalCount = 1
            };

            CdcAuditSearchRequest auditRequest = new CdcAuditSearchRequest()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I"
            };

            durableOrchestrationContextMock.Setup(x => x.GetInput<CdcEvent>()).Returns(cdcEvent);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcGetAccountInfoResponse>("CdcGetAccountInfoActivityFunction", "5df2e1cf6a92415394466354f3cb827c")).ReturnsAsync(userDetails);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcAuditSearchResponse>("CdcGetAuditDetailsActivityFunction", auditRequest)).ReturnsAsync(auditResponse);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<int>("KondoFeedActivityFunction", cdcEvent)).ReturnsAsync(1);


            //Act
            var function = new CdcIdentityOrchestratorFunction(_telemetry.Object);
            var result = await function.Run(durableOrchestrationContextMock.Object);

            //Assert
            Assert.Equal(0, result);

        }
        [Fact]
        public async void CdcIdentityOrchestratorFunction_UpdatePlayHQid_ShouldNotCall_KondoActivityFunction()
        {
            var _telemetry = new Mock<ITelemetryHandler>();

            //Arrange
            var durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>(); //MOQ
            var cdcEvent = new CdcEvent()
            {
                Type = "accountUpdated",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                Data = new CdcEventData()
                {
                    AccountType = "full",
                    UId = "5df2e1cf6a92415394466354f3cb827c"
                },
                Id = "83238969-04ae-442e-a507-6816347d878d",
                Timestamp = 1653302340,
                Version = "2.0"
            };

            var userDetails = new CdcGetAccountInfoResponse()
            {

                UID = "5df2e1cf6a92415394466354f3cb827c",
                CallId = "Test",
                Data = new CdcGetAccountInfoData()
                {
                    //CrmId = null,
                    //FavTeam = null,
                    //FollowedTeam = null,
                    Id = new CdcIdFields()
                    {
                        //CrowdCrm = null,
                        MyCricket = null,
                        Participant = null,
                        PlayHq = "Test"
                    },
                    //MyCricketDashboardCards = null,
                    //MyCricketFollowedClubs = null,
                    //MyCricketFollowedPlayers = null,
                    MyCricketId = null,
                    PlayHqId = null

                },
                Profile = new CdcGetAccountInfoProfile()
                {
                    //BirthYear = 0,
                    //Country = null,
                    Email = "",
                    FirstName = "Test F",
                    LastName = "Test L",
                    //MobileNumber = null,
                    //Zip = null

                },
                StatusCode = 200,
                StatusReason = "Test",
                ErrorCode = 0,
                ErrorDetails = null,
                ErrorMessage = null

            };

            var auditresult = new List<CdcAuditSearchResult>();
            var searchRes = new CdcAuditSearchResult()
            {
                Apikey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                CallID = "842a3f1a453649bba3cf8fa05c2e7303",
                Endpoint = "",
                ErrCode = "0",
                ErrMessage = "",
                Params = new CdcAuditSearchParams()
                {
                    ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I",
                    Data = "\n{\n\t\"id\": {\n\t\t\"playHQ\" : \"VT 123\"\n\t}\n  }",
                    Profile = "",
                    Uid = "5df2e1cf6a92415394466354f3cb827c"
                },
            };
            auditresult.Add(searchRes);

            var auditResponse = new CdcAuditSearchResponse()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ErrorCode = 0,
                ObjectsCount = 1,
                Results = auditresult,
                StatusCode = 200,
                StatusReason = "",
                Time = DateTime.Now,
                TotalCount = 1
            };

            CdcAuditSearchRequest auditRequest = new CdcAuditSearchRequest()
            {
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                ApiKey = "3_KpMYzpHCEDK5pfaGckE4CitUMVlVYUA8kGeBetcNHmo9TUBj3ajj-Z1DFsaJ_Y8I"
            };

            durableOrchestrationContextMock.Setup(x => x.GetInput<CdcEvent>()).Returns(cdcEvent);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcGetAccountInfoResponse>("CdcGetAccountInfoActivityFunction", "5df2e1cf6a92415394466354f3cb827c")).ReturnsAsync(userDetails);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<CdcAuditSearchResponse>("CdcGetAuditDetailsActivityFunction", auditRequest)).ReturnsAsync(auditResponse);
            durableOrchestrationContextMock.Setup(x => x.CallActivityAsync<int>("KondoFeedActivityFunction", cdcEvent)).ReturnsAsync(1);

            //Act
            var function = new CdcIdentityOrchestratorFunction(_telemetry.Object);
            var result = await function.Run(durableOrchestrationContextMock.Object);

            //Assert
            Assert.Equal(0, result);

        }

    }
}