using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azChallengeFA_Customer.model
{
    public class CustomerInfo
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long BirthdayInEpoch { get; set; }
        public string Email { get; set; }
    }
}
