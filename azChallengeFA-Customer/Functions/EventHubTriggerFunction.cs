// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using azChallengeFA_Customer.services;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using azChallengeFA_Customer.model;

namespace azChallengeFA_Customer.Functions
{
    public  class EventHubTriggerFunction
    {
        private readonly ILogger<EventHubTriggerFunction> _logger;
        private readonly ICustomerService _customerService;

        public EventHubTriggerFunction(ILogger<EventHubTriggerFunction> logger, ICustomerService customerService)
        {
                _customerService = customerService;
                _logger = logger;
        }

        [FunctionName("EventHubTriggerFunction")]
        public async Task Run([EventGridTrigger]EventGridEvent eventGridEvent)
        {
            _logger.LogInformation($"C# Event Grid trigger function processed an event: {eventGridEvent.EventType}");

            try
            {
                // Deserialize the binary data to the Customer class
                var customer = JsonConvert.DeserializeObject<CustomerInfo>(Encoding.UTF8.GetString(eventGridEvent.Data));

                if (customer != null && !string.IsNullOrEmpty(customer.FirstName) && !string.IsNullOrEmpty(customer.LastName) && !string.IsNullOrEmpty(customer.Email))
                {
                    await _customerService.UpsertCustomerDataAsync(customer);
                    _logger.LogInformation($"Customer data upserted: {customer.FirstName} {customer.LastName} ({customer.Email})");
                }
                else
                {
                    _logger.LogError("Invalid event data. Expected properties not found.");
                }

            }

            catch (Exception ex)
            {
                _logger.LogError($"An error occured: {ex.StackTrace}");
            }
        }
    }
}
