using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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

        private void StandardizeInput(LoaiSanPham loaiSanPham)
        {
            if (loaiSanPham == null) return;

            loaiSanPham.Ma_LSP = loaiSanPham.Ma_LSP?.Trim().ToUpper();
            loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP?.Trim();
            loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu?.Trim();

            if (loaiSanPham.Ma_LSP?.Length > 50) loaiSanPham.Ma_LSP = loaiSanPham.Ma_LSP.Substring(0, 50);
            if (loaiSanPham.Ten_LSP?.Length > 200) loaiSanPham.Ten_LSP = loaiSanPham.Ten_LSP.Substring(0, 200);
            if (loaiSanPham.Ghi_Chu?.Length > 500) loaiSanPham.Ghi_Chu = loaiSanPham.Ghi_Chu.Substring(0, 500);
        }

        public async Task<IEnumerable<LoaiSanPham>> GetDanhSach()
        {
            string sql = "SELECT Id, Ma_LSP, Ten_LSP, Ghi_Chu FROM tbl_DM_Loai_San_Pham";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<LoaiSanPham>(sql);
        }

        public async Task<LoaiSanPham> GetLoaiSanPhamById(int id)
        {
            string sql = "SELECT Id, Ma_LSP, Ten_LSP, Ghi_Chu FROM tbl_DM_Loai_San_Pham WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<LoaiSanPham>(sql, new { Id = id });
        }

        public async Task AddLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham);

            using var connection = new SqlConnection(_connectionString);
            string checkSql = "SELECT COUNT(*) FROM tbl_DM_Loai_San_Pham WHERE TRIM(Ma_LSP) = @Ma_LSP";
            int count = await connection.ExecuteScalarAsync<int>(checkSql, new { Ma_LSP = loaiSanPham.Ma_LSP });
            if (count > 0)
                throw new Exception("Lỗi: Mã loại sản phẩm này đã tồn tại.");

            string sql = "INSERT INTO tbl_DM_Loai_San_Pham (Ma_LSP, Ten_LSP, Ghi_Chu) VALUES (@Ma_LSP, @Ten_LSP, @Ghi_Chu)";
            try
            {
                await connection.ExecuteAsync(sql, loaiSanPham);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Lỗi: Mã hoặc Tên loại sản phẩm này đã tồn tại.");
                else throw;
            }
        }

        public async Task UpdateLoaiSanPham(LoaiSanPham loaiSanPham)
        {
            StandardizeInput(loaiSanPham);

            using var connection = new SqlConnection(_connectionString);
            string checkSql = "SELECT COUNT(*) FROM tbl_DM_Loai_San_Pham WHERE TRIM(Ma_LSP) = @Ma_LSP AND Id != @Id";
            int count = await connection.ExecuteScalarAsync<int>(checkSql, new { Ma_LSP = loaiSanPham.Ma_LSP, Id = loaiSanPham.Id });
            if (count > 0)
                throw new Exception("Lỗi: Mã loại sản phẩm này đã tồn tại.");

            string sql = "UPDATE tbl_DM_Loai_San_Pham SET Ma_LSP=@Ma_LSP, Ten_LSP=@Ten_LSP, Ghi_Chu=@Ghi_Chu WHERE Id=@Id";
            try
            {
                await connection.ExecuteAsync(sql, loaiSanPham);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Lỗi: Mã hoặc Tên loại sản phẩm này đã tồn tại.");
                else throw;
            }
        }

        public async Task DeleteLoaiSanPham(int id)
        {
            string sql = "DELETE FROM tbl_DM_Loai_San_Pham WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new Exception("Không thể xóa Loại sản phẩm này vì nó đang được sử dụng.");
                else throw;
            }
        }
    }
}
