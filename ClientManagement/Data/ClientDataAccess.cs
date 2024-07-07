using ClientManagement.Models;
using ClientsManager.Models;
using System.Data.SqlClient;

namespace ClientsManager.Data
{
    public class ClientDataAccess
    {
        public string ConnectionString { get; }

        public ClientDataAccess(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IEnumerable<Client> GetClients()
        {
            var clients = new List<Client>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand("SELECT c.*, g.Type AS Gender FROM Clients c JOIN Genders g ON c.GenderId = g.GenderId", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientId = (int)reader["ClientId"],
                            Name = reader["Name"].ToString(),
                            GenderId = (int)reader["GenderId"],
                            Details = reader["Details"].ToString()
                        });
                    }
                }
            }

            foreach (var client in clients)
            {
                client.Addresses = (List<Addresses>)GetAddressesByClientId(client.ClientId);
                client.Contacts = (List<Contacts>)GetContactsByClientId(client.ClientId);
            }

            return clients;
        }

        public Client GetClientById(int id)
        {
            Client client = null;
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand("SELECT c.*, g.Type AS Gender FROM Clients c JOIN Genders g ON c.GenderId = g.GenderId WHERE ClientId = @ClientId", connection);
                command.Parameters.AddWithValue("@ClientId", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        client = new Client
                        {
                            ClientId = (int)reader["ClientId"],
                            Name = reader["Name"].ToString(),
                            GenderId = (int)reader["GenderId"],
                            Details = reader["Details"].ToString()
                        };
                    }
                }
            }

            if (client != null)
            {
                client.Addresses = (List<Addresses>)GetAddressesByClientId(client.ClientId);
                client.Contacts = (List<Contacts>)GetContactsByClientId(client.ClientId);
            }
            return client;
        }

        public IEnumerable<Addresses> GetAddressesByClientId(int clientId)
        {
            var addresses = new List<Addresses>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(
                    "SELECT a.*, at.Type AS AddressType " +
                    "FROM Addresses a " +
                    "JOIN AddressTypes at ON a.AddressTypeId = at.AddressTypeId " +
                    "WHERE ClientId = @ClientId", connection);
                command.Parameters.AddWithValue("@ClientId", clientId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        addresses.Add(new Addresses
                        {
                            AddressId = (int)reader["AddressId"],
                            ClientId = (int)reader["ClientId"],
                            AddressTypeId = (int)reader["AddressTypeId"],
                            Address = reader["Address"].ToString(),
                            AddressType = new AddressType { AddressTypeId = (int)reader["AddressTypeId"], Type = reader["AddressType"].ToString() }
                        });
                    }
                }
            }
            return addresses;
        }

        public IEnumerable<Contacts> GetContactsByClientId(int clientId)
        {
            var contacts = new List<Contacts>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(
                    "SELECT c.*, ct.Type AS ContactType " +
                    "FROM Contacts c " +
                    "JOIN ContactTypes ct ON c.ContactTypeId = ct.ContactTypeId " +
                    "WHERE ClientId = @ClientId", connection);
                command.Parameters.AddWithValue("@ClientId", clientId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contacts.Add(new Contacts
                        {
                            ContactId = (int)reader["ContactId"],
                            ClientId = (int)reader["ClientId"],
                            ContactTypeId = (int)reader["ContactTypeId"],
                            Contact = reader["Contact"].ToString(),
                            ContactType = new ContactType { ContactTypeId = (int)reader["ContactTypeId"], Type = reader["ContactType"].ToString() }
                        });
                    }
                }
            }
            return contacts;
        }

        public void UpdateClient(Client client)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update client
                        var command = new SqlCommand("UPDATE Clients SET Name = @Name, GenderId = @GenderId, Details = @Details WHERE ClientId = @ClientId", connection, transaction);
                        command.Parameters.AddWithValue("@Name", client.Name);
                        command.Parameters.AddWithValue("@GenderId", client.GenderId);
                        command.Parameters.AddWithValue("@Details", client.Details);
                        command.Parameters.AddWithValue("@ClientId", client.ClientId);
                        command.ExecuteNonQuery();

                        // Delete existing addresses
                        var deleteAddressesCommand = new SqlCommand("DELETE FROM Addresses WHERE ClientId = @ClientId", connection, transaction);
                        deleteAddressesCommand.Parameters.AddWithValue("@ClientId", client.ClientId);
                        deleteAddressesCommand.ExecuteNonQuery();

                        // Insert new addresses
                        foreach (var address in client.Addresses)
                        {
                            AddAddress(address, client.ClientId, connection, transaction);
                        }

                        // Delete existing contacts
                        var deleteContactsCommand = new SqlCommand("DELETE FROM Contacts WHERE ClientId = @ClientId", connection, transaction);
                        deleteContactsCommand.Parameters.AddWithValue("@ClientId", client.ClientId);
                        deleteContactsCommand.ExecuteNonQuery();

                        // Insert new contacts
                        foreach (var contact in client.Contacts)
                        {
                            AddContact(contact, client.ClientId, connection, transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void AddClient(Client client)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert client
                        var command = new SqlCommand("INSERT INTO Clients (Name, GenderId, Details) VALUES (@Name, @GenderId, @Details); SELECT SCOPE_IDENTITY();", connection, transaction);
                        command.Parameters.AddWithValue("@Name", client.Name);
                        command.Parameters.AddWithValue("@GenderId", client.GenderId);
                        command.Parameters.AddWithValue("@Details", client.Details);
                        client.ClientId = Convert.ToInt32(command.ExecuteScalar());

                        // Insert addresses
                        foreach (var address in client.Addresses)
                        {
                            AddAddress(address, client.ClientId, connection, transaction);
                        }

                        // Insert contacts
                        foreach (var contact in client.Contacts)
                        {
                            AddContact(contact, client.ClientId, connection, transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void AddAddress(Addresses address, int clientId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool isConnectionProvided = connection != null;

            if (!isConnectionProvided)
            {
                connection = new SqlConnection(ConnectionString);
                connection.Open();
            }

            using (var command = new SqlCommand("INSERT INTO Addresses (ClientId, AddressTypeId, Address) VALUES (@ClientId, @AddressTypeId, @Address)", connection, transaction))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                command.Parameters.AddWithValue("@AddressTypeId", address.AddressTypeId);
                command.Parameters.AddWithValue("@Address", address.Address);
                command.ExecuteNonQuery();
            }

            if (!isConnectionProvided)
            {
                connection.Close();
            }
        }

        public void AddContact(Contacts contact, int clientId, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            bool isConnectionProvided = connection != null;

            if (!isConnectionProvided)
            {
                connection = new SqlConnection(ConnectionString);
                connection.Open();
            }

            using (var command = new SqlCommand("INSERT INTO Contacts (ClientId, ContactTypeId, Contact) VALUES (@ClientId, @ContactTypeId, @Contact)", connection, transaction))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                command.Parameters.AddWithValue("@ContactTypeId", contact.ContactTypeId);
                command.Parameters.AddWithValue("@Contact", contact.Contact);
                command.ExecuteNonQuery();
            }

            if (!isConnectionProvided)
            {
                connection.Close();
            }
        }

        public void DeleteClient(int id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var deleteContactsCommand = new SqlCommand("DELETE FROM Contacts WHERE ClientId = @ClientId", connection, transaction);
                        deleteContactsCommand.Parameters.AddWithValue("@ClientId", id);
                        deleteContactsCommand.ExecuteNonQuery();

                        var deleteAddressesCommand = new SqlCommand("DELETE FROM Addresses WHERE ClientId = @ClientId", connection, transaction);
                        deleteAddressesCommand.Parameters.AddWithValue("@ClientId", id);
                        deleteAddressesCommand.ExecuteNonQuery();

                        var deleteClientCommand = new SqlCommand("DELETE FROM Clients WHERE ClientId = @ClientId", connection, transaction);
                        deleteClientCommand.Parameters.AddWithValue("@ClientId", id);
                        deleteClientCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<Gender> GetGenders()
        {
            var genders = new List<Gender>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand("SELECT GenderId, Type FROM Genders", connection);
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