using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class KhoUserService
    {
        private readonly string _connectionString;

        public KhoUserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<KhoUser>> GetDanhSach()
        {
            // Code cũ của bạn logic đã đúng
            string sql = @"
                SELECT ku.Id, ku.Ma_Dang_Nhap, ku.Kho_ID, k.Ten_Kho
                FROM tbl_DM_Kho_User ku
                LEFT JOIN tbl_DM_Kho k ON ku.Kho_ID = k.Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<KhoUser>(sql);
            }
        }

        public async Task AddKhoUser(KhoUser khoUser)
        {
            string sql = @"INSERT INTO tbl_DM_Kho_User (Ma_Dang_Nhap, Kho_ID) VALUES (@Ma_Dang_Nhap, @Kho_ID)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, khoUser);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Phân quyền này đã tồn tại.");
                else throw;
            }
        }

        /* === SỬA HÀM NÀY: Thêm Try-Catch an toàn === */
        public async Task DeleteKhoUser(int id)
        {
            string sql = "DELETE FROM tbl_DM_Kho_User WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    // Phòng trường hợp user đang đăng nhập hoặc có liên kết hệ thống khác
                    if (ex.Number == 547)
                        throw new Exception("Không thể xóa phân quyền này vì đang được sử dụng/tham chiếu.");
                    else throw;
                }
            }
        }
    }
}