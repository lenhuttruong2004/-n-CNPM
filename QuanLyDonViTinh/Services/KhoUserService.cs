using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration; // Thêm using

namespace QuanLyDonViTinh.Services
{
    public class KhoUserService
    {
        private readonly string _connectionString;

        public KhoUserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) - SỬA LẠI JOIN */
        public async Task<IEnumerable<KhoUser>> GetDanhSach()
        {
            string sql = @"
                SELECT 
                    -- === SỬA ===: Thêm "ku.Id"
                    ku.Id, ku.Ma_Dang_Nhap, ku.Kho_ID,
                    k.Ten_Kho-- Lấy tên kho từ bảng tbl_DM_Kho
                FROM tbl_DM_Kho_User ku
                -- === SỬA ===: JOIN vào "k.Id", không phải "k.Ma_Kho"
                --(Vì tbl_DM_Kho có khóa chính là "Id"
                LEFT JOIN tbl_DM_Kho k ON ku.Kho_ID = k.Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<KhoUser>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddKhoUser(KhoUser khoUser)
        {
            // Câu SQL này đã đúng
            string sql = @"
                INSERT INTO tbl_DM_Kho_User (Ma_Dang_Nhap, Kho_ID) 
                VALUES (@Ma_Dang_Nhap, @Kho_ID)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, khoUser);
                }
            }
            catch (SqlException ex)
            {
                // (Logic try-catch này BÂY GIỜ đã đúng, vì ta đã thêm UNIQUE constraint ở file SQL)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Phân quyền này đã tồn tại cho User và Kho được chọn.");
                }
                else { throw; }
            }
        }

        /* HÀM XÓA (DELETE) - SỬA LẠI HOÀN TOÀN */
        // Phải xóa bằng Khóa Chính "Id", không phải khóa tổng hợp (tưởng tượng)
        public async Task DeleteKhoUser(int id)
        {
            string sql = "DELETE FROM tbl_DM_Kho_User WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
    }
}