using System;
using Xunit;
using Source;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        private readonly string ApiServer = "http://localhost:4428";
        private readonly string IdentityServer = "http://someIdentityServer.no";

        private readonly string userName = "testuser@uc.no";
        private readonly string password = "secret";               

        [Fact]
        public void Test2()
        {

            var apiRunner = new FluentApiRunner()
                .SetServer(ApiServer)
                .SetLocalpath("/api/somecontroller")
                .AddParam("Id", "0")
                .AddParam("Weeks", "201723")
                .AddParam("Weeks", "201725")
                .AddAutenticationHeader("Bearer", GetLoginToken())
                .Get();

            Console.WriteLine(apiRunner.Contents);
            Assert.True(apiRunner.Response.IsSuccessStatusCode, "Statuscode not 200");
        }

        [Fact]
        public void DemoMetodeMJson()
        {
            var submittedApproveDto = @"
            {
                'Id': 0,
                'Week': 201725,
                'Approve': true,
                'DataSource': 'crm',
                'Comment': 'First automated comment',
                'UserId': [47]
            }";

            var apiRunner2 = new FluentApiRunner()
                .SetServer(ApiServer)
                .SetLocalpath("api/somecontroller")
                .AddAutenticationHeader("Bearer", GetLoginToken())
                .AddJson(submittedApproveDto)
                .Post();

            Console.WriteLine(apiRunner2.Contents);
            Assert.True(apiRunner2.Response.IsSuccessStatusCode, "Statuscode not 200");
            Assert.True(apiRunner2.Contents.Contains("First automated comment"), "User should return input");
        }

        private string GetLoginToken()
        {
            var apiRunner = new FluentApiRunner()
                .SetServer(IdentityServer)
                .SetLocalpath("/connect/token")
                .AddAutenticationHeader("Basic", "sdfsdfsdfsdfsdf") // <= ********Header her *********
                .AddParam("username", userName)
                .AddParam("Password", password)
                .AddParam("scope", "api1 offline_access email roles openid")
                .AddParam("grant_type", "password")
                .Post()
                .ProcessIdentityserverResults();  // <== Denne metoden må kanskje endres for SPV

            return apiRunner.AccessToken;
        }     
    }
}
