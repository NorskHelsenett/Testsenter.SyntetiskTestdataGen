using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Mapper;

namespace SyntetiskTestdataGen.Shared.DataProviders
{
    public class DatabasePregDataProvider : IPregDataProvider
    {
        private readonly string _connectionString;
        Queue<Person> _dataList;
        private int _totalNumberOfPersonAddresses;
        private int _offsetBatchSize = 1000000;
        private int _offset;


        public DatabasePregDataProvider(string connectionString)
        {
            _connectionString = connectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                CreatePersonAddressTable(conn);
                SetTotalNumberOfPersonAddresses(conn);
            }
        }

        private void CreatePersonAddressTable(SqlConnection conn)
        {

            using (var command = new SqlCommand("", conn))
            {
                command.CommandTimeout = 0;
                command.CommandText = $@"
                    IF (EXISTS (SELECT * 
                     FROM INFORMATION_SCHEMA.TABLES 
                     WHERE TABLE_SCHEMA = 'dbo' 
                     AND  TABLE_NAME = '{PregQueries.PersonAddressTable}'))
                    BEGIN
                        DROP TABLE {PregQueries.PersonAddressTable}
                    END
                    ";

                command.ExecuteNonQuery();
                command.CommandText = PregQueries.CreatePersonAddressQuery;
                command.ExecuteNonQuery();

            }
        }


        private void GetNextPersonPage(SqlConnection conn)
        {
            _dataList = new Queue<Person>();

        string query = PregQueries.GetNextPersonPageQuery(_offset,_offsetBatchSize);

            Console.WriteLine($"Getting next batch"); 
            Console.WriteLine($"TotalNumber of PersonsAdress {_totalNumberOfPersonAddresses}");
            Console.WriteLine($"Offset {_offset}");
            Console.WriteLine($"Offset interval {_offsetBatchSize}");

            SqlCommand command = new SqlCommand(query, conn);
            command.CommandTimeout = 0;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Person currentPerson = null;
                var addresses = new List<Address>();

                while (reader.Read())
                {
                    try
                    {
                        var nin = reader["nin"]?.ToString();

                        if (currentPerson != null && nin != currentPerson.NIN)
                        {
                            currentPerson.Addresses = addresses.ToArray();
                            _dataList.Enqueue(currentPerson);
                            currentPerson = null;
                            addresses = new List<Address>();
                        }
                        if (currentPerson == null)
                            currentPerson = reader.MapToPerson();

                        var address = reader.MapToAddress();
                        addresses.Add(address);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error reading DB " + e);
                        throw;
                    }
                }
                reader.Dispose();
                Console.WriteLine($"Loaded persons to process {_dataList.Count}");
            }
            
        }

        private void SetTotalNumberOfPersonAddresses(SqlConnection conn)
        {
            SqlCommand command = new SqlCommand(PregQueries.GetAllPersonsQuery, conn) {CommandTimeout = 0};
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    _totalNumberOfPersonAddresses = reader.GetInt32(0);

                if (_totalNumberOfPersonAddresses <= 0)
                    throw new Exception($"There are no persons in {PregQueries.PersonTable}");
            }
        }

        public async Task<Person> GetNextPerson()
        {
            if(_dataList == null)
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    GetNextPersonPage(conn);
                }

            return _dataList.Dequeue();

        }

        public bool HasMore()
        {

            if (_dataList.Count > 0)
                return true;

            if (_offset + _offsetBatchSize >= _totalNumberOfPersonAddresses)
                return false;

            GetNextBach();

            return true;
        }

        private void GetNextBach()
        {
            _offset = _offset + _offsetBatchSize;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                GetNextPersonPage(conn);
            }
        }
    }
}


