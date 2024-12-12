using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_CategoryLifecycle_RecipeBook()
        {
            // Step 1: Create a new category
            var createRequest = new RestRequest("/category", Method.Post);
             createRequest.AddHeader("Authorization", $"Bearer {token}"); 
            createRequest.AddJsonBody(new { name = "Vegan Recipes" }); 
            var createResponse = client.Execute(createRequest); 
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Create category failed."); 
            var createResponseBody = JObject.Parse(createResponse.Content); 
            var categoryId = createResponseBody["_id"].ToString(); 
            Assert.That(categoryId, Is.Not.Null.Or.Empty, "Category ID should be present."); 

            // Step 2: Get all categories
            var getAllRequest = new RestRequest("/category", Method.Get); 
            getAllRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var getAllResponse = client.Execute(getAllRequest); 
            Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Get all categories failed."); 
            Assert.That(getAllResponse.Content, Is.Not.Null.Or.Empty, "Get all categories response should not be empty."); 
            var categories = JArray.Parse(getAllResponse.Content); 
            Assert.That(categories.Count, Is.GreaterThanOrEqualTo(1), "There should be at least one category."); 
            
            // Step 3: Get category by ID
            var getByIdRequest = new RestRequest($"/category/{categoryId}", Method.Get); 
            getByIdRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var getByIdResponse = client.Execute(getByIdRequest); 
            Assert.That(getByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Get category by ID failed."); 
            Assert.That(getByIdResponse.Content, Is.Not.Null.Or.Empty, "Get category by ID response should not be empty."); 
            var category = JObject.Parse(getByIdResponse.Content); 
            Assert.That(category["_id"].ToString(), Is.EqualTo(categoryId), "Category ID should match."); 
            Assert.That(category["name"].ToString(), Is.EqualTo("Vegan Recipes"), "Category name should be 'Vegan Recipes'."); 
            
            // Step 4: Edit the category
            var editRequest = new RestRequest($"/category/{categoryId}", Method.Put); 
            editRequest.AddHeader("Authorization", $"Bearer {token}"); 
            editRequest.AddJsonBody(new { name = "Healthy Vegan Recipes" }); 
            var editResponse = client.Execute(editRequest); 
            Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Edit category failed."); 
            
            // Step 5: Verification (after edit)
            
            var verifyEditRequest = new RestRequest($"/category/{categoryId}", Method.Get); 
            verifyEditRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var verifyEditResponse = client.Execute(verifyEditRequest); 
            Assert.That(verifyEditResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Verify edited category failed."); 
            Assert.That(verifyEditResponse.Content, Is.Not.Null.Or.Empty, "Verify edited category response should not be empty."); 
            var updatedCategory = JObject.Parse(verifyEditResponse.Content); 
            Assert.That(updatedCategory["name"].ToString(), Is.EqualTo("Healthy Vegan Recipes"), "Category name should be updated to 'Healthy Vegan Recipes'."); 
            
            // Step 6: Delete the category
            var deleteRequest = new RestRequest($"/category/{categoryId}", Method.Delete); 
            deleteRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var deleteResponse = client.Execute(deleteRequest); 
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Delete category failed."); 
            
            // Step 7: Verify the deleted category cannot be found
            var verifyDeleteRequest = new RestRequest($"/category/{categoryId}", Method.Get); 
            verifyDeleteRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var verifyDeleteResponse = client.Execute(verifyDeleteRequest); 
            Assert.That(verifyDeleteResponse.Content, Is.EqualTo("null"), "Deleted category should not be found."); 

        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
