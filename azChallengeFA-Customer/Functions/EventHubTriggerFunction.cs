// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using azChallengeFA_Customer.services;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using azChallengeFA_Customer.model;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Cosmos.Core;

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
                var eventData = eventGridEvent.Data as JObject;

                if (eventData == null)
                {
                    // Convert BinaryData to a string and then deserialize to JObject
                    var eventDataAsString = Encoding.UTF8.GetString((byte[])eventGridEvent.Data);
                    eventData = JObject.Parse(eventDataAsString);
                }

                // Check if eventData contains the necessary properties
                if (eventData.ContainsKey("FirstName") && eventData.ContainsKey("LastName") && eventData.ContainsKey("Email"))
                {
                    // Extract the customer information
                    string Id = eventData["Id"].ToString();
                    string firstName = eventData["FirstName"].ToString();
                    string lastName = eventData["LastName"].ToString();
                    long birthdayInEpoch = eventData.ContainsKey("BirthdayInEpoch") ? eventData["BirthdayInEpoch"].ToObject<long>() : 0;
                    string email = eventData["Email"].ToString();

                    // Create a Customer object from the event data
                    var customer = new CustomerInfo
                    {
                        Id = Guid.Parse(Id),
                        FirstName = firstName,
                        LastName = lastName,
                        BirthdayInEpoch = birthdayInEpoch,
                        Email = email
                    };

                    // Upsert the customer data into Cosmos DB
                    await _customerService.UpsertCustomerDataAsync(customer);

                    _logger.LogInformation($"Customer data upserted: {firstName} {lastName} ({email})");
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
