using ClientsManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace ClientsManager.Pages.Clients
{
    public class ExportModel : PageModel
    {
        private readonly ClientDataAccess _dataAccess;

        public ExportModel(ClientDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public IActionResult OnGet()
        {
            var clients = _dataAccess.GetClients();
            var builder = new StringBuilder();
            builder.AppendLine("Name,GenderId,Details");

            foreach (var client in clients)
            {
                builder.AppendLine($"{client.Name},{client.GenderId},{client.Details}");
            }

            var content = builder.ToString();
            return File(Encoding.UTF8.GetBytes(content), "text/csv", "clients.csv");
        }
    }
}
