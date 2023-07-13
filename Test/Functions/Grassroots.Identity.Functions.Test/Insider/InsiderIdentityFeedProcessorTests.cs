using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Moq;
using Moq.Protected;
using Xunit;
using System.Collections;
using System.Threading.Tasks;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Microsoft.Azure.EventGrid.Models;
using Grassroots.Identity.Functions.External;
using Grassroots.Identity.Functions.PlayHQ.Registration;
using Grassroots.Identity.Functions.External.Insider;
using Grassroots.Identity.Functions.External.Common.Model;
using System.Collections.Generic;
using Grassroots.Identity.Functions.Test.Common.Helper;

namespace Grassroots.Identity.Functions.Test.Insider
{
    public class InsiderIdentityFeedProcessorTests
    {
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock = new Mock<ITelemetryHandler>();
        private readonly Mock<IConfigProvider> _configurationMock = new Mock<IConfigProvider>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();


        [Fact]
        public async void InsiderFeed_CricketAustraliaGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_AdelaideStrikersGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "adelaideStrikers.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_AdelaideStrikersMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "adelaideStrikers.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_AdelaideStrikersTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "adelaideStrikers.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_BrisbaneHeatGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "brisbaneHeat.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_BrisbaneHeatMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "brisbaneHeat.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_BrisbaneHeatTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "brisbaneHeat.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_HobartHurricanesGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "hobartHurricanes.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_HobartHurricanesMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "hobartHurricanes.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_HobartHurricanesTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "hobartHurricanes.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_MelbourneRenegadesGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "melbourneRenegades.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_MelbourneRenegadesMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "melbourneRenegades.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_MelbourneRenegadesTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "melbourneRenegades.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_MelbourneStarsGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });
            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "melbourneStars.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_MelbourneStarsMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "melbourneStars.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_MelbourneStarsTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "melbourneStars.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_PerthScorchersGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "perthScorchers.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_PerthScorchersMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "perthScorchers.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_PerthScorchersTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "perthScorchers.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SydneyThunderGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "sydneyThunder.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SydneyThunderMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "sydneyThunder.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SydneyThunderTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "sydneyThunder.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SydneySixersGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "sydneySixers.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SydneySixersMembershipSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "sydneySixers.membership"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SydneySixersTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "sydneySixers.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaShopSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.shop"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaTicketsSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
                .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
                .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.tickets"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object,
                _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaTravelOfficeSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.travelOffice"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_ActGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "ACT.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NSWGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "NSW.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NTGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "NT.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_QLDGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "QLD.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SAGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
                .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
                .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "SA.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object,
                _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_TASGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "TAS.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_VICGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "VIC.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_WAGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "WA.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaCoachingNewsletterSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.coachingNewsletter"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaUmpiringNewsletterSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.umpiringNewsletter"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaSchoolsNewsletterSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.schoolsNewsletter"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaAdminsNewsletterSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.adminsNewsletter"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaBlastNewsletterSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.blastNewsletter"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaTheWicketSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.theWicket"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_ACTParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "ACT.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_CricketAustraliaParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NSWParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "NSW.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NTParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "NT.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_QLDParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "QLD.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_SAParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "SA.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_TASParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "TAS.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_WICParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
                .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
                .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "WIC.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object,
                _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_WAParticipationSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "WA.participation"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_Profile_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Profile = new CdcGetAccountInfoProfile()
                {
                    LastName = "TestL",
                    BirthYear = 1990,
                    Country = "Australia",
                    Email = "a@bc.com",
                    FirstName = "TestF",
                    Gender = "M",
                    Phones = new CdcGetAccountInfoPhones()
                    {
                        Number = "9876543210"
                    },
                    State = "VIC",
                    Zip = "3002"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        //[Fact]
        //public async void InsiderFeed_CricketAustraliaGeneralMarketingSubscription_UpdatedInsider_Failure()
        //{
        //    // Setup

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
        //       .Returns(() => "https://testinsiderurl.com/");

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
        //       .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
        //       .Returns(() => "cricketauuat");

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
        //        .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
        //        .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
        //        .Returns(() => "AIUGKBCKsn2z");

        //    _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
        //        .Returns(() => "https://testCdcUrl.com/");

        //    var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        //    mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        //    Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
        //    messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Content = new StringContent("{'data':{" +
        //                                    "'fail':{" +
        //                                    "'count':1}}}"
        //        )
        //    });

        //    messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}}}}")
        //    });

        //    var client = new HttpClient(new CustomHttpMessageHandler(messages));
        //    _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        //    IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
        //    {
        //        Uid = "Test",
        //        FeedId = 1,
        //        Subscriptions = new SubscriptionRequest()
        //        {
        //            IsSubscribed = true,
        //            Subscription = "adelaideStrikers.generalMarketing"
        //        }
        //    };


        //    // Act
        //    var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

        //    await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessEvent(data));
        //    //await processor.ProcessEvent(data);

        //    // Assert
        //    //Assert.True(true);
        //}

        [Fact]
        public async void InsiderFeed_AllPanel_CricketAustraliaGeneralMarketingSubscription_UpdatedInsider_Successfully()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
               .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderWaCricketApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderWaCricketPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketActApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketActPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderAdelaideStrikersApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderAdelaideStrikersPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderBrisbaneHeatApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderBrisbaneHeatPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderMelbourneStarsApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderMelbourneStarsPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderMelbourneRenegadesApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderMelbourneRenegadesPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketNswApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketNswPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketTasApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketTasPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketVicApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketVicPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderHobartHurricanesApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderHobartHurricanesPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderNtCricketApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderNtCricketPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderPerthScorchersApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderPerthScorchersPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderQldCricketApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderQldCricketPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderSaCaAuApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderSaCaAuPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderPlayCricketAuApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderPlayCricketAuPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderShopCricketAuApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderShopCricketAuPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderSydneySixersApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderSydneySixersPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderSydneyThunderApiKey))
                .Returns(() => "2d3138857b5708196d08134bdf41");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderSydneyThunderPartnerName))
                .Returns(() => "cricketau");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true},\"playcricketau\":{\"isConsentGranted\":true},\"shopcricketau\":{\"isConsentGranted\":true},\"adelaidestrikers\":{\"isConsentGranted\":true},\"brisbaneheat\":{\"isConsentGranted\":true},\"hobarthurricanes\":{\"isConsentGranted\":true},\"melbournerenegades\":{\"isConsentGranted\":true},\"melbournestars\":{\"isConsentGranted\":true},\"perthscorchers\":{\"isConsentGranted\":true},\"sydneysixers\":{\"isConsentGranted\":true},\"sydneythunder\":{\"isConsentGranted\":true},\"cricketact\":{\"isConsentGranted\":true},\"cricketnsw\":{\"isConsentGranted\":true},\"qldcricket\":{\"isConsentGranted\":true},\"sacaau\":{\"isConsentGranted\":true},\"crickettas\":{\"isConsentGranted\":true},\"cricketvic\":{\"isConsentGranted\":true},\"wacricket\":{\"isConsentGranted\":true},\"ntcricket\":{\"isConsentGranted\":true}}}}")
            });

            messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\"}]}")
            });

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IdentityExternalFeedRequest data = new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Subscriptions = new SubscriptionRequest()
                {
                    IsSubscribed = true,
                    Subscription = "cricketAustralia.generalMarketing"
                }
            };


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_AdelaideStrikersNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"adelaideStrikers.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.adelaidestrikers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_BrisbaneheatNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"adelaideStrikers.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.brisbaneheat";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketactNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"adelaideStrikers.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketact";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketauNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"brisbaneHeat.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketnswNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"brisbaneHeat.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketnsw";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_crickettasNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"brisbaneHeat.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.crickettas";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketvicNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"hobartHurricanes.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketvic";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_HobarthurricanesNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"hobartHurricanes.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.hobarthurricanes";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_MelbournerenegadesNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"hobartHurricanes.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.melbournerenegades";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_NtcricketNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"melbourneRenegades.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.ntcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_PerthscorchersNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"melbourneRenegades.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.perthscorchers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_PlaycricketauNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"melbourneRenegades.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.playcricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_QldcricketNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"melbourneStars.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.qldcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_SacaauNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"melbourneStars.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.sacaau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_ShopcricketauNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"melbourneStars.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.shopcricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_SydneysixersNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"perthScorchers.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.sydneysixers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_SydneythunderNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"perthScorchers.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.sydneythunder";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_WacricketNewPanelSubscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"perthScorchers.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.wacricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_MelbournestarsNewPanelSubscription_ProfileData_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"sydneyThunder.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.melbournestars";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_AdelaideStrikersNewPanel_sydneyThundermembership_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"sydneyThunder.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.adelaidestrikers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_BrisbaneheatNewPanel_sydneyThundertickets_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"sydneyThunder.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.brisbaneheat";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketactNewPanel_sydneySixersgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"sydneySixers.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketact";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketauNewPanel_sydneySixersmembership_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"sydneySixers.membership\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketnswNewPanel_sydneySixerstickets_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"sydneySixers.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketnsw";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_crickettasNewPanel_cricketAustraliageneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.crickettas";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketvicNewPanel_cricketAustraliashop_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.shop\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketvic";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_HobarthurricanesNewPanel_cricketAustraliatickets_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.tickets\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.hobarthurricanes";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_MelbournerenegadesNewPanel_cricketAustraliatravelOffice_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.travelOffice\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.melbournerenegades";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_NtcricketNewPanel_ACTgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"ACT.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.ntcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_PerthscorchersNewPanel_NSWgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"NSW.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.perthscorchers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_PlaycricketauNewPanel_NTgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"NT.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.playcricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_QldcricketNewPanel_QLDgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"QLD.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.qldcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_SacaauNewPanel_SAgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"SA.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.sacaau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_ShopcricketauNewPanel_TASgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"TAS.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.shopcricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_SydneysixersNewPanel_VICgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"VIC.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.sydneysixers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_SydneythunderNewPanel_WAgeneralMarketing_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"WA.generalMarketing\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.sydneythunder";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_WacricketNewPanel_cricketAustraliacoachingNewsletter_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.coachingNewsletter\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.wacricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_MelbournestarsNewPanel_cricketAustraliaumpiringNewsletter_Subscription_ProfileData_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.umpiringNewsletter\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.melbournestars";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_AdelaideStrikersNewPanel_cricketAustraliaschoolsNewsletter_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.schoolsNewsletter\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.adelaidestrikers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_BrisbaneheatNewPanel_cricketAustraliaadminsNewsletter_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.adminsNewsletter\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.brisbaneheat";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketactNewPanel_cricketAustraliablastNewsletter_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.blastNewsletter\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketact";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketauNewPanel_cricketAustraliatheWicket_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.theWicket\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketnswNewPanel_ACTparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"ACT.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketnsw";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_crickettasNewPanel_cricketAustraliaparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"cricketAustralia.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.crickettas";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_CricketvicNewPanel_NSWparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"NSW.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.cricketvic";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_HobarthurricanesNewPanel_NTparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"NT.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.hobarthurricanes";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_MelbournerenegadesNewPanel_QLDparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"QLD.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.melbournerenegades";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_NtcricketNewPanel_SAparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"SA.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.ntcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_PerthscorchersNewPanel_TASparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"TAS.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.perthscorchers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_PlaycricketauNewPanel_VICparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"VIC.participation\"";
            MockConfigurationKeyAndHttpClient(subscription, isCdcEmailAccountsResponse);
            var consent = "panel.playcricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void InsiderFeed_QldcricketNewPanel_WAparticipation_Subscription_UpdatedInsider_Successfully(bool isCdcEmailAccountsResponse)
        {
            // Setup
            var subscription = "\"WA.participation\"";
            MockConfigurationKeyAndHttpClient(subscription);
            var consent = "panel.qldcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);

            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Adelaidestrikers_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup
            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.adelaidestrikers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Brisbaneheat_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup
            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.brisbaneheat";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Cricketact_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.cricketact";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Cricketau_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.cricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Cricketnsw_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.cricketnsw";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_crickettas_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.crickettas";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Cricketvic_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.cricketvic";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Hobarthurricanes_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.hobarthurricanes";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Melbournerenegades_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.melbournerenegades";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Elbournestars_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.,elbournestars";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Ntcricket_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.ntcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Perthscorchers_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.perthscorchers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Playcricketau_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.playcricketau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Qldcricket_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.qldcricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Sacaau_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.sacaau";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Shopcricketsu_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.shopcricketsu";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Sydneysixers_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.sydneysixers";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Sydneythunder_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.sydneythunder";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async void InsiderFeed_NewPanel_Wacricket_Subscription_ProfileData_UpdatedInsider_Successfully()
        {
            // Setup

            MockConfigurationKeyAndHttpClient();
            var consent = "subscribe.wacricket";
            IdentityExternalFeedRequest data = SetIdentityExternalFeedRequestData(consent);


            // Act
            var processor = new InsiderIdentityFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            await processor.ProcessEvent(data);

            // Assert
            Assert.True(true);
        }
        private static IdentityExternalFeedRequest SetIdentityExternalFeedRequestData(string consent)
        {

            return new IdentityExternalFeedRequest()
            {
                Uid = "Test",
                FeedId = 1,
                Preferences = new ConsentRequest()
                {
                    Consent = consent,
                    IsConsentGranted = true

                }
            };
        }

        private void MockConfigurationKeyAndHttpClient(string subsciption = "\"\"", bool isCdcEmailAccountsResponse = true)
        {
            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderUpsertDataUrl))
                           .Returns(() => "https://testinsiderurl.com/");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuApiKey))
               .Returns(() => "2d3138857b5708196d08134bdf4167894c8ad1a673c5cfc21fc390b92c0947");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.InsiderCricketAuPartnerName))
               .Returns(() => "cricketauuat");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCApiKey))
                .Returns(() => "4_V_QZAzAvVc9AcWf9KsaeCA");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCSecretKey))
                .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCUserKey))
                .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(External.AppSettingsKey.CDCAccountApiBaseUrl))
                .Returns(() => "https://testCdcUrl.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Dictionary<string, HttpResponseMessage> messages = new Dictionary<string, HttpResponseMessage>();
            messages.Add("https://testinsiderurl.com/", new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'data':{" +
                                            "'successful':{" +
                                            "'count':1}}}"
                )
            });

            if (isCdcEmailAccountsResponse)
            {
                messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"profile\":{\"firstName\":\"Test\",\"lastName\":\"Test\",\"email\":\"Test\",\"gender\":\"Test\",\"country\":\"Test\",\"state\":\"Test\",\"zip\":\"Test\"},\"regSource\":\"Test\"}")
                });

                messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[{\"hasLiteAccount\":false,\"hasFullAccount\":true,\"email\":\"a@b.com\",\"Preferences\":{\"panel\":{\"cricketau\":{\"isConsentGranted\":true}},\"subscribe\":{\"cricketau\":{\"isConsentGranted\":true},\"playcricketau\":{\"isConsentGranted\":true},\"shopcricketau\":{\"isConsentGranted\":true},\"adelaidestrikers\":{\"isConsentGranted\":true},\"brisbaneheat\":{\"isConsentGranted\":true},\"hobarthurricanes\":{\"isConsentGranted\":true},\"melbournerenegades\":{\"isConsentGranted\":true},\"melbournestars\":{\"isConsentGranted\":true},\"perthscorchers\":{\"isConsentGranted\":true},\"sydneysixers\":{\"isConsentGranted\":true},\"sydneythunder\":{\"isConsentGranted\":true},\"cricketact\":{\"isConsentGranted\":true},\"cricketnsw\":{\"isConsentGranted\":true},\"qldcricket\":{\"isConsentGranted\":true},\"sacaau\":{\"isConsentGranted\":true},\"crickettas\":{\"isConsentGranted\":true},\"cricketvic\":{\"isConsentGranted\":true},\"wacricket\":{\"isConsentGranted\":true},\"ntcricket\":{\"isConsentGranted\":true}}},\"Subscriptions\":{" + subsciption + ":{\"email\":{\"isSubscribed\":true,\"doubleOptIn\": {\"status\": \"Confirmed\"}}}}}]}")                   
                });
            }
            else
            {
                messages.Add("https://testcdcurl.com/accounts.getAccountInfo", new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"profile\":{\"firstName\":\"Test\",\"lastName\":\"Test\",\"email\":\"Test\",\"gender\":\"Test\",\"country\":\"Test\",\"state\":\"Test\",\"zip\":\"Test\"},\"Subscriptions\":{" + subsciption + ":{\"\":{\"email\":{\"isSubscribed\":true,\"doubleOptIn\": {\"status\": \"Confirmed\"}}}}},\"regSource\":\"Test\"}")
                });
                messages.Add("https://testcdcurl.com/accounts.search", new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"results\":[]}")
                });
            }

            var client = new HttpClient(new CustomHttpMessageHandler(messages));
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
        }
    }
}
