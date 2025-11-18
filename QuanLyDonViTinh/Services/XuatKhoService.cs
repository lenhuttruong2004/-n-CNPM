using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class XuatKhoService
    {
        private readonly string _connectionString;

        public XuatKhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* 1. LẤY DANH SÁCH (Đơn giản hóa: Chỉ lấy từ tbl_DM_Xuat_Kho) */
        public async Task<IEnumerable<XuatKho>> GetDanhSach()
        {
            // Lưu ý: Xuất kho thường không có Nhà Cung Cấp (NCC), chỉ có Kho
            string sql = @"
                SELECT 
                    xk.Id, 
                    xk.So_Phieu_Xuat_Kho, 
                    xk.Ngay_Xuat_Kho, 
                    xk.Ghi_Chu,
                    k.Ten_Kho, 
                    ISNULL(SUM(xkr.SL_Xuat * xkr.Don_Gia_Xuat), 0) AS Tong_Tien
                FROM tbl_DM_Xuat_Kho xk
                LEFT JOIN tbl_DM_Kho k ON xk.Kho_ID = k.Id
                LEFT JOIN tbl_DM_Xuat_Kho_Raw_Data xkr ON xk.Id = xkr.Xuat_Kho_ID
                GROUP BY 
                    xk.Id, xk.So_Phieu_Xuat_Kho, xk.Ngay_Xuat_Kho, xk.Ghi_Chu,
                    k.Ten_Kho
                ORDER BY xk.Id DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<XuatKho>(sql);
            }
        }

        /* 2. LẤY 1 PHIẾU THEO ID */
        public async Task<XuatKho> GetPhieuXuatById(int id)
        {
            string sql = @"
                SELECT Id, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu 
                FROM tbl_DM_Xuat_Kho 
                WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<XuatKho>(sql, new { Id = id });
            }
        }

        /* 3. THÊM MỚI PHIẾU */
        public async Task AddPhieuXuat(XuatKhoFull phieuXuatFull)
        {
            if (phieuXuatFull.Details == null || !phieuXuatFull.Details.Any())
                throw new Exception("Phiếu xuất phải có ít nhất một sản phẩm chi tiết.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Thêm Header
                        string headerSql = @"
                            INSERT INTO tbl_DM_Xuat_Kho (So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu) 
                            VALUES (@So_Phieu_Xuat_Kho, @Kho_ID, @Ngay_Xuat_Kho, @Ghi_Chu);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                        int newId = await connection.QuerySingleAsync<int>(headerSql, phieuXuatFull.Header, transaction: transaction);

                        // Thêm Detail (Chi tiết)
                        string detailSql = @"
                            INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) 
                            VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";

                        foreach (var detail in phieuXuatFull.Details)
                        {
                            detail.Xuat_Kho_ID = newId; // Gán ID vừa tạo
                            await connection.ExecuteAsync(detailSql, detail, transaction: transaction);
                        }
                        transaction.Commit();
                    }
                    catch (Exception) { transaction.Rollback(); throw; }
                }
            }
        }

        /* 4. CẬP NHẬT PHIẾU (UPDATE trực tiếp) */
        public async Task UpdatePhieuXuat(XuatKho xuatKho)
        {
            string sql = @"
                UPDATE tbl_DM_Xuat_Kho 
                SET So_Phieu_Xuat_Kho = @So_Phieu_Xuat_Kho,
                    Kho_ID = @Kho_ID,
                    Ngay_Xuat_Kho = @Ngay_Xuat_Kho,
                    Ghi_Chu = @Ghi_Chu
                WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, xuatKho);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        throw new Exception("Lỗi: Số phiếu xuất này đã tồn tại.");
                    }
                    throw;
                }
            }
        }

        /* 5. XÓA PHIẾU */
        public async Task DeletePhieuXuat(int id)
        {
            // Do có ON DELETE CASCADE trong SQL nên chỉ cần xóa bảng cha
            string sql = "DELETE FROM tbl_DM_Xuat_Kho WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        /* 6. LẤY CHI TIẾT SẢN PHẨM TRONG PHIẾU */
        public async Task<List<XuatKhoRawData>> GetChiTiet(int xuatKhoId)
        {
            string sql = @"
                SELECT 
                    xkr.Id, xkr.Xuat_Kho_ID, xkr.San_Pham_ID, xkr.SL_Xuat, xkr.Don_Gia_Xuat,
                    sp.Ma_San_Pham, 
                    sp.Ten_San_Pham,
                    dvt.Ten_Don_Vi_Tinh 
                FROM tbl_DM_Xuat_Kho_Raw_Data xkr
                LEFT JOIN tbl_DM_San_Pham sp ON xkr.San_Pham_ID = sp.Id
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Id
                WHERE xkr.Xuat_Kho_ID = @XuatKhoId";

            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<XuatKhoRawData>(sql, new { XuatKhoId = xuatKhoId });
                return result.ToList();
            }
        }

        /* 7. CÁC HÀM CRUD CHO CHI TIẾT (Thêm/Sửa/Xóa từng dòng sản phẩm) */
        public async Task UpdateChiTiet(XuatKhoRawData detail)
        {
            string sql = "UPDATE tbl_DM_Xuat_Kho_Raw_Data SET SL_Xuat = @SL_Xuat, Don_Gia_Xuat = @Don_Gia_Xuat WHERE Id = @ID";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task AddChiTiet(XuatKhoRawData detail)
        {
            string sql = "INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task DeleteChiTiet(int id)
        {
            string sql = "DELETE FROM tbl_DM_Xuat_Kho_Raw_Data WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, new { Id = id }); }
        }

        /* 8. LẤY VIEW MODEL ĐỂ IN ẤN */
        public async Task<PhieuXuatViewModel> GetPhieuXuatView(int id)
        {
            var header = await GetPhieuXuatById(id);
            if (header == null) return null;

            // Lấy thêm tên kho để hiển thị khi in
            using (var connection = new SqlConnection(_connectionString))
            {
                string sqlKho = "SELECT Ten_Kho FROM tbl_DM_Kho WHERE Id = @Id";
                header.Ten_Kho = await connection.QueryFirstOrDefaultAsync<string>(sqlKho, new { Id = header.Kho_ID });
            }

            var details = await GetChiTiet(id);
            decimal tongTien = details.Sum(d => d.SL_Xuat * d.Don_Gia_Xuat);

            return new PhieuXuatViewModel
            {
                Header = header,
                Details = details,
                TongTienSo = tongTien,
                TongTienVietChu = "" // Bạn cần bổ sung hàm đọc số thành chữ nếu muốn
            };
        }

        /* 9. BÁO CÁO CHI TIẾT HÀNG XUẤT */
        public async Task<IEnumerable<BaoCaoChiTietHangXuatViewModel>> GetBaoCaoChiTietHangXuat(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"
                SELECT 
                    xk.Ngay_Xuat_Kho, xk.So_Phieu_Xuat_Kho,
                    xkr.San_Pham_ID, sp.Ma_San_Pham, sp.Ten_San_Pham, 
                    xkr.SL_Xuat, xkr.Don_Gia_Xuat
                FROM tbl_DM_Xuat_Kho_Raw_Data xkr
                INNER JOIN tbl_DM_Xuat_Kho xk ON xkr.Xuat_Kho_ID = xk.Id
                INNER JOIN tbl_DM_San_Pham sp ON xkr.San_Pham_ID = sp.Id
                WHERE xk.Ngay_Xuat_Kho >= @TuNgay AND xk.Ngay_Xuat_Kho <= @DenNgay
                ORDER BY xk.Ngay_Xuat_Kho, xk.So_Phieu_Xuat_Kho, sp.Ten_San_Pham;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<BaoCaoChiTietHangXuatViewModel>(sql, new { TuNgay = tuNgay, DenNgay = denNgay });
            }
        }
    }
}