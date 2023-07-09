using Auction.MVC.Models;
using Npgsql;
using System.ComponentModel;
using System.Text;

namespace Auction.MVC {
    public static class DbHelper {
        public static async Task<Trade> GetTradeById(long tradeId, string connectionString) {
            var con = new NpgsqlConnection(connectionString);
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"SELECT * FROM auction.tbtrade WHERE \"Id\"={tradeId}";
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            var result = new List<Trade>();
            while(await reader.ReadAsync()) {
                var trade = new Trade() {
                    Id = (long)reader["Id"],
                    ObjectId = (long)reader["ObjectId"],
                    Type = (TradeType)reader["Type"],
                    StartDate = (DateTime)reader["StartDate"],
                    AuctionDate = (DateTime)reader["AuctionDate"],
                    EndDate = (DateTime)reader["EndDate"],
                    Winner = reader["Winner"] == DBNull.Value ? null : (string?)reader["Winner"],
                    ParticipantIds = (long[])reader["ParticipantIds"],
                    Status = (TradeStatus)reader["Status"]
                };
                result.Add(trade);
            }
            if(result.Count > 1) {
                throw new InvalidOperationException();
            }
            con.Close();
            return result.First();
        }
        public static async Task<TradeObject> GetTradeObjectById(long tradeObjectId, string connectionString) {
            var con = new NpgsqlConnection(connectionString);
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"SELECT * FROM auction.tbtradeobject WHERE \"Id\"={tradeObjectId}";
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            var result = new List<TradeObject>();
            while(await reader.ReadAsync()) {
                var tradeObject = new TradeObject() {
                    Id = (long)reader["Id"],
                    Name = (string)reader["Name"],  
                    Description= (string)reader["Description"],
                    Price = (decimal)reader["Price"],
                    Status = (TradeObjectStatus)reader["Status"]
                };
                result.Add(tradeObject);
            }
            if(result.Count > 1) {
                throw new InvalidOperationException();
            }
            con.Close();
            return result.First();
        }

        public static void Update(string table, string connectionString, Dictionary<string, string> keyValue, string id) {
            var con = new NpgsqlConnection(connectionString);
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            var command = new StringBuilder();
            command.Append($"UPDATE {table} SET");
            var count = 0;
            foreach(var key in keyValue.Keys) {
                command.Append($"{key}={keyValue[key]}");
                if(count < keyValue.Count-1) {
                    command.Append(",");
                }
                count++;
            }
            command.Append($"WHERE \"Id\"={id}");
            cmd.CommandText = command.ToString();
            cmd.ExecuteNonQuery();
        }
    }
}
