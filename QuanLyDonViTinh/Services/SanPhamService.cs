using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

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
            // === SỬA === (Dùng Ma_San_Pham thay vì Ma_SP)
            sp.Ma_San_Pham = sp.Ma_San_Pham?.Trim().ToUpper(); // Thêm ToUpper() cho Mã
            sp.Ten_San_Pham = sp.Ten_San_Pham?.Trim();
            sp.Ghi_Chu = sp.Ghi_Chu?.Trim();

            // === SỬA === (Độ dài 200)
            if (sp.Ten_San_Pham != null && sp.Ten_San_Pham.Length > 200)
                sp.Ten_San_Pham = sp.Ten_San_Pham.Substring(0, 200);

            // === SỬA === (Dùng Ma_San_Pham)
            if (sp.Ma_San_Pham != null && sp.Ma_San_Pham.Length > 50)
                sp.Ma_San_Pham = sp.Ma_San_Pham.Substring(0, 50);
        }

        /* === SỬA TOÀN BỘ HÀM NÀY === */
        public async Task<IEnumerable<SanPham>> GetDanhSach()
        {
            string sql = @"
                SELECT 
                    -- Sửa: Lấy "Id" và "Ma_San_Pham" (thay vì Ma_San_Pham và Ma_SP)
                    sp.Id, sp.Ma_San_Pham, sp.Ten_San_Pham, sp.Loai_San_Pham_ID, sp.Don_Vi_Tinh_ID, sp.Ghi_Chu,
                    lsp.Ten_LSP AS Ten_Loai_San_Pham,
                    dvt.Ten_Don_Vi_Tinh AS Ten_Don_Vi_Tinh
                FROM tbl_DM_San_Pham sp


                --Sửa: JOIN vào "Id"(PK) của bảng LoaiSanPham, không phải "Ma_LSP"(Business Key)
                LEFT JOIN tbl_DM_Loai_San_Pham lsp ON sp.Loai_San_Pham_ID = lsp.Id

                -- Sửa: JOIN vào "Id"(PK) của bảng DonViTinh, không phải "Ma_Don_Vi_Tinh"(Business Key)
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Id

                -- Sửa: Sắp xếp theo Id(PK) để cái mới nhất lên đầu
                ORDER BY sp.Id DESC";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<SanPham>(sql);
            }
        }

        /* === SỬA TOÀN BỘ HÀM NÀY === */
        public async Task AddSanPham(SanPham sanPham)
        {
            if (sanPham == null) throw new ArgumentNullException(nameof(sanPham));
            StandardizeInput(sanPham);

            // Sửa: INSERT vào "Ma_San_Pham" (không phải "Ma_SP")
            string sql = @"INSERT INTO tbl_DM_San_Pham (Ma_San_Pham, Ten_San_Pham, Loai_San_Pham_ID, Don_Vi_Tinh_ID, Ghi_Chu) 
                           VALUES (@Ma_San_Pham, @Ten_San_Pham, @Loai_San_Pham_ID, @Don_Vi_Tinh_ID, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Dapper sẽ tự map các thuộc tính: @Ma_San_Pham, @Ten_San_Pham... từ object
                    await connection.ExecuteAsync(sql, sanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    // Sửa: Lấy đúng tên thuộc tính
                    throw new Exception($"Mã sản phẩm '{sanPham.Ma_San_Pham}' đã tồn tại.");
                else throw;
            }
        }

        /* === SỬA TOÀN BỘ HÀM NÀY === */
        public async Task UpdateSanPham(SanPham sanPham)
        {
            if (sanPham == null) throw new ArgumentNullException(nameof(sanPham));
            StandardizeInput(sanPham);

            string sql = @"UPDATE tbl_DM_San_Pham SET 
                                Ma_San_Pham = @Ma_San_Pham,
                                Ten_San_Pham = @Ten_San_Pham, 
                                Loai_San_Pham_ID = @Loai_San_Pham_ID, 
                                Don_Vi_Tinh_ID = @Don_Vi_Tinh_ID, 
                                Ghi_Chu = @Ghi_Chu 
                           -- Sửa: WHERE bằng Khóa Chính "Id" (không phải Ma_San_Pham)
                           WHERE Id = @Id";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Dapper sẽ map @Ma_San_Pham, @Ten_San_Pham, ... và @Id từ object
                    await connection.ExecuteAsync(sql, sanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    // Sửa: Lấy đúng tên thuộc tính
                    throw new Exception($"Mã sản phẩm '{sanPham.Ma_San_Pham}' đã tồn tại.");
                else throw;
            }
        }

        /* === SỬA HÀM NÀY === */
        public async Task DeleteSanPham(int id)
        {
            // Sửa: WHERE bằng "Id" (PK)
            string sql = "DELETE FROM tbl_DM_San_Pham WHERE Id = @Id";
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