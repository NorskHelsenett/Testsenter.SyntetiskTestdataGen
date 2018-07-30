using System;
using System.Data.SqlClient;

namespace SyntetiskTestdataGen.Shared.Utils
{
    public static class ReaderExtensions
    {

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ?
                        (DateTime?)null :
                        (DateTime?)reader.GetDateTime(col);
        }

        public static bool GetBooleanTrueFalse(this SqlDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return !reader.IsDBNull(col) && reader.GetBoolean(col);
        }

        public static int? GetNullableInt(this SqlDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ?
                        (int?)null :
                        (int?)reader.GetInt32(col);
        }

        public static decimal? GetNullableDecimal(this SqlDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ?
                        (decimal?)null :
                        (decimal?)reader.GetDecimal(col);
        }
    }
}
