using ClientsManager.Data;
using ClientsManager.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace ClientsManager.Tests
{
    public class ClientDataAccessTests
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly Mock<IDataReader> _mockDataReader;
        private readonly ClientDataAccess _dataAccess;

        public ClientDataAccessTests()
        {
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockDataReader = new Mock<IDataReader>();
            _dataAccess = new ClientDataAccess("Data Source=OLIVIER_NSENGA;Initial Catalog=abc;Integrated Security=True");

            // Set up the connection to return the mock command
            _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
        }

        [Fact]
        public void GetClients_ReturnsListOfClients()
        {
            // Arrange
            _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockDataReader.Object);
            _mockDataReader.SetupSequence(reader => reader.Read())
                           .Returns(true).Returns(true).Returns(true)
                           .Returns(true).Returns(true).Returns(true)
                           .Returns(true).Returns(true).Returns(false);

            _mockDataReader.SetupSequence(reader => reader["ClientId"])
                           .Returns(1).Returns(2).Returns(3)
                           .Returns(4).Returns(6).Returns(7);

            _mockDataReader.SetupSequence(reader => reader["Name"])
                           .Returns("John Doe").Returns("Jane Smith").Returns("Jim Brown")
                           .Returns("Jill White").Returns("Olivier N'senga").Returns("Olivier N'senga");

            _mockDataReader.SetupSequence(reader => reader["GenderId"])
                           .Returns(1).Returns(2).Returns(1)
                           .Returns(2).Returns(1).Returns(2);

            _mockDataReader.SetupSequence(reader => reader["Details"])
                           .Returns("Details about John Doe").Returns("Details about Jane Smith").Returns("Details about Jim Brown")
                           .Returns("Details about Jill White").Returns("111111").Returns("wrwetwetwetwe");

            _mockCommand.Setup(cmd => cmd.Connection).Returns(_mockConnection.Object);
            _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);

            // Act
            var clients = _dataAccess.GetClients();

            // Assert
            var clientList = Assert.IsType<List<Client>>(clients);
            Assert.Equal(6, clientList.Count);

            Assert.Equal(1, clientList[0].ClientId);
            Assert.Equal("John Doe", clientList[0].Name);
            Assert.Equal(1, clientList[0].GenderId);
            Assert.Equal("Details about John Doe", clientList[0].Details);

            _mockCommand.Verify(cmd => cmd.ExecuteReader(), Times.Once);
        }

        [Fact]
        public void GetClientById_ReturnsClient()
        {
            // Arrange
            int clientId = 1;
            _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockDataReader.Object);
            _mockCommand.Setup(cmd => cmd.Parameters.Add(It.IsAny<IDataParameter>()));

            _mockDataReader.SetupSequence(reader => reader.Read())
                           .Returns(true)
                           .Returns(false);

            _mockDataReader.Setup(reader => reader["ClientId"]).Returns(clientId);
            _mockDataReader.Setup(reader => reader["Name"]).Returns("John Doe");
            _mockDataReader.Setup(reader => reader["GenderId"]).Returns(1);
            _mockDataReader.Setup(reader => reader["Details"]).Returns("Details about John Doe");

            _mockCommand.Setup(cmd => cmd.Connection).Returns(_mockConnection.Object);
            _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);

            // Act
            var client = _dataAccess.GetClientById(clientId);

            // Assert
            Assert.NotNull(client);
            Assert.Equal(clientId, client.ClientId);
            Assert.Equal("John Doe", client.Name);

            _mockCommand.Verify(cmd => cmd.ExecuteReader(), Times.Once);
        }

        [Fact]
        public void AddClient_AddsClientToDatabase()
        {
            // Arrange
            var client = new Client
            {
                Name = "New Client",
                GenderId = 1,
                Details = "New Details",
                Addresses = new List<Addresses>(),
                Contacts = new List<Contacts>()
            };

            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(11);
            _mockCommand.Setup(cmd => cmd.Parameters.Add(It.IsAny<IDataParameter>()));

            _mockCommand.Setup(cmd => cmd.Connection).Returns(_mockConnection.Object);
            _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);

            // Act
            _dataAccess.AddClient(client);

            // Assert
            Assert.Equal(11, client.ClientId);

            _mockCommand.Verify(cmd => cmd.ExecuteScalar(), Times.Once);
        }

        [Fact]
        public void UpdateClient_UpdatesClientInDatabase()
        {
            // Arrange
            var client = new Client
            {
                ClientId = 1,
                Name = "Updated Client",
                GenderId = 1,
                Details = "Updated Details",
                Addresses = new List<Addresses>(),
                Contacts = new List<Contacts>()
            };

            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
            _mockCommand.Setup(cmd => cmd.Parameters.Add(It.IsAny<IDataParameter>()));

            _mockCommand.Setup(cmd => cmd.Connection).Returns(_mockConnection.Object);
            _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);

            // Act
            _dataAccess.UpdateClient(client);

            // Assert
            _mockCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void DeleteClient_DeletesClientFromDatabase()
        {
            // Arrange
            int clientId = 1;
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
            _mockCommand.Setup(cmd => cmd.Parameters.Add(It.IsAny<IDataParameter>()));

            _mockCommand.Setup(cmd => cmd.Connection).Returns(_mockConnection.Object);
            _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);

            // Act
            _dataAccess.DeleteClient(clientId);

            // Assert
            _mockCommand.Verify(cmd => cmd.ExecuteNonQuery(), Times.Once);
        }
    }



}
