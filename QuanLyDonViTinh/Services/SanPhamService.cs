using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
// using Microsoft.AspNetCore.DataProtection.KeyManagement; // Dòng này có vẻ không cần thiết, bạn có thể xóa

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
            // === SỬA LỖI CÚ PHÁP: Các dấu "" và đóng chuỗi SQL ===
            string sql = @"
                SELECT 
                    -- Sửa: Lấy ""Id"" và ""Ma_San_Pham"" (thay vì Ma_San_Pham và Ma_SP)
                    sp.Id, sp.Ma_San_Pham, sp.Ten_San_Pham, sp.Loai_San_Pham_ID, sp.Don_Vi_Tinh_ID, sp.Ghi_Chu,
                    lsp.Ten_LSP AS Ten_Loai_San_Pham,
                    dvt.Ten_Don_Vi_Tinh AS Ten_Don_Vi_Tinh
                FROM tbl_DM_San_Pham sp

                --Sửa: JOIN vào ""Id""(PK) của bảng LoaiSanPham, không phải ""Ma_LSP""(Business Key)
                LEFT JOIN tbl_DM_Loai_San_Pham lsp ON sp.Loai_San_Pham_ID = lsp.Id

                -- Sửa: JOIN vào ""Id""(PK) của bảng DonViTinh, không phải ""Ma_Don_Vi_Tinh""(Business Key)
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Id
                
                -- Sửa: Sắp xếp theo Id(PK) để cái mới nhất lên đầu
                ORDER BY sp.Id DESC"; // <-- Lỗi 1: Chuỗi SQL phải được đóng ở đây

            // Lỗi 2: Khối using này đã bị dán vào BÊN TRONG chuỗi sql ở code của bạn
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

            string sql = @"INSERT INTO tbl_DM_San_Pham (Ma_San_Pham, Ten_San_Pham, Loai_San_Pham_ID, Don_Vi_Tinh_ID, Ghi_Chu) 
                   VALUES (@Ma_San_Pham, @Ten_San_Pham, @Loai_San_Pham_ID, @Don_Vi_Tinh_ID, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, sanPham);
                }
            }
            catch (SqlException ex)
            {
                // Lỗi trùng Mã sản phẩm (Duplicate Key)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception($"Mã sản phẩm '{sanPham.Ma_San_Pham}' đã tồn tại.");
                }
                // === BỔ SUNG: Bắt lỗi Khóa ngoại (Foreign Key - 547) ===
                else if (ex.Number == 547)
                {
                    // Kiểm tra thông báo lỗi để biết chính xác là do Đơn vị tính
                    if (ex.Message.Contains("FK_tbl_DM_San_Pham_tbl_DM_Don_Vi_Tinh"))
                    {
                        throw new Exception("Đơn vị tính bạn chọn không còn tồn tại . Vui lòng tải lại trang!");
                    }
                    // Phòng hờ lỗi do Loại sản phẩm bị xóa
                    else if (ex.Message.Contains("FK_tbl_DM_San_Pham_tbl_DM_Loai_San_Pham"))
                    {
                        throw new Exception("Loại sản phẩm bạn chọn không còn tồn tại. Vui lòng tải lại trang!");
                    }
                    else
                    {
                        throw new Exception("Dữ liệu liên quan không tồn tại. Chi tiết: " + ex.Message);
                    }
                }
                else
                {
                    throw; // Các lỗi khác ném ra bình thường
                }
            }
        }

        /* === SỬA TOÀN BỘ HÀM NÀY === */
        public async Task UpdateSanPham(SanPham sanPham)
        {
            if (sanPham == null) throw new ArgumentNullException(nameof(sanPham));
            StandardizeInput(sanPham);

            // === SỬA LỖI CÚ PHÁP: Dấu "" trong chú thích ===
            string sql = @"UPDATE tbl_DM_San_Pham SET 
                                Ma_San_Pham = @Ma_San_Pham,
                                Ten_San_Pham = @Ten_San_Pham, 
                                Loai_San_Pham_ID = @Loai_San_Pham_ID, 
                                Don_Vi_Tinh_ID = @Don_Vi_Tinh_ID, 
                                Ghi_Chu = @Ghi_Chu 
                           -- Sửa: WHERE bằng Khóa Chính ""Id"" (không phải Ma_San_Pham)
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