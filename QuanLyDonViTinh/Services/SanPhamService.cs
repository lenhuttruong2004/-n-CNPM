using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class SanPhamService
    {
        private readonly string _connectionString;

        public SanPhamService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM HỖ TRỢ: Chuẩn hóa dữ liệu */
        private void StandardizeInput(SanPham sp)
        {
            // Sửa: Trim() cho cả Ma_SP
            sp.Ma_SP = sp.Ma_SP?.Trim();
            sp.Ten_San_Pham = sp.Ten_San_Pham?.Trim();
            sp.Ghi_Chu = sp.Ghi_Chu?.Trim();

            // Cắt ngắn nếu quá dài
            if (sp.Ten_San_Pham != null && sp.Ten_San_Pham.Length > 100)
                sp.Ten_San_Pham = sp.Ten_San_Pham.Substring(0, 100);
            if (sp.Ma_SP != null && sp.Ma_SP.Length > 50)
                sp.Ma_SP = sp.Ma_SP.Substring(0, 50);
        }

        /* Sửa: Lấy thêm Ma_SP */
        public async Task<IEnumerable<SanPham>> GetDanhSach()
        {
            string sql = @"
                SELECT 
                    sp.Ma_San_Pham, sp.Ma_SP, sp.Ten_San_Pham, sp.Loai_San_Pham_ID, sp.Don_Vi_Tinh_ID, sp.Ghi_Chu,
                    lsp.Ten_LSP AS Ten_Loai_San_Pham,
                    dvt.Ten_Don_Vi_Tinh AS Ten_Don_Vi_Tinh
                FROM tbl_DM_San_Pham sp
                LEFT JOIN tbl_DM_Loai_San_Pham lsp ON sp.Loai_San_Pham_ID = lsp.Ma_LSP
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Ma_Don_Vi_Tinh
                ORDER BY sp.Ma_San_Pham DESC"; // Sắp xếp sản phẩm mới lên đầu
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<SanPham>(sql);
            }
        }

        /* Sửa: Thêm Ma_SP vào INSERT và bắt lỗi trùng */
        public async Task AddSanPham(SanPham sanPham)
        {
            if (sanPham == null) throw new ArgumentNullException(nameof(sanPham));
            StandardizeInput(sanPham);

            string sql = @"INSERT INTO tbl_DM_San_Pham (Ma_SP, Ten_San_Pham, Loai_San_Pham_ID, Don_Vi_Tinh_ID, Ghi_Chu) 
                           VALUES (@Ma_SP, @Ten_San_Pham, @Loai_San_Pham_ID, @Don_Vi_Tinh_ID, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, sanPham);
                }
            }
            catch (SqlException ex)
            {
                // Bắt lỗi trùng Mã SP (UNIQUE constraint)
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception($"Mã sản phẩm '{sanPham.Ma_SP}' đã tồn tại.");
                else throw;
            }
        }

        /* Sửa: Thêm Ma_SP vào UPDATE và bắt lỗi trùng */
        public async Task UpdateSanPham(SanPham sanPham)
        {
            if (sanPham == null) throw new ArgumentNullException(nameof(sanPham));
            StandardizeInput(sanPham);

            string sql = @"UPDATE tbl_DM_San_Pham SET 
                                Ma_SP = @Ma_SP,
                                Ten_San_Pham = @Ten_San_Pham, 
                                Loai_San_Pham_ID = @Loai_San_Pham_ID, 
                                Don_Vi_Tinh_ID = @Don_Vi_Tinh_ID, 
                                Ghi_Chu = @Ghi_Chu 
                           WHERE Ma_San_Pham = @Ma_San_Pham";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, sanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception($"Mã sản phẩm '{sanPham.Ma_SP}' đã tồn tại.");
                else throw;
            }
        }

        /* Sửa: Thêm thông báo lỗi rõ hơn */
        public async Task DeleteSanPham(int id)
        {
            string sql = "DELETE FROM tbl_DM_San_Pham WHERE Ma_San_Pham = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                        throw new Exception("Sản phẩm này đã phát sinh giao dịch (phiếu nhập/xuất), không thể xóa.");
                    else throw;
                }
            }
        }
    }
}