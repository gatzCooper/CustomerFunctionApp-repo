using azChallengeFA_Customer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azChallengeFA_Customer.services
{
    public interface ICustomerService
    {
        Task SaveCustomerToCosmosDbAsync(CustomerInfo customer);
        Task PublishEventAsync(dynamic data);
    }
}
