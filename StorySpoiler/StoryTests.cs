using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;


namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("burgaski95", "502319a");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }


        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            var story = new StoryDTO { Title = "New Story", Description = "This is a spoiler" };
            request.AddJsonBody(story);

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Data?.StoryId, Is.Not.Null.Or.Empty);
            Assert.That(response.Data?.Msg, Is.EqualTo("Successfully created!"));
            createdStoryId = response.Data?.StoryId; // Store the created story's ID
        }


        [Test, Order(2)]
        public void EditStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/Story/Edit/{createdStoryId}", Method.Put);
            var story = new StoryDTO { Title = "Updated Story", Description = "Updated spoiler" };
            request.AddJsonBody(story);

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data?.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("[]").Or.Contains("["));
        }


        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data?.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            var story = new StoryDTO { Title = "", Description = "" };
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }



        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var nonExistingStoryId = "non-existing-id";
            var request = new RestRequest($"/Story/Edit/{nonExistingStoryId}", Method.Put);
            var story = new StoryDTO { Title = "Non-existent Story", Description = "This does not exist" };
            request.AddJsonBody(story);

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Data?.Msg, Is.EqualTo("No spoilers..."));
        }


        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var nonExistingStoryId = "non-existing-id";
            var request = new RestRequest($"/Story/Delete/{nonExistingStoryId}", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data?.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}