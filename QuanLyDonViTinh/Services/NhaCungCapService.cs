using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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

        // ================================================
        //  CHUẨN HÓA DỮ LIỆU
        // ================================================
        private void StandardizeInput(NhaCungCap ncc)
        {
            if (ncc == null) return;

            // Chuẩn hóa mã NCC để tránh trùng
            ncc.Ma_NCC = ncc.Ma_NCC?
                .Trim()                 // bỏ khoảng trắng đầu & cuối
                .Replace(" ", "")       // bỏ khoảng trắng thừa trong chuỗi
                .ToUpper();             // chuyển thành chữ hoa

            ncc.Ten_NCC = ncc.Ten_NCC?.Trim();
            ncc.Ghi_Chu = ncc.Ghi_Chu?.Trim();

            if (ncc.Ma_NCC?.Length > 50) ncc.Ma_NCC = ncc.Ma_NCC.Substring(0, 50);
            if (ncc.Ten_NCC?.Length > 200) ncc.Ten_NCC = ncc.Ten_NCC.Substring(0, 200);
            if (ncc.Ghi_Chu?.Length > 500) ncc.Ghi_Chu = ncc.Ghi_Chu.Substring(0, 500);
        }

        // ================================================
        //  GET LIST (READ)
        // ================================================
        public async Task<IEnumerable<NhaCungCap>> GetDanhSach()
        {
            string sql = @"
                SELECT Id, Ma_NCC, Ten_NCC, Ghi_Chu 
                FROM tbl_DM_NCC
            ";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<NhaCungCap>(sql);
            }
        }

        // ================================================
        //  ADD NEW (CREATE)
        // ================================================
        public async Task AddNhaCungCap(NhaCungCap nhaCungCap)
        {
            StandardizeInput(nhaCungCap);

            string sql = @"
                INSERT INTO tbl_DM_NCC (Ma_NCC, Ten_NCC, Ghi_Chu)
                VALUES (@Ma_NCC, @Ten_NCC, @Ghi_Chu)
            ";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, nhaCungCap);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Mã hoặc Tên nhà cung cấp này đã tồn tại.");

                throw;
            }
        }

        // ================================================
        //  UPDATE (SỬA)
        // ================================================
        public async Task UpdateNhaCungCap(NhaCungCap nhaCungCap)
        {
            StandardizeInput(nhaCungCap);

            string sql = @"
                UPDATE tbl_DM_NCC
                SET Ma_NCC = @Ma_NCC, 
                    Ten_NCC = @Ten_NCC, 
                    Ghi_Chu = @Ghi_Chu
                WHERE Id = @Id
            ";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, nhaCungCap);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Mã hoặc Tên nhà cung cấp này đã tồn tại.");

                throw;
            }
        }

        // ================================================
        //  GET BY ID
        // ================================================
        public async Task<NhaCungCap> GetNhaCungCapById(int id)
        {
            string sql = "SELECT Id, Ma_NCC, Ten_NCC, Ghi_Chu FROM tbl_DM_NCC WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<NhaCungCap>(sql, new { Id = id });
            }
        }

        // ================================================
        //  DELETE
        // ================================================
        public async Task DeleteNhaCungCap(int id)
        {
            string sql = "DELETE FROM tbl_DM_NCC WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                        throw new Exception("Không thể xóa nhà cung cấp này vì đang được sử dụng trong dữ liệu khác.");

                    throw;
                }
            }
        }
    }
}
