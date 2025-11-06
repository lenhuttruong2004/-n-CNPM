using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration; // Đảm bảo có dòng này

namespace QuanLyDonViTinh.Services
{
    public class KhoService
    {
        private readonly string _connectionString;

        public KhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<Kho>> GetDanhSach()
        {
            string sql = "SELECT * FROM tbl_DM_Kho";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Kho>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddKho(Kho kho)
        {
            string sql = "INSERT INTO tbl_DM_Kho (Ten_Kho, Ghi_Chu) VALUES (@Ten_Kho, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, kho);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Ràng buộc DUY NHẤT (UNIQUE)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên kho này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM CẬP NHẬT (UPDATE / SỬA) */
        public async Task UpdateKho(Kho kho)
        {
            string sql = "UPDATE tbl_DM_Kho SET Ten_Kho = @Ten_Kho, Ghi_Chu = @Ghi_Chu WHERE Ma_Kho = @Ma_Kho";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, kho);
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi Ràng buộc DUY NHẤT (UNIQUE)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên kho này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM XÓA (DELETE) */
        /* HÀM XÓA (DELETE) - ĐÃ CẬP NHẬT TRY...CATCH */
        public async Task DeleteKho(int id)
        {
            string sql = "DELETE FROM tbl_DM_Kho WHERE Ma_Kho = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    // Mã lỗi 547 là lỗi vi phạm Ràng buộc Khóa ngoại (REFERENCE constraint)
                    if (ex.Number == 547)
                    {
                        // Ném ra một lỗi mới với thông báo dễ hiểu
                        throw new Exception("Không thể xóa kho này vì nó đang được sử dụng ở Phân quyền User hoặc Phiếu Nhập/Xuất.");
                    }
                    else
                    {
                        // Ném ra lỗi SQL gốc nếu là lỗi khác
                        throw;
                    }
                }
            }
        }
    }
}