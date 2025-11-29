// File: AnalysisService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using ChessClient.Models;

namespace ChessUI.Services
{
    public class AnalysisService
    {
        private readonly HttpClient _httpClient;
        private const string CHESS_API_URL = "https://chess-api.com/v1";

        public AnalysisService()
        {
            _httpClient = new HttpClient();
        }

        // --- HÀM 1: Gọi API Lichess (Mới - Ưu tiên dùng) ---
        public async Task<ChessApiResponse> GetLichessAnalysisAsync(string fen)
        {
            string url = $"https://lichess.org/api/cloud-eval?fen={fen}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<LichessResponse>(json);

                    if (data != null && data.pvs != null && data.pvs.Count > 0)
                    {
                        var bestPv = data.pvs[0];
                        string bestMove = bestPv.moves.Split(' ')[0];
                        return new ChessApiResponse
                        {
                            text = "Lichess Cloud",
                            move = bestMove,
                            centipawns = bestPv.cp,
                            mate = bestPv.mate
                        };
                    }
                }
            }
            catch { }
            return null;
        }

        // --- HÀM 2: Gọi API Chess-api.com (Cũ - Dự phòng) ---
        // Lỗi CS1061 xảy ra vì bạn đang thiếu hàm này
        public async Task<ChessApiResponse> GetAnalysisAsync(string fen)
        {
            try
            {
                var requestData = new { fen = fen };
                string jsonBody = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(CHESS_API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<ChessApiResponse>(jsonResponse, options);
                }
            }
            catch { }
            return null;
        }

        // Các class hỗ trợ cho Lichess
        public class LichessResponse
        {
            public string fen { get; set; }
            public List<LichessPv> pvs { get; set; }
        }
        public class LichessPv
        {
            public string moves { get; set; }
            public int cp { get; set; }
            public int? mate { get; set; }
        }
    }
}