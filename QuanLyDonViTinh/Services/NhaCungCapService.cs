using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace QuanLyDonViTinh.Services
{
    public class NhaCungCapService
    {
        private readonly string _connectionString;

        public NhaCungCapService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<NhaCungCap>> GetDanhSach()
        {
            string sql = "SELECT * FROM tbl_DM_NCC";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<NhaCungCap>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddNhaCungCap(NhaCungCap nhaCungCap)
        {
            string sql = "INSERT INTO tbl_DM_NCC (Ten_NCC, Ghi_Chu) VALUES (@Ten_NCC, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, nhaCungCap);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Ràng buộc DUY NHẤT (UNIQUE)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên nhà cung cấp này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM CẬP NHẬT (UPDATE / SỬA) */
        public async Task UpdateNhaCungCap(NhaCungCap nhaCungCap)
        {
            string sql = "UPDATE tbl_DM_NCC SET Ten_NCC = @Ten_NCC, Ghi_Chu = @Ghi_Chu WHERE Ma_NCC = @Ma_NCC";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, nhaCungCap);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Ràng buộc DUY NHẤT (UNIQUE)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên nhà cung cấp này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }
        public async Task<NhaCungCap> GetNhaCungCapById(int id)
        {
            string sql = "SELECT * FROM tbl_DM_Nha_Cung_Cap WHERE Ma_NCC = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<NhaCungCap>(sql, new { Id = id });
            }
        }

        /* HÀM XÓA (DELETE) */
        public async Task DeleteNhaCungCap(int id)
        {
            string sql = "DELETE FROM tbl_DM_NCC WHERE Ma_NCC = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
    }
}