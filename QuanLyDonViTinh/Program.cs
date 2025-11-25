/* Đây là code ĐÚNG cho dự án .NET 7.0 của bạn 
*/

using Microsoft.AspNetCore.Localization;
using System.Globalization;
using QuanLyDonViTinh.Services; // Thêm dòng này để "thấy" Service

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/* .NET 7 dùng 2 dòng này */
builder.Services.AddRazorPages();   
builder.Services.AddServerSideBlazor();

/* Thêm "người thợ" (Service) của bạn vào */
builder.Services.AddScoped<DonViTinhService>();
builder.Services.AddScoped<LoaiSanPhamService>();
builder.Services.AddScoped<SanPhamService>();
builder.Services.AddScoped<NhaCungCapService>();
builder.Services.AddScoped<KhoService>();
builder.Services.AddScoped<KhoUserService>();
builder.Services.AddScoped<NhapKhoService>();
builder.Services.AddScoped<XuatKhoService>();
builder.Services.AddScoped<ReportService>();


builder.Services.AddLocalization();
var app = builder.Build();
var supportedCultures = new[] { new CultureInfo("vi-VN") };
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};


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
app.UseRequestLocalization(locOptions);
app.Run();
