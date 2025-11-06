/* Đây là code ĐÚNG cho dự án .NET 7.0 của bạn 
*/

using QuanLyDonViTinh.Services; // Thêm dòng này để "thấy" Service

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/* .NET 7 dùng 2 dòng này */
builder.Services.AddRazorPages();   
builder.Services.AddServerSideBlazor();

/* Thêm "người thợ" (Service) của bạn vào */
builder.Services.AddSingleton<DonViTinhService>();
builder.Services.AddSingleton<LoaiSanPhamService>();
builder.Services.AddSingleton<SanPhamService>();
builder.Services.AddSingleton<NhaCungCapService>();
builder.Services.AddSingleton<KhoService>();
builder.Services.AddSingleton<KhoUserService>();
builder.Services.AddSingleton<NhapKhoService>();
builder.Services.AddSingleton<XuatKhoService>();
builder.Services.AddSingleton<ReportService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

/* .NET 7 dùng 2 dòng này để chạy Blazor */
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();