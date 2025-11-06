/* Dòng này rất quan trọng để dùng [Required] */
using System.ComponentModel.DataAnnotations;

/* 'QuanLyDonViTinh' là tên dự án của bạn */
namespace QuanLyDonViTinh.Models
{
    public class DonViTinh
    {
        /* Các thuộc tính này phải đặt tên 
           GIỐNG HỆT TÊN CỘT trong SQL Server 
        */
        public int Ma_Don_Vi_Tinh { get; set; }

        /* [Required] để xử lý ràng buộc "không được rỗng"
           ErrorMessage là thông báo sẽ hiện ra nếu người dùng không nhập
        */
        [Required(ErrorMessage = "Tên đơn vị tính không được để trống")]
        public string Ten_Don_Vi_Tinh { get; set; }

        public string Ghi_Chu { get; set; }
    }
}