using System.Data.SqlClient;
using WebApplication2.Models;

namespace WebApplication2.Repositories
{
    public interface ITripRepository
    {
        Task<IEnumerable<TripDTO>> GetTripsAsync();
    }

    public class TripRepository : ITripRepository
    {
        private readonly string _connectionString;

        public TripRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<TripDTO>> GetTripsAsync()
        {
            var query = @"SELECT Trip.IdTrip, Name, Description, DateFrom, DateTo, MaxPeople,
                (SELECT STRING_AGG(Name, ', ') FROM Country INNER JOIN Country_Trip ON Country.IdCountry = Country_Trip.IdCountry WHERE Country_Trip.IdTrip = Trip.IdTrip) AS Countries
                FROM Trip";

            var trips = new List<TripDTO>();

            await using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(query, connection);
                await connection.OpenAsync();
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    var idOrdinal = reader.GetOrdinal("IdTrip");
                    var nameOrdinal = reader.GetOrdinal("Name");
                    var descOrdinal = reader.GetOrdinal("Description");
                    var dateFromOrdinal = reader.GetOrdinal("DateFrom");
                    var dateToOrdinal = reader.GetOrdinal("DateTo");
                    var maxPeopleOrdinal = reader.GetOrdinal("MaxPeople");
                    var countriesOrdinal = reader.GetOrdinal("Countries");

                    while (await reader.ReadAsync())
                    {
                        trips.Add(new TripDTO
                        {
                            IdTrip = reader.GetInt32(idOrdinal),
                            Name = reader.GetString(nameOrdinal),
                            Description = reader.GetString(descOrdinal),
                            DateFrom = reader.GetDateTime(dateFromOrdinal),
                            DateTo = reader.GetDateTime(dateToOrdinal),
                            MaxPeople = reader.GetInt32(maxPeopleOrdinal),
                            Countries = reader.IsDBNull(countriesOrdinal) ? "" : reader.GetString(countriesOrdinal)
                        });
                    }
                }
            }

            return trips;
        }
    }
}
