using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class NhaCungCapService
    {
        private readonly string _connectionString;

        public NhaCungCapService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // === HÀM HỖ TRỢ VALIDATE VÀ CHUẨN HÓA DỮ LIỆU ===
        private void StandardizeInput(NhaCungCap ncc)
        {
            if (ncc == null) return;

            // Trim khoảng trắng và chuyển Mã NCC về chữ in hoa
            ncc.Ma_NCC = ncc.Ma_NCC?.Trim().ToUpper();
            ncc.Ten_NCC = ncc.Ten_NCC?.Trim();
            ncc.Ghi_Chu = ncc.Ghi_Chu?.Trim();

            // Cắt chuỗi nếu quá dài (phòng vệ)
            if (ncc.Ma_NCC != null && ncc.Ma_NCC.Length > 50) ncc.Ma_NCC = ncc.Ma_NCC.Substring(0, 50);
            if (ncc.Ten_NCC != null && ncc.Ten_NCC.Length > 200) ncc.Ten_NCC = ncc.Ten_NCC.Substring(0, 200);
            if (ncc.Ghi_Chu != null && ncc.Ghi_Chu.Length > 500) ncc.Ghi_Chu = ncc.Ghi_Chu.Substring(0, 500);
        }

        // =============================
        // HÀM KIỂM TRA TRÙNG MÃ (CASE & TRIM-INSENSITIVE)
        // =============================
        private async Task<bool> MaNCC_DaTonTai(string maNCC, int id = 0)
        {
            // Sử dụng UPPER(LTRIM(RTRIM(...))) trong SQL để so sánh với Mã đã được chuẩn hóa
            string sql = @"
                SELECT COUNT(*)
                FROM tbl_DM_NCC
                WHERE UPPER(LTRIM(RTRIM(Ma_NCC))) = @MaNCC_Cleaned
                AND Id <> @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                // Truyền Mã NCC đã được chuẩn hóa vào tham số
                int count = await connection.ExecuteScalarAsync<int>(sql,
                    new { MaNCC_Cleaned = maNCC.Trim().ToUpper(), Id = id });
                return count > 0;
            }
        }

        /* HÀM LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<NhaCungCap>> GetDanhSach()
        {
            string sql = "SELECT Id, Ma_NCC, Ten_NCC, Ghi_Chu FROM tbl_DM_NCC";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<NhaCungCap>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddNhaCungCap(NhaCungCap nhaCungCap)
        {
            StandardizeInput(nhaCungCap); // Chuẩn hóa dữ liệu

            // KIỂM TRA TRÙNG TRƯỚC KHI THÊM
            if (await MaNCC_DaTonTai(nhaCungCap.Ma_NCC))
            {
                throw new Exception($"Mã nhà cung cấp '{nhaCungCap.Ma_NCC}' đã tồn tại.");
            }

            string sql = "INSERT INTO tbl_DM_NCC (Ma_NCC, Ten_NCC, Ghi_Chu) VALUES (@Ma_NCC, @Ten_NCC, @Ghi_Chu)";
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
                {
                    throw new Exception("Tên nhà cung cấp này đã tồn tại.");
                }
                else
                {
                    throw;
                }
            }
        }

        /* HÀM CẬP NHẬT (UPDATE) */
        public async Task UpdateNhaCungCap(NhaCungCap nhaCungCap)
        {
            StandardizeInput(nhaCungCap); // Chuẩn hóa dữ liệu

            // KIỂM TRA TRÙNG TRƯỚC KHI SỬA (Bỏ qua chính nó)
            if (await MaNCC_DaTonTai(nhaCungCap.Ma_NCC, nhaCungCap.Id))
            {
                throw new Exception($"Mã nhà cung cấp '{nhaCungCap.Ma_NCC}' đã tồn tại.");
            }

            string sql = "UPDATE tbl_DM_NCC SET Ma_NCC = @Ma_NCC, Ten_NCC = @Ten_NCC, Ghi_Chu = @Ghi_Chu WHERE Id = @Id";
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
                {
                    throw new Exception("Tên nhà cung cấp này đã tồn tại.");
                }
                else
                {
                    throw;
                }
            }
        }

        /* HÀM XÓA (DELETE) */
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
                    {
                        throw new Exception("Không thể xóa nhà cung cấp này vì đang được sử dụng.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}