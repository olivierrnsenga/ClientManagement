using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientsManager.Data;
using ClientManagement.Models;
using System.Text;

namespace ClientsManager.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly ClientDataAccess _dataAccess;

        public IndexModel(ClientDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public IEnumerable<Client> Clients { get; private set; }
        public IEnumerable<Gender> Genders { get; private set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; private set; }

        private const int PageSize = 10;

        [BindProperty]
        public Client Client { get; set; }

        public void OnGet()
        {
            Genders = _dataAccess.GetGenders();
            var clients = _dataAccess.GetClients();

            TotalPages = (int)Math.Ceiling(clients.Count() / (double)PageSize);
            Clients = clients.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _dataAccess.UpdateClient(Client);
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            _dataAccess.DeleteClient(id);
            return RedirectToPage();
        }

        public IActionResult OnPostExport()
        {
            Genders = _dataAccess.GetGenders();
            var clients = _dataAccess.GetClients().ToList();
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("ClientId,Name,Gender,Details,AddressType,Address");

            foreach (var client in clients)
            {
                var gender = Genders.FirstOrDefault(g => g.GenderId == client.GenderId)?.Type ?? "Not Specified";
                var addresses = _dataAccess.GetAddressesByClientId(client.ClientId);
                foreach (var address in addresses)
                {
                    csvBuilder.AppendLine($"{client.ClientId},{client.Name},{gender},{client.Details},{address.AddressType.Type},{address.Address}");
                }
            }

            var csvContent = csvBuilder.ToString();
            var bytes = Encoding.UTF8.GetBytes(csvContent);
            var output = new MemoryStream(bytes);

            return File(output, "text/csv", "clients_with_addresses.csv");
        }
    }
}