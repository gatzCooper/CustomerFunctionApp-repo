using azChallengeFA_Customer.model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azChallengeFA_Customer.services
{
    public class CustomerService : ICustomerService
    {
        private readonly string _cosmosDbDatabaseName;
        private readonly string _cosmosDbContainerName;
        private readonly ILogger<CustomerService> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly string _eventGridTopicEndpoint;
        private readonly string _eventGridTopicKey;


     
        public CustomerService(ILogger<CustomerService> logger, CosmosClient cosmosClient, IConfiguration config)
        {
            _logger = logger;
            _cosmosDbDatabaseName = config["CosmosDbDatabaseName"];
            _cosmosDbContainerName = config["CosmosDbContainerName"];
            _cosmosClient = cosmosClient;
            _eventGridTopicEndpoint = config["EventGridTopicEndpoint"];
            _eventGridTopicKey = config["EventGridTopicKey"];

        }
        public async Task SaveCustomerToCosmosDbAsync(CustomerInfo customer)
        {
            var container = _cosmosClient.GetContainer(_cosmosDbDatabaseName, _cosmosDbContainerName);
            await container.CreateItemAsync(customer);
        }

        public async Task PublishEventAsync(dynamic data)
        {
             TopicCredentials topicCredentials = new TopicCredentials(_eventGridTopicKey);
             EventGridClient eventGridClient = new EventGridClient(topicCredentials);
             var topicHostname = new Uri(_eventGridTopicEndpoint).Host;

            var eventGridEvents = new List<EventGridEvent>
        {
            new EventGridEvent
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "DataChangeEvent",
                DataVersion = "1.0",
                Subject = "DataChange",
                Data = data,
                EventTime = DateTime.UtcNow
            }
        };

            await eventGridClient.PublishEventsAsync(topicHostname, eventGridEvents);
        }

    }
}
