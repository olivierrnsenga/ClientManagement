using ClientManagement.Models;

namespace ClientsManager.Models
{
    public class Contacts
    {
        public int ContactId { get; set; }
        public int ClientId { get; set; }
        public int ContactTypeId { get; set; }
        public string Contact { get; set; }

        public Client Client { get; set; }
        public ContactType ContactType { get; set; }
    }

}