using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace QuanLyDonViTinh.Services
{
    public class KhoUserService
    {
        private readonly string _connectionString;

        public KhoUserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) - DÙNG JOIN ĐỂ LẤY TÊN KHO */
        public async Task<IEnumerable<KhoUser>> GetDanhSach()
        {
            // JOIN với tbl_DM_Kho để lấy tên kho hiển thị lên UI
            string sql = @"
                SELECT 
                    ku.Ma_Dang_Nhap, ku.Kho_ID,
                    k.Ten_Kho -- Lấy tên kho từ bảng tbl_DM_Kho
                FROM tbl_DM_Kho_User ku
                LEFT JOIN tbl_DM_Kho k ON ku.Kho_ID = k.Ma_Kho";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<KhoUser>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddKhoUser(KhoUser khoUser)
        {
            string sql = @"
                INSERT INTO tbl_DM_Kho_User (Ma_Dang_Nhap, Kho_ID) 
                VALUES (@Ma_Dang_Nhap, @Kho_ID)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, khoUser);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Khóa tổng hợp (Composite Key) DUY NHẤT
                // Mã lỗi 2627 hoặc 2601 xảy ra khi Ma_Dang_Nhap và Kho_ID đã tồn tại
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Phân quyền này đã tồn tại cho User và Kho được chọn.");
                }
                else { throw; }
            }
        }

        /* HÀM XÓA (DELETE) */
        // Cần truyền cả 2 thành phần của khóa chính: Ma_Dang_Nhap và Kho_ID
        public async Task DeleteKhoUser(string maDangNhap, int khoId)
        {
            string sql = "DELETE FROM tbl_DM_Kho_User WHERE Ma_Dang_Nhap = @Ma_Dang_Nhap AND Kho_ID = @Kho_ID";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Ma_Dang_Nhap = maDangNhap, Kho_ID = khoId });
            }
        }

        // Lưu ý: Chúng ta không có hàm Update vì việc Update một khóa tổng hợp
        // thường là Delete (Xóa) và Add (Thêm) lại cặp mới.
    }
}