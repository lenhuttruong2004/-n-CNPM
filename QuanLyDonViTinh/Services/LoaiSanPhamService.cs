using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class LoaiSanPhamService
    {
        private readonly string _connectionString;

        public LoaiSanPhamService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM HỖ TRỢ: Chuẩn hóa dữ liệu (Trim + Cắt ngắn nếu cần) */
        private void StandardizeInput(LoaiSanPham loaiSanPham)
        {
            if (loaiSanPham == null) return;

            // 1. Trim khoảng trắng trước
            loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP?.Trim();
            loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu?.Trim();

            // 2. Cắt ngắn nếu vẫn quá dài (An toàn tuyệt đối cho SQL)
            if (loaiSanPham.Ten_LSP != null && loaiSanPham.Ten_LSP.Length > 50)
            {
                // Nếu dài hơn 50 ký tự, chỉ lấy 50 ký tự đầu tiên
                loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP.Substring(0, 50);
            }

            // Tương tự cho Ghi chú nếu cần (ví dụ database cho 200 ký tự)
            if (loaiSanPham.Ghi_Chu != null && loaiSanPham.Ghi_Chu.Length > 200)
            {
                loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu.Substring(0, 200);
            }
        }

        /* HÀM LẤY 1 CÁI THEO ID (Thường cần cho chức năng Sửa nếu tải lại trang) */
        public async Task<LoaiSanPham> GetLoaiSanPhamById(int id)
        {
            string sql = "SELECT * FROM tbl_DM_Loai_San_Pham WHERE Ma_LSP = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<LoaiSanPham>(sql, new { Id = id });
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham); // <-- Gọi hàm chuẩn hóa

            string sql = "INSERT INTO tbl_DM_Loai_San_Pham (Ten_LSP, Ghi_Chu) VALUES (@Ten_Loai_San_Pham, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên loại sản phẩm này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM CẬP NHẬT (UPDATE / SỬA) */
        public async Task UpdateLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham); // <-- Gọi hàm chuẩn hóa

            string sql = "UPDATE tbl_DM_Loai_San_Pham SET Ten_LSP = @Ten_Loai_San_Pham, Ghi_Chu = @Ghi_Chu WHERE Ma_LSP = @Ma_Loai_San_Pham";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên loại sản phẩm này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM XÓA (DELETE) */
        public async Task DeleteLoaiSanPham(int id)
        {
            string sql = "DELETE FROM tbl_DM_Loai_San_Pham WHERE Ma_LSP = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    // Mã lỗi 547: Vi phạm ràng buộc khóa ngoại (đang được sử dụng ở bảng khác)
                    if (ex.Number == 547)
                    {
                        throw new Exception("Không thể xóa Loại sản phẩm này vì nó đang được sử dụng.");
                    }
                    else { throw; }
                }
            }
        }
    }
}