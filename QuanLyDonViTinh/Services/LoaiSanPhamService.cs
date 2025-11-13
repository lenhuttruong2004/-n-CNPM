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

        /* HÀM HỖ TRỢ: Chuẩn hóa dữ liệu */
        private void StandardizeInput(LoaiSanPham loaiSanPham)
        {
            if (loaiSanPham == null) return;
            loaiSanPham.Ma_Loai_SP = loaiSanPham.Ma_Loai_SP?.Trim().ToUpper();
            loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP?.Trim();
            loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu?.Trim();
            if (loaiSanPham.Ma_Loai_SP != null && loaiSanPham.Ma_Loai_SP.Length > 20)
            {
                loaiSanPham.Ma_Loai_SP = loaiSanPham.Ma_Loai_SP.Substring(0, 20);
            }
            if (loaiSanPham.Ten_LSP != null && loaiSanPham.Ten_LSP.Length > 50)
            {
                loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP.Substring(0, 50);
            }
        }

        // --- ĐÃ BỔ SUNG LẠI HÀM NÀY ---
        public async Task<IEnumerable<LoaiSanPham>> GetDanhSach()
        {
            // Sửa SQL: Thêm Ma_Loai_SP
            string sql = "SELECT Ma_LSP, Ma_Loai_SP, Ten_LSP, Ghi_Chu FROM tbl_DM_Loai_San_Pham";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<LoaiSanPham>(sql);
            }
        }
        // ------------------------------

        public async Task<LoaiSanPham> GetLoaiSanPhamById(int id)
        {
            string sql = "SELECT * FROM tbl_DM_Loai_San_Pham WHERE Ma_LSP = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<LoaiSanPham>(sql, new { Id = id });
            }
        }

        public async Task AddLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham);
            // Sửa SQL: Thêm Ma_Loai_SP
            string sql = "INSERT INTO tbl_DM_Loai_San_Pham (Ma_Loai_SP, Ten_LSP, Ghi_Chu) VALUES (@Ma_Loai_SP, @Ten_LSP, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                // Sửa thông báo lỗi
                if (ex.Number == 2627 || ex.Number == 2601) throw new Exception("Lỗi: Mã hoặc Tên loại sản phẩm này đã tồn tại.");
                else throw;
            }
        }

        public async Task UpdateLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham);
            // Sửa SQL: Thêm Ma_Loai_SP
            string sql = "UPDATE tbl_DM_Loai_San_Pham SET Ma_Loai_SP = @Ma_Loai_SP, Ten_LSP = @Ten_LSP, Ghi_Chu = @Ghi_Chu WHERE Ma_LSP = @Ma_LSP";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                // Sửa thông báo lỗi
                if (ex.Number == 2627 || ex.Number == 2601) throw new Exception("Lỗi: Mã hoặc Tên loại sản phẩm này đã tồn tại.");
                else throw;
            }
        }

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
                    if (ex.Number == 547) throw new Exception("Không thể xóa Loại sản phẩm này vì nó đang được sử dụng.");
                    else throw;
                }
            }
        }
    }
}