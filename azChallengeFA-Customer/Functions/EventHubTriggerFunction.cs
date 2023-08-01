// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using azChallengeFA_Customer.services;
using System.Threading.Tasks;

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
        public async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

            try
            {
                await _customerService.PublishEventAsync(eventGridEvent.Data);
            }

            catch (Exception ex)
            {
                _logger.LogError($"An error occured: {ex.StackTrace}");
            }
        }
    }
}
