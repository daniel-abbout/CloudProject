using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudPoject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly string _sqlConnectionString;
        private static readonly List<Student> value = [];

        public List<Student> Students { get; private set; } = value;

        public IndexModel()
        {
            // Fetch secrets from Key Vault
            var kvUri = new Uri("https://danielk.vault.azure.net/");
            var secretClient = new SecretClient(kvUri, new DefaultAzureCredential());

            _sqlConnectionString = secretClient.GetSecret("DbConnectionString").Value.Value;
        }

        public async Task OnGetAsync()
        {
            await FetchStudentsFromDatabase();
        }

        private async Task FetchStudentsFromDatabase()
        {
            using var connection = new SqlConnection(_sqlConnectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT Id, Name FROM Students", connection);
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Students.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}