using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ToDoCRUD
{
    public static class ToDoFunction
    {
        [FunctionName("CreateToDo")]
        public static async Task<IActionResult> CreateToDo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "todo")] HttpRequest req,
            [Blob("todos/{rand-guid}.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] TextWriter outputBlob,
            ILogger log)
        {
            log.LogInformation("Creating a new ToDo item.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var todoItem = new ToDoItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = data.name,
                Description = data.description,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            string json = JsonConvert.SerializeObject(todoItem);
            await outputBlob.WriteAsync(json);

            return new OkObjectResult(todoItem);
        }

        [FunctionName("GetToDo")]
        public static IActionResult GetToDo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "todo/{id}")] HttpRequest req,
            [Blob("todos/{id}.json", FileAccess.Read, Connection = "AzureWebJobsStorage")] string todoJson,
            ILogger log, string id)
        {
            log.LogInformation($"Getting ToDo item with ID: {id}");

            if (string.IsNullOrEmpty(todoJson))
            {
                return new NotFoundResult();
            }

            var todoItem = JsonConvert.DeserializeObject<ToDoItem>(todoJson);
            return new OkObjectResult(todoItem);
        }

        [FunctionName("UpdateToDo")]
        public static async Task<IActionResult> UpdateToDo(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "todo/{id}")] HttpRequest req,
            [Blob("todos/{id}.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] TextWriter outputBlob,
            ILogger log, string id)
        {
            log.LogInformation($"Updating ToDo item with ID: {id}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedTodoItem = JsonConvert.DeserializeObject<ToDoItem>(requestBody);
            updatedTodoItem.Id = id;

            string json = JsonConvert.SerializeObject(updatedTodoItem);
            await outputBlob.WriteAsync(json);

            return new OkObjectResult(updatedTodoItem);
        }

        [FunctionName("DeleteToDo")]
        public static async Task<IActionResult> DeleteToDo(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "todo/{id}")] HttpRequest req,
            [Blob("todos/{id}.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream blob,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting ToDo item with ID: {id}");

            blob.SetLength(0);

            return new OkResult();
        }
    }

    public class ToDoItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    // public static class ToDoCRUD_BlobStorage
    // {
    //     [FunctionName("ToDoCRUD_BlobStorage")]
    //     public static async Task<IActionResult> Run(
    //         [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
    //         ILogger log)
    //     {
    //         log.LogInformation("C# HTTP trigger function processed a request.");

    //         string name = req.Query["name"];

    //         string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    //         dynamic data = JsonConvert.DeserializeObject(requestBody);
    //         name = name ?? data?.name;

    //         string responseMessage = string.IsNullOrEmpty(name)
    //             ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
    //             : $"Hello, {name}. This HTTP triggered function executed successfully.";

    //         return new OkObjectResult(responseMessage);
    //     }
    // }
}
