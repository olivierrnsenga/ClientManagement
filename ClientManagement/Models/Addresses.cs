using ClientManagement.Models;

namespace ClientsManager.Models
{
    public class Addresses
    {
        public int AddressId { get; set; }
        public int ClientId { get; set; }
        public int AddressTypeId { get; set; }
        public string Address { get; set; }

        public Client Client { get; set; }
        public AddressType AddressType { get; set; }
    }

}
