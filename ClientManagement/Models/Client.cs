using ClientsManager.Models;

public class Client
{
    public int ClientId { get; set; }
    public string Name { get; set; }
    public int GenderId { get; set; }
    public string Details { get; set; }

    public List<Addresses> Addresses { get; set; } = new List<Addresses>();
    public List<Contacts> Contacts { get; set; } = new List<Contacts>();
}