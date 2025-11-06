using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace QuanLyDonViTinh.Services
{
    public class LoaiSanPhamService
    {
        private readonly string _connectionString;

        public LoaiSanPhamService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<LoaiSanPham>> GetDanhSach()
        {
            string sql = "SELECT * FROM tbl_DM_Loai_San_Pham";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<LoaiSanPham>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            string sql = "INSERT INTO tbl_DM_Loai_San_Pham (Ten_LSP, Ghi_Chu) VALUES (@Ten_LSP, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Ràng buộc DUY NHẤT (UNIQUE)
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
            string sql = "UPDATE tbl_DM_Loai_San_Pham SET Ten_LSP = @Ten_LSP, Ghi_Chu = @Ghi_Chu WHERE Ma_LSP = @Ma_LSP";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Ràng buộc DUY NHẤT (UNIQUE)
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
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
    }
}