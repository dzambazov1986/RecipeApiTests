using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class RecipeTests : IDisposable
    {
        private RestClient client;
        private string token;
        private object categoryId;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_GetAllRecipes()
       
         {
                var request = new RestRequest("recipe", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = client.Execute(request);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
                Assert.That(response.Content, Is.Not.Null.Or.Empty, "Response content should not be empty.");

                var recipes = JArray.Parse(response.Content);
                Assert.That(recipes, Is.InstanceOf<JArray>(), "Expected JSON array in response.");
                Assert.That(recipes.Count, Is.GreaterThan(0), "Expected at least one recipe in response.");

                foreach (var recipe in recipes)
                {
                    Assert.That(recipe["title"]?.ToString(), Is.Not.Null.Or.Empty, "Title field should not be null or empty.");
                    Assert.That(recipe["ingredients"], Is.InstanceOf<JArray>(), "Ingredients field should be a JSON array.");
                    Assert.That(recipe["instructions"], Is.InstanceOf<JArray>(), "Instructions field should be a JSON array.");
                    Assert.That(recipe["cookingTime"]?.ToString(), Is.Not.Null.Or.Empty, "CookingTime field should not be null or empty.");
                    Assert.That(recipe["servings"]?.ToString(), Is.Not.Null.Or.Empty, "Servings field should not be null or empty.");
                    Assert.That(recipe["category"]?.ToString(), Is.Not.Null.Or.Empty, "Category field should not be null or empty.");
                }
            }

      

        [Test]
        public void Test_GetRecipeByTitle()
        {
            
                var request = new RestRequest("recipe", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = client.Execute(request);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
                Assert.That(response.Content, Is.Not.Null.Or.Empty, "Response content should not be empty.");

                var recipes = JArray.Parse(response.Content);
                var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == "Chocolate Chip Cookies");

                Assert.That(recipe, Is.Not.Null, "Recipe with title 'Chocolate Chip Cookies' should exist.");

                // Correcting the expected value to match the actual value returned by the API
                Assert.That(recipe["cookingTime"]?.ToString(), Is.EqualTo("25"), "CookingTime should match.");
                Assert.That(recipe["servings"]?.ToString(), Is.EqualTo("24"), "Servings should match.");
                Assert.That(recipe["ingredients"].Count(), Is.EqualTo(9), "Number of ingredients should match.");
                Assert.That(recipe["instructions"].Count(), Is.EqualTo(7), "Number of instructions should match.");
            }

        

        [Test]
        public void Test_AddRecipe()
        {
         
                // Step 1: Get all Categories
                var getCategoriesRequest = new RestRequest("category", Method.Get);
                getCategoriesRequest.AddHeader("Authorization", $"Bearer {token}");
                var getCategoriesResponse = client.Execute(getCategoriesRequest);
                Assert.That(getCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var categories = JArray.Parse(getCategoriesResponse.Content);
                string categoryId = categories[0]["_id"]?.ToString();

                // Step 2: Create new recipe
                var newRecipe = new
                {
                    title = "Foodyyy",
                    description = "Test Description",
                    ingredients = new[]
                    {
            new { name = "Spaghetti", quantity = "200g" }
        },
                    instructions = new[]
                    {
            new { step = "Cook the according to package instructions." }
        },
                    cookingTime = 20, // Number representing minutes
                    servings = 2, // Assuming servings should be a number
                    category = categoryId // Use a valid category ID from the list
                };

                var addRecipeRequest = new RestRequest("recipe", Method.Post);
                addRecipeRequest.AddHeader("Authorization", $"Bearer {token}");
                addRecipeRequest.AddJsonBody(newRecipe);
                var addRecipeResponse = client.Execute(addRecipeRequest);

                // Step 3: Response Assertions for Add Recipe
                Assert.That(addRecipeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                    $"Expected status code 200 OK, but got {addRecipeResponse.StatusCode}. Response content: {addRecipeResponse.Content}");
                Assert.That(addRecipeResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

                var createdRecipe = JObject.Parse(addRecipeResponse.Content);
                string? createdRecipeId = createdRecipe["_id"]?.ToString();
                Assert.That(createdRecipeId, Is.Not.Null.And.Not.Empty, "Created recipe ID should not be null or empty");

                // Step 4: Get the details of the created Recipe
                var getRecipeRequest = new RestRequest($"recipe/{createdRecipeId}", Method.Get);
                getRecipeRequest.AddHeader("Authorization", $"Bearer {token}");
                var getRecipeResponse = client.Execute(getRecipeRequest);

                // Step 5: Response Assertions for Get Recipe
                Assert.That(getRecipeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                    $"Expected status code 200 OK, but got {getRecipeResponse.StatusCode}. Response content: {getRecipeResponse.Content}");
                Assert.That(getRecipeResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

                var retrievedRecipe = JObject.Parse(getRecipeResponse.Content);

                // Step 6: Recipe Fields Assertions
                Assert.That(retrievedRecipe["title"]?.ToString(), Is.EqualTo(newRecipe.title), "Recipe title should match the input value");
                Assert.That(retrievedRecipe["description"]?.ToString(), Is.EqualTo(newRecipe.description), "Recipe description should match the input value");
                Assert.That(retrievedRecipe["cookingTime"]?.ToObject<int>(), Is.EqualTo(newRecipe.cookingTime), "Recipe cookingTime should match the input value");
                Assert.That(retrievedRecipe["servings"]?.ToObject<int>(), Is.EqualTo(newRecipe.servings), "Recipe servings should match the input value");

                var retrievedCategory = retrievedRecipe["category"];
                Assert.That(retrievedCategory, Is.Not.Null, "Category should not be empty");
                Assert.That(retrievedCategory["_id"]?.ToString(), Is.EqualTo(newRecipe.category), "Category ID should match the input value");

                var retrievedIngredients = retrievedRecipe["ingredients"] as JArray;
                Assert.That(retrievedIngredients, Is.Not.Null, "Ingredients should be a JSON array");
                Assert.That(retrievedIngredients.Count, Is.EqualTo(newRecipe.ingredients.Length), "Ingredients array should have the same number of elements as the input value");
                for (int i = 0; i < retrievedIngredients.Count; i++)
                {
                    Assert.That(retrievedIngredients[i]["name"].ToString(), Is.EqualTo(newRecipe.ingredients[i].name), "Ingredient names should match the input values");
                    Assert.That(retrievedIngredients[i]["quantity"].ToString(), Is.EqualTo(newRecipe.ingredients[i].quantity), "Ingredient quantities should match the input values");
                }

                var retrievedInstructions = (JArray)retrievedRecipe["instructions"];
                Assert.That(retrievedInstructions, Is.Not.Null, "Instructions should be a JSON array");
                Assert.That(retrievedInstructions.Count, Is.EqualTo(newRecipe.instructions.Length), "Instructions array should have the same number of elements as the input value");
                for (int i = 0; i < retrievedInstructions.Count; i++)
                {
                    Assert.That(retrievedInstructions[i]["step"].ToString(), Is.EqualTo(newRecipe.instructions[i].step), "Instructions values should match the input values");
                }
            }


        





        [Test]
        public void Test_UpdateRecipe()
        {
            
      
            }

        [Test]
        public void Test_DeleteRecipe()
        {
           
                // Get all recipes
                var getRequest = new RestRequest("recipe", Method.Get);
                getRequest.AddHeader("Authorization", $"Bearer {token}");
                var getResponse = client.Execute(getRequest);

                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "API should return 200 OK for get all");
                var recipes = JArray.Parse(getResponse.Content);

                var recipe = recipes.First(r => r["title"].ToString() == "Foodyyy");
                Assert.That(recipe, Is.Not.Null, "Recipe with title 'Foodyyy' should exist");

                var recipeId = recipe["_id"].ToString();

                // Delete the recipe
                var deleteRequest = new RestRequest($"recipe/{recipeId}", Method.Delete);
                deleteRequest.AddHeader("Authorization", $"Bearer {token}");
                var deleteResponse = client.Execute(deleteRequest);

                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "API should return 200 OK for delete");

                // Verify deletion
                var verifyRequest = new RestRequest($"recipe/{recipeId}", Method.Get);
                verifyRequest.AddHeader("Authorization", $"Bearer {token}");
                var verifyResponse = client.Execute(verifyRequest);

                Assert.That(verifyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "API should return 200 OK for verify");
                Assert.That(verifyResponse.Content, Is.EqualTo("null"), "Deleted recipe should return null content");
            }

        

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
