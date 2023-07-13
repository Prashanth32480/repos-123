﻿using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Grassroots.Identity.Functions.Cdc.Test.Cdc
{
    public class CdcGetAuditDetailsActivityFunctionTests
    {
        [Fact]
        public async void CdcGetAuditDetailsActivityFunction_Calls_Cdc_Success()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _httpClientFactory = new Mock<IHttpClientFactory>();
            var _configurationMock = new Mock<IConfigProvider>();

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAuditApiBaseUrl))
               .Returns(() => "https://audit.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            CdcAuditSearchRequest auditReq = new CdcAuditSearchRequest()
            {
                ApiKey = "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                FeedId = 1,
                Uid = "842a3f1a453649bba3cf8fa05c2e7304"
            };
            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<CdcAuditSearchRequest>()).Returns(auditReq);
            var function = new CdcGetAuditDetailsActivityFunction(_telemetry.Object, _httpClientFactory.Object, _configurationMock.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(0, result.ErrorCode);
        }

        [Fact]
        public async void CdcGetAuditDetailsActivityFunction_Calls_Cdc_Failure()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _httpClientFactory = new Mock<IHttpClientFactory>();
            var _configurationMock = new Mock<IConfigProvider>();

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAuditApiBaseUrl))
               .Returns(() => "https://audit.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":1230,\"apiVersion\":2,\"statusCode\":401,\"statusReason\":\"UnAuthorized user\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            CdcAuditSearchRequest auditReq = new CdcAuditSearchRequest()
            {
                ApiKey = "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd",
                CallId = "842a3f1a453649bba3cf8fa05c2e7303",
                FeedId = 1,
                Uid = "842a3f1a453649bba3cf8fa05c2e7304"
            };
            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<CdcAuditSearchRequest>()).Returns(auditReq);
            var function = new CdcGetAuditDetailsActivityFunction(_telemetry.Object, _httpClientFactory.Object, _configurationMock.Object);
            //var result = await function.Run(durableActivityContextMock.Object);
            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));
            //Assert.Equal(1230, result.ErrorCode);
        }
    }
}