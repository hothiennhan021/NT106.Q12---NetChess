using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient; // Hoặc System.Data.SqlClient tùy phiên bản bạn dùng

namespace ChessData
{
    public class FriendRepository
    {
        private readonly string _connectionString;

        public FriendRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 1. Gửi lời mời kết bạn
        public string SendFriendRequest(int fromUserId, string toUsername)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Bước A: Tìm ID người nhận
                string findIdSql = "SELECT UserId FROM Users WHERE Username = @u";
                SqlCommand cmdFind = new SqlCommand(findIdSql, conn);
                cmdFind.Parameters.AddWithValue("@u", toUsername);
                object result = cmdFind.ExecuteScalar();

                if (result == null) return "NOT_FOUND"; // Không tìm thấy user này
                int toUserId = (int)result;

                if (toUserId == fromUserId) return "SELF_ERROR"; // Không thể tự kết bạn

                // Bước B: Kiểm tra xem đã kết bạn chưa
                string checkSql = "SELECT COUNT(*) FROM Friendships WHERE " +
                                  "((RequesterId = @f AND ReceiverId = @t) OR (RequesterId = @t AND ReceiverId = @f))";
                SqlCommand cmdCheck = new SqlCommand(checkSql, conn);
                cmdCheck.Parameters.AddWithValue("@f", fromUserId);
                cmdCheck.Parameters.AddWithValue("@t", toUserId);
                
                if ((int)cmdCheck.ExecuteScalar() > 0) return "EXISTED"; // Đã có lời mời hoặc đã là bạn

                // Bước C: Thêm lời mời vào DB
                string insertSql = "INSERT INTO Friendships (RequesterId, ReceiverId, Status) VALUES (@f, @t, 0)";
                SqlCommand cmdInsert = new SqlCommand(insertSql, conn);
                cmdInsert.Parameters.AddWithValue("@f", fromUserId);
                cmdInsert.Parameters.AddWithValue("@t", toUserId);
                cmdInsert.ExecuteNonQuery();

                return "SUCCESS";
            }
        }

        // 2. Lấy danh sách bạn bè
        public List<string> GetListFriends(int userId)
        {
            List<string> list = new List<string>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Lấy tên bạn bè và trạng thái Online
                string sql = @"
                    SELECT u.Username, u.Elo, u.IsOnline 
                    FROM Friendships f
                    JOIN Users u ON (u.UserId = f.RequesterId OR u.UserId = f.ReceiverId)
                    WHERE (f.RequesterId = @uid OR f.ReceiverId = @uid)
                      AND u.UserId != @uid  -- Lấy người kia, không lấy mình
                      AND f.Status = 1";    //Chỉ lấy người đã đồng ý (Status = 1)

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // Format trả về: "TenUser|Elo|IsOnline"
                    list.Add($"{reader["Username"]}|{reader["Elo"]}|{reader["IsOnline"]}");
                }
            }
            return list;
        }

        // 3. Lấy danh sách lời mời kết bạn (Status = 0 và mình là người nhận - Receiver)
        public List<string> GetFriendRequests(int userId)
        {
            List<string> list = new List<string>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT u.Username, f.Id as RequestId
                    FROM Friendships f
                    JOIN Users u ON u.UserId = f.RequesterId
                    WHERE f.ReceiverId = @uid AND f.Status = 0";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // Format: "RequestId|TenNguoiGui"
                    list.Add($"{reader["RequestId"]}|{reader["Username"]}");
                }
            }
            return list;
        }

        // 4. Đồng ý kết bạn
        public void AcceptFriend(int requestId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE Friendships SET Status = 1 WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", requestId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}