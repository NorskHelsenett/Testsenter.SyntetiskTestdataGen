using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Utils;

namespace SyntetiskTestdataGen.Build.Pushers
{
    public class SqlPregPusher : QueuePregPusher
    {
        public int SqlBatchSize { get; internal set; }
        public int TempSetBatchSize { get; internal set; }
        private readonly int _numberOfConsumers;
        private readonly Func<SqlConnection> _createConnection;

        public SqlPregPusher(string connectionString, int numberOfConsumers, int sqlBatchSize, int tempSetBatchSize)
        {
            SqlBatchSize = sqlBatchSize;
            TempSetBatchSize = tempSetBatchSize;
            _numberOfConsumers = numberOfConsumers;
            _createConnection = () => new SqlConnection(connectionString);
        }
        public override Task Save()
        {
            var tasks = new Task[_numberOfConsumers];
            for (int i = 0; i < _numberOfConsumers; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var sqlConnection = _createConnection();
                    sqlConnection.Open();
                    var thisSet = new List<Tuple<PersonPreg, AddressPreg[]>>();

                    try
                    {
                        while (!_queue.IsCompleted)
                        {
                            Person item;
                            if (!_queue.TryTake(out item, TimeSpan.FromMilliseconds(10)))
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(20));
                            }
                            else
                            {
                                Consumed();

                                thisSet.Add(new Tuple<PersonPreg, AddressPreg[]>(new PersonPreg(item), item.Addresses.Select(x => new AddressPreg(x)).ToArray()));
                                if (thisSet.Count > TempSetBatchSize)
                                {
                                    var stopwatch = new System.Diagnostics.Stopwatch();
                                    stopwatch.Start();

                                    await TryPush(thisSet, sqlConnection, SqlBatchSize);

                                    stopwatch.Stop();
                                    Outputter.WriteLine("Pushing took " + stopwatch.Elapsed.TotalSeconds + " seconds");

                                    thisSet.Clear();
                                }
                            }
                        }

                        if(thisSet.Any())
                            await TryPush(thisSet, sqlConnection, SqlBatchSize);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        UnexceptedQuit = true;
                    }
                });
            }

            var completeTask = Task.WhenAll(tasks);
            return completeTask;
        }

        private async Task TryPush(List<Tuple<PersonPreg, AddressPreg[]>> tempQueue, SqlConnection sqlConnection, int sqlBatchSize)
        {
            int tries = 10;
            SqlException e = null;
            while (tries-- > 0)
            {
                try
                {
                    await Push(tempQueue, sqlConnection, sqlBatchSize);
                    return;
                }
                catch (SqlException ee)
                {
                    e = ee;
                    await Task.Delay(10000);
                }
            }

            Outputter.WriteLine("After 10 tries, could not push. Trying to reset connection and try again");
            try
            {
                sqlConnection.Close();
            }
            catch (Exception f)
            {
                Outputter.WriteLine("While closing connection, got exception: " + f.Message);
            }

            tries = 20;
            while (tries-- > 0)
            {
                try
                {
                    sqlConnection = _createConnection();
                    await TryPush(tempQueue, sqlConnection, sqlBatchSize);
                    return;
                }
                catch (Exception f)
                {
                    Outputter.WriteLine("While creating new connection, got exception: " + f.Message + ". Trying again in 10 seconds. Remaining tries is " + tries);
                    await Task.Delay(10000);
                }
            }

            throw e ?? new Exception("Could not write to sql after 10 tries");
        }

        private static async Task Push(List<Tuple<PersonPreg, AddressPreg[]>> tempQueue, SqlConnection sqlConnection, int sqlBatchSize)
        {
            if (!tempQueue.Any())
                return;

            var persons = tempQueue.Select(x => x.Item1).ToList();
            var addresses = tempQueue.SelectMany(x => x.Item2).ToList();

            var dtPersons = new DataTable("PersonBatch");
            dtPersons = ConvertToDataTable(persons);

            var dtAddresses = new DataTable("AddressBatch");
            dtAddresses = ConvertToDataTable(addresses);

            using (SqlBulkCopy personBulkcopy = new SqlBulkCopy(sqlConnection))
            using (SqlBulkCopy addressBulkcopy = new SqlBulkCopy(sqlConnection))
            {
                personBulkcopy.BatchSize = sqlBatchSize;
                personBulkcopy.BulkCopyTimeout = 660;
                personBulkcopy.DestinationTableName = "Persons";

                addressBulkcopy.BatchSize = sqlBatchSize;
                addressBulkcopy.BulkCopyTimeout = 660;
                addressBulkcopy.DestinationTableName = "Addresses";

                try
                {
                    if (persons.Any())
                        await personBulkcopy.WriteToServerAsync(dtPersons);

                    if (addresses.Any())
                        await addressBulkcopy.WriteToServerAsync(dtAddresses);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                personBulkcopy.Close();
                addressBulkcopy.Close();
            }
        }

        public override void DisposeDb()
        {

        }

        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

    }
}
