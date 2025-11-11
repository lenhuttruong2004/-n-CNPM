using System;
using System.Text;

// Đặt namespace này trùng với dự án của bạn
namespace QuanLyDonViTinh.Models
{
    public static class VNCurrency
    {
        private static readonly string[] ChuSo = { " không ", " một ", " hai ", " ba ", " bốn ", " năm ", " sáu ", " bảy ", " tám ", " chín " };
        private static readonly string[] Tien = { "", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ" };

        private static string DocSoBaChuSo(int baso)
        {
            int tram, chuc, donvi;
            string KetQua = "";
            tram = baso / 100;
            chuc = (baso % 100) / 10;
            donvi = baso % 10;
            if (tram == 0 && chuc == 0 && donvi == 0) return "";

            if (tram != 0)
            {
                KetQua += ChuSo[tram] + " trăm ";
                if (chuc == 0 && donvi != 0) KetQua += " linh ";
            }
            if (chuc != 0 && chuc != 1)
            {
                KetQua += ChuSo[chuc] + " mươi";
                if (chuc == 0 && donvi != 0) KetQua += " linh ";
            }
            if (chuc == 1) KetQua += " mười ";
            switch (donvi)
            {
                case 1:
                    if (chuc != 0 && chuc != 1) KetQua += " mốt ";
                    else KetQua += ChuSo[donvi];
                    break;
                case 5:
                    if (chuc == 0) KetQua += ChuSo[donvi];
                    else KetQua += " lăm ";
                    break;
                default:
                    if (donvi != 0) KetQua += ChuSo[donvi];
                    break;
            }
            return KetQua;
        }

        public static string ToString(decimal soTien)
        {
            long soTienNguyen = (long)Math.Round(soTien, 0);
            return ToString(soTienNguyen);
        }

        public static string ToString(long soTien)
        {
            if (soTien == 0) return "Không";

            long So;
            int lan = 0, i = 0;
            string KetQua = "", tmp = "";
            long[] ViTri = new long[6];
            if (soTien < 0) return "Số tiền âm!";

            ViTri[5] = soTien / 1000000000000000;
            soTien = soTien % 1000000000000000;
            ViTri[4] = soTien / 1000000000000;
            soTien = soTien % 1000000000000;
            ViTri[3] = soTien / 1000000000;
            soTien = soTien % 1000000000;
            ViTri[2] = soTien / 1000000;
            ViTri[1] = (soTien % 1000000) / 1000;
            ViTri[0] = soTien % 1000;

            if (ViTri[5] > 0) lan = 5;
            else if (ViTri[4] > 0) lan = 4;
            else if (ViTri[3] > 0) lan = 3;
            else if (ViTri[2] > 0) lan = 2;
            else if (ViTri[1] > 0) lan = 1;
            else lan = 0;

            for (i = lan; i >= 0; i--)
            {
                tmp = DocSoBaChuSo((int)ViTri[i]);
                KetQua += tmp;
                if (ViTri[i] != 0) KetQua += Tien[i];
                if (i > 0 && !string.IsNullOrEmpty(tmp)) KetQua += ",";
            }

            if (KetQua.EndsWith(",")) KetQua = KetQua.Substring(0, KetQua.Length - 1);
            KetQua = KetQua.Trim();

            string result = KetQua.Substring(0, 1).ToUpper() + KetQua.Substring(1);
            if (result.EndsWith(","))
                result = result.Substring(0, result.Length - 1);

            return result.Replace("  ", " ").Trim();
        }
    }
}