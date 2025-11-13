using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration; // Thêm using này

namespace QuanLyDonViTinh.Services
{
    public class NhaCungCapService
    {
        private readonly string _connectionString;

        public NhaCungCapService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // === THÊM HÀM HỖ TRỢ VALIDATE ===
        private void StandardizeInput(NhaCungCap ncc)
        {
            if (ncc == null) return;
            ncc.Ma_NCC = ncc.Ma_NCC?.Trim().ToUpper();
            ncc.Ten_NCC = ncc.Ten_NCC?.Trim();
            ncc.Ghi_Chu = ncc.Ghi_Chu?.Trim();

            // Cắt chuỗi nếu quá dài (phòng vệ)
            if (ncc.Ma_NCC?.Length > 50)
                ncc.Ma_NCC = ncc.Ma_NCC.Substring(0, 50);
            if (ncc.Ten_NCC?.Length > 200)
                ncc.Ten_NCC = ncc.Ten_NCC.Substring(0, 200);
            if (ncc.Ghi_Chu?.Length > 500)
                ncc.Ghi_Chu = ncc.Ghi_Chu.Substring(0, 500);
        }

        /* HÀM LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<NhaCungCap>> GetDanhSach()
        {
            // === SỬA === (Nên chỉ định rõ cột)
            string sql = "SELECT Id, Ma_NCC, Ten_NCC, Ghi_Chu FROM tbl_DM_NCC";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<NhaCungCap>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddNhaCungCap(NhaCungCap nhaCungCap)
        {
            StandardizeInput(nhaCungCap); // Chuẩn hóa dữ liệu
            // === SỬA === (Thêm "Ma_NCC" vào câu INSERT)
            string sql = "INSERT INTO tbl_DM_NCC (Ma_NCC, Ten_NCC, Ghi_Chu) VALUES (@Ma_NCC, @Ten_NCC, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, nhaCungCap);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    // Sửa thông báo lỗi
                    throw new Exception("Mã hoặc Tên nhà cung cấp này đã tồn tại.");
                }
                else { throw; }
            }
        }

        /* HÀM CẬP NHẬT (UPDATE / SỬA) */
        public async Task UpdateNhaCungCap(NhaCungCap nhaCungCap)
        {
            StandardizeInput(nhaCungCap); // Chuẩn hóa dữ liệu
            // === SỬA === (Thêm "Ma_NCC" vào SET và WHERE bằng "Id")
            string sql = "UPDATE tbl_DM_NCC SET Ma_NCC = @Ma_NCC, Ten_NCC = @Ten_NCC, Ghi_Chu = @Ghi_Chu WHERE Id = @Id";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, nhaCungCap);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Mã hoặc Tên nhà cung cấp này đã tồn tại.");
                }
                else { throw; }
            }
        }

        public async Task<NhaCungCap> GetNhaCungCapById(int id)
        {
            // === SỬA === (Sửa tên bảng và WHERE bằng "Id")
            string sql = "SELECT * FROM tbl_DM_NCC WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<NhaCungCap>(sql, new { Id = id });
            }
        }

        /* HÀM XÓA (DELETE) */
        public async Task DeleteNhaCungCap(int id)
        {
            // === SỬA === (WHERE bằng "Id")
            string sql = "DELETE FROM tbl_DM_NCC WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                // === THÊM (Nên có) === Bắt lỗi khóa ngoại
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547) // Lỗi ràng buộc khóa ngoại
                    {
                        throw new Exception("Không thể xóa nhà cung cấp này vì đang được sử dụng (ví dụ: trong phiếu nhập).");
                    }
                    else { throw; }
                }
            }
        }
    }
}