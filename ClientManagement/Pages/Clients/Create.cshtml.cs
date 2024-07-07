using ClientManagement.Models;
using ClientsManager.Data;
using ClientsManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace ClientsManager.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly ClientDataAccess _dataAccess;

        public CreateModel(ClientDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [BindProperty]
        public Client Client { get; set; } = new Client
        {
            Addresses = new List<Addresses>(),
            Contacts = new List<Contacts>()
        };

        public List<AddressType> AddressTypes { get; set; }
        public List<ContactType> ContactTypes { get; set; }
        public List<Gender> Genders { get; set; }

        public void OnGet()
        {
            AddressTypes = GetAddressTypes();
            ContactTypes = GetContactTypes();
            Genders = GetGenders();
        }

        public IActionResult OnPost()
        {
            _dataAccess.AddClient(Client);
            return RedirectToPage("/Clients/Index");
        }

        private List<AddressType> GetAddressTypes()
        {
            var addressTypes = new List<AddressType>();
            using (var connection = new SqlConnection(_dataAccess.ConnectionString))
            {
                var command = new SqlCommand("SELECT * FROM AddressTypes", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        addressTypes.Add(new AddressType
                        {
                            AddressTypeId = (int)reader["AddressTypeId"],
                            Type = reader["Type"].ToString()
                        });
                    }
                }
            }
            return addressTypes;
        }

        private List<ContactType> GetContactTypes()
        {
            var contactTypes = new List<ContactType>();
            using (var connection = new SqlConnection(_dataAccess.ConnectionString))
            {
                var command = new SqlCommand("SELECT * FROM ContactTypes", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contactTypes.Add(new ContactType
                        {
                            ContactTypeId = (int)reader["ContactTypeId"],
                            Type = reader["Type"].ToString()
                        });
                    }
                }
            }
            return contactTypes;
        }

        private List<Gender> GetGenders()
        {
            var genders = new List<Gender>();
            using (var connection = new SqlConnection(_dataAccess.ConnectionString))
            {
                var command = new SqlCommand("SELECT * FROM Genders", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        genders.Add(new Gender
                        {
                            GenderId = (int)reader["GenderId"],
                            Type = reader["Type"].ToString()
                        });
                    }
                }
            }
            return genders;
        }
    }
}