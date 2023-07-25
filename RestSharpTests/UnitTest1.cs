using NUnit.Framework;
using RestSharpTests.GitHub;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RestSharpTests
{
    /// <summary>
    /// To get a Personal Access Token:
    /// 1. Go to your GitHub account.
    /// 2. Click your profile picture.
    /// 3. Click "Settings"
    /// 4. Click "&lt;&gt; Developer Settings"
    /// 5. Click "Personal access tokens"
    /// 6. Click "Generate new token" (Password may be required)
    /// 7. Give the token a descriptive name.
    /// 8. Give it an expiration date. If compromised, the longer the attacker
    ///    has access the more damage that can be done.
    /// 9. Give the token the permissions necessary and only the ones needed.
    /// 10 Click "Generate token"
    /// </summary>
    [TestFixture]
    public class Tests
    {
        private GitHubRestApi _gitHubApi;

        [OneTimeSetUp]
        public void Setup()
        {
            _gitHubApi = new GitHubRestApi();
        }

        [Test]
        public void Test1()
        {
            var zen = _gitHubApi.GetZen().Result;

            Assert.IsNotNull(zen);

            Debug.WriteLine(zen);
        }

        [Test]
        public async Task Test2()
        {
            var profile = await _gitHubApi.GetUserProfile("RolandChristensen");

            Assert.IsNotNull(profile);

            var output = "Login: " + profile.Login + Environment.NewLine +
                "Name: " + profile.Name + Environment.NewLine +
                "ID: " + profile.id;

            Debug.WriteLine(output);
        }

        [Test]
        public async Task Test3()
        {
            var repos = await _gitHubApi.GetYourRepos();

            Assert.IsNotNull(repos);
        }

        [Test]
        public async Task Test4()
        {
            var profile = await _gitHubApi.GetUserProfile2("RolandChristensen");

            Assert.IsNotNull(profile);

            var output = "Login: " + profile.Login + Environment.NewLine +
                "Name: " + profile.Name + Environment.NewLine +
                "ID: " + profile.id;

            Debug.WriteLine(output);
        }
    }
}