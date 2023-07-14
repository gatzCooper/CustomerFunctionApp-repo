using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using azChallengeFA_Customer.model;
using azChallengeFA_Customer.services;
using Microsoft.Azure.Cosmos;

namespace azChallengeFA_Customer.Functions
{
    public class SaveCustomerFunction
    {
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;

        public SaveCustomerFunction(ILogger<SaveCustomerFunction> logger, ICustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }

        [FunctionName("SaveCustomerFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(requestBody);

                await _customerService.SaveCustomerToCosmosDbAsync(customerInfo);

                return new OkObjectResult("Customer save successfully!");
            }

            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid Customer JSON payload.");
                return new BadRequestResult();
            }

            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to save customer info to Cosmos DB.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


        }
    }
}
