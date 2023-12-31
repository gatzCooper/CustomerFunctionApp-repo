﻿using azChallengeFA_Customer.model;
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
        public CustomerService(ILogger<CustomerService> logger, CosmosClient cosmosClient, IConfiguration config)
        {
            _logger = logger;
            _cosmosDbDatabaseName = config["CosmosDbDatabaseName"];
            _cosmosDbContainerName = config["CosmosDbContainerName"];
            _cosmosClient = cosmosClient;
        }
        public async Task SaveCustomerToCosmosDbAsync(CustomerInfo customer)
        {
            var cust = new CustomerInfo
            {
                Id = Guid.NewGuid(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                BirthdayInEpoch = customer.BirthdayInEpoch,
                Email = customer.Email
            };
            var container = _cosmosClient.GetContainer(_cosmosDbDatabaseName, _cosmosDbContainerName);
            await container.CreateItemAsync(cust);
        }

        public async Task UpsertCustomerDataAsync(CustomerInfo  customer)
        {
            var container =_cosmosClient.GetContainer(_cosmosDbDatabaseName, _cosmosDbContainerName);
            await container.UpsertItemAsync(customer, new PartitionKey(customer.Id.ToString()));
        }

    }
}
