using WebApplication2.Models;
using System.Data.SqlClient;
using WebApplication2.Exceptions;
using System.Globalization;

namespace WebApplication2.Repositories
{
    public interface IClientRepository
    {
        Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId);
        Task<int> CreateClientAsync(ClientDTO client);
        Task RegisterClientToTripAsync(int clientId, int tripId);
        Task DeleteClientTripAsync(int clientId, int tripId);
    }

    public class ClientRepository : IClientRepository
    {
        private readonly string _connectionString;

        public ClientRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId)
        {
            var query = @"SELECT Trip.IdTrip, Name, Description, DateFrom, DateTo, MaxPeople,
                               Client_Trip.RegisteredAt, Client_Trip.PaymentDate,
                               (
                                   SELECT STRING_AGG(Country.Name, ', ')
                                   FROM Country
                                   INNER JOIN Country_Trip ON Country.IdCountry = Country_Trip.IdCountry
                                   WHERE Country_Trip.IdTrip = Trip.IdTrip
                               ) AS Countries
                        FROM Trip
                        INNER JOIN Client_Trip ON Trip.IdTrip = Client_Trip.IdTrip
                        WHERE Client_Trip.IdClient = @Id";
            var trips = new List<ClientTripDTO>();

            await using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", clientId);

                await connection.OpenAsync();
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return null;

                    var idOrdinal = reader.GetOrdinal("IdTrip");
                    var nameOrdinal = reader.GetOrdinal("Name");
                    var descOrdinal = reader.GetOrdinal("Description");
                    var dateFromOrdinal = reader.GetOrdinal("DateFrom");
                    var dateToOrdinal = reader.GetOrdinal("DateTo");
                    var maxPeopleOrdinal = reader.GetOrdinal("MaxPeople");
                    var regAtOrdinal = reader.GetOrdinal("RegisteredAt");
                    var paymentDateOrdinal = reader.GetOrdinal("PaymentDate");
                    var countriesOrdinal = reader.GetOrdinal("Countries");

                    while (await reader.ReadAsync())
                    {
                        trips.Add(new ClientTripDTO
                        {
                            IdTrip = reader.GetInt32(idOrdinal),
                            Name = reader.GetString(nameOrdinal),
                            Description = reader.GetString(descOrdinal),
                            DateFrom = reader.GetDateTime(dateFromOrdinal),
                            DateTo = reader.GetDateTime(dateToOrdinal),
                            MaxPeople = reader.GetInt32(maxPeopleOrdinal),
                            RegisteredAt = DateTime.ParseExact(reader.GetInt32(regAtOrdinal).ToString(), "yyyyMMdd", CultureInfo.InvariantCulture),
                            PaymentDate = reader.IsDBNull(paymentDateOrdinal)
                                ? (DateTime?)null
                                : DateTime.ParseExact(reader.GetInt32(paymentDateOrdinal).ToString(), "yyyyMMdd", CultureInfo.InvariantCulture),
                            Countries = reader.IsDBNull(countriesOrdinal)
                                ? string.Empty
                                : reader.GetString(countriesOrdinal)
                        });
                    }
                }
            }
            return trips;
        }

        public async Task<int> CreateClientAsync(ClientDTO client)
        {
            var query = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) 
                        OUTPUT INSERTED.IdClient VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
            int newId;

            await using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@FirstName", client.FirstName);
                command.Parameters.AddWithValue("@LastName", client.LastName);
                command.Parameters.AddWithValue("@Email", client.Email);
                command.Parameters.AddWithValue("@Telephone", client.Telephone);
                command.Parameters.AddWithValue("@Pesel", client.Pesel);

                await connection.OpenAsync();
                newId = (int)await command.ExecuteScalarAsync();
            }
            return newId;
        }

        public async Task RegisterClientToTripAsync(int clientId, int tripId)
        {
            var query1 = "SELECT COUNT(1) FROM Client WHERE IdClient = @Id";
            var query2 = "SELECT COUNT(1) FROM Trip WHERE IdTrip = @TripId";
            var query3 = @"SELECT MaxPeople - (SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId) FROM Trip WHERE IdTrip = @TripId";
            var query4 = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) VALUES (@Id, @TripId, @RegisteredAt)";
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkClient = new SqlCommand(query1, connection);
                checkClient.Parameters.AddWithValue("@Id", clientId);
                if ((int)await checkClient.ExecuteScalarAsync() == 0)
                    throw new NotFoundException("Client not found.");

                var checkTrip = new SqlCommand(query2, connection);
                checkTrip.Parameters.AddWithValue("@TripId", tripId);
                if ((int)await checkTrip.ExecuteScalarAsync() == 0)
                    throw new NotFoundException("Trip not found.");

                var checkLimit = new SqlCommand(query3, connection);
                checkLimit.Parameters.AddWithValue("@TripId", tripId);
                if ((int)await checkLimit.ExecuteScalarAsync() <= 0)
                    throw new BadRequestException("Trip is full.");

                var insert = new SqlCommand(query4, connection);
                insert.Parameters.AddWithValue("@Id", clientId);
                insert.Parameters.AddWithValue("@TripId", tripId);
                var currentDate = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
                insert.Parameters.AddWithValue("@RegisteredAt", currentDate);

                await insert.ExecuteNonQueryAsync();

            }
        }

        public async Task DeleteClientTripAsync(int clientId, int tripId)
        {
            var query1 = "SELECT COUNT(1) FROM Client_Trip WHERE IdClient = @Id AND IdTrip = @TripId";
            var query2 = "DELETE FROM Client_Trip WHERE IdClient = @Id AND IdTrip = @TripId";
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var check = new SqlCommand(query1, connection);
                check.Parameters.AddWithValue("@Id", clientId);
                check.Parameters.AddWithValue("@TripId", tripId);

                if ((int)await check.ExecuteScalarAsync() == 0)
                    throw new NotFoundException("Registration not found.");

                var delete = new SqlCommand(query2, connection);
                delete.Parameters.AddWithValue("@Id", clientId);
                delete.Parameters.AddWithValue("@TripId", tripId);

                await delete.ExecuteNonQueryAsync();
            }
        }
    }
}
