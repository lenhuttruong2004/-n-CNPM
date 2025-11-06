using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace QuanLyDonViTinh.Services
{
    public class SanPhamService
    {
        private readonly string _connectionString;

        public SanPhamService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) - DÙNG JOIN ĐỂ LẤY TÊN */
        public async Task<IEnumerable<SanPham>> GetDanhSach()
        {
            // Dùng LEFT JOIN để lấy tên từ 2 bảng khóa ngoại
            string sql = @"
                SELECT 
                    sp.Ma_San_Pham, sp.Ten_San_Pham, sp.Loai_San_Pham_ID, sp.Don_Vi_Tinh_ID, sp.Ghi_Chu,
                    lsp.Ten_LSP AS Ten_Loai_San_Pham,  -- Đổi tên cột để map vào Model SanPham
                    dvt.Ten_Don_Vi_Tinh AS Ten_Don_Vi_Tinh -- Đổi tên cột
                FROM tbl_DM_San_Pham sp
                LEFT JOIN tbl_DM_Loai_San_Pham lsp ON sp.Loai_San_Pham_ID = lsp.Ma_LSP
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Ma_Don_Vi_Tinh;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                // Dapper tự động map các cột Ten_Loai_San_Pham, Ten_Don_Vi_Tinh vào Model
                return await connection.QueryAsync<SanPham>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddSanPham(SanPham sanPham)
        {
            string sql = @"
                INSERT INTO tbl_DM_San_Pham (Ten_San_Pham, Loai_San_Pham_ID, Don_Vi_Tinh_ID, Ghi_Chu) 
                VALUES (@Ten_San_Pham, @Loai_San_Pham_ID, @Don_Vi_Tinh_ID, @Ghi_Chu)";

            using (var connection = new SqlConnection(_connectionString))
            {
                // Không cần try/catch lỗi UNIQUE vì Ma_San_Pham là IDENTITY
                await connection.ExecuteAsync(sql, sanPham);
            }
        }

        /* HÀM CẬP NHẬT (UPDATE / SỬA) */
        public async Task UpdateSanPham(SanPham sanPham)
        {
            string sql = @"
                UPDATE tbl_DM_San_Pham SET 
                    Ten_San_Pham = @Ten_San_Pham, 
                    Loai_San_Pham_ID = @Loai_San_Pham_ID, 
                    Don_Vi_Tinh_ID = @Don_Vi_Tinh_ID, 
                    Ghi_Chu = @Ghi_Chu 
                WHERE Ma_San_Pham = @Ma_San_Pham";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, sanPham);
            }
        }

        /* HÀM XÓA (DELETE) */
        public async Task DeleteSanPham(int id)
        {
            string sql = "DELETE FROM tbl_DM_San_Pham WHERE Ma_San_Pham = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
    }
}