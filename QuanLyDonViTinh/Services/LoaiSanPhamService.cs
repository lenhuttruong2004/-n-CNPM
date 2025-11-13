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

            // === SỬA === (Từ Ma_Loai_SP thành Ma_LSP)
            loaiSanPham.Ma_LSP = loaiSanPham.Ma_LSP?.Trim().ToUpper();
            loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP?.Trim();
            loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu?.Trim();

            // === SỬA === (Độ dài từ 20 -> 50)
            if (loaiSanPham.Ma_LSP != null && loaiSanPham.Ma_LSP.Length > 50)
            {
                loaiSanPham.Ma_LSP = loaiSanPham.Ma_LSP.Substring(0, 50);
            }
            // === SỬA === (Độ dài từ 50 -> 200)
            if (loaiSanPham.Ten_LSP != null && loaiSanPham.Ten_LSP.Length > 200)
            {
                loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP.Substring(0, 200);
            }
            // (Thêm cho Ghi_Chu)
            if (loaiSanPham.Ghi_Chu != null && loaiSanPham.Ghi_Chu.Length > 500)
            {
                loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu.Substring(0, 500);
            }
        }

        public async Task<IEnumerable<LoaiSanPham>> GetDanhSach()
        {
            // === SỬA === (Thêm "Id" và bỏ "Ma_Loai_SP")
            string sql = "SELECT Id, Ma_LSP, Ten_LSP, Ghi_Chu FROM tbl_DM_Loai_San_Pham";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<LoaiSanPham>(sql);
            }
        }

        public async Task<LoaiSanPham> GetLoaiSanPhamById(int id)
        {
            // === SỬA === (WHERE "Id" và SELECT đúng cột)
            string sql = "SELECT Id, Ma_LSP, Ten_LSP, Ghi_Chu FROM tbl_DM_Loai_San_Pham WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<LoaiSanPham>(sql, new { Id = id });
            }
        }

        public async Task AddLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham);
            // === SỬA === (INSERT vào "Ma_LSP", không phải "Ma_Loai_SP")
            string sql = "INSERT INTO tbl_DM_Loai_San_Pham (Ma_LSP, Ten_LSP, Ghi_Chu) VALUES (@Ma_LSP, @Ten_LSP, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Dapper sẽ tự map thuộc tính @Ma_LSP từ object loaiSanPham
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) throw new Exception("Lỗi: Mã hoặc Tên loại sản phẩm này đã tồn tại.");
                else throw;
            }
        }

        public async Task UpdateLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham);
            // === SỬA === (UPDATE "Ma_LSP" và WHERE "Id")
            string sql = "UPDATE tbl_DM_Loai_San_Pham SET Ma_LSP = @Ma_LSP, Ten_LSP = @Ten_LSP, Ghi_Chu = @Ghi_Chu WHERE Id = @Id";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Dapper sẽ map @Ma_LSP, @Ten_LSP, @Ghi_Chu, và @Id từ object
                    await connection.ExecuteAsync(sql, loaiSanPham);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) throw new Exception("Lỗi: Mã hoặc Tên loại sản phẩm này đã tồn tại.");
                else throw;
            }
        }

        public async Task DeleteLoaiSanPham(int id)
        {
            // === SỬA === (WHERE "Id")
            string sql = "DELETE FROM tbl_DM_Loai_San_Pham WHERE Id = @Id";
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