# Website Giới thiệu Di tích Lịch sử và Văn hóa Việt Nam

Đồ án học phần Thực tập Đồ án Cơ sở ngành, năm học 2024 - 2028.

## 1. Thông tin sinh viên

| Thông tin | Nội dung |
|---|---|
| Họ và tên | Phạm Văn Phong |
| MSSV | 170124953 |
| Lớp | DX24TT8 |
| Email | phong.pv579@gmail.com |
| Số điện thoại | 0961458313 |
| Khoa | CNTT |
| Trường | Đại Học Trà Vinh |
| Học phần | Thực tập Đồ án Cơ sở ngành |
| Giảng viên hướng dẫn | Trầm Hoàng Nam |

## 2. Giới thiệu đề tài

Việt Nam có hàng nghìn di tích đã được xếp hạng cấp tỉnh và cấp quốc gia, nhưng thông tin về chúng nằm rải rác ở nhiều nơi: cổng thông tin của từng địa phương, trang du lịch thương mại, bài báo và bài viết cá nhân. Mức độ chi tiết và độ tin cậy giữa các nguồn không đồng đều, nhiều trang chỉ mô tả sơ lược hoặc thiếu hẳn địa chỉ, lịch sử hình thành và thông tin phục vụ tham quan. Người muốn tra cứu một di tích cụ thể, hoặc muốn biết một tỉnh có những di tích nào, thường phải tìm qua nhiều trang rồi tự đối chiếu.

Đề tài xây dựng một website tập hợp thông tin về các di tích lịch sử và văn hóa tiêu biểu của Việt Nam, tổ chức theo tỉnh hoặc thành phố và theo loại di tích. Mỗi di tích được trình bày đầy đủ tên gọi, hình ảnh, địa chỉ, lịch sử, mô tả giá trị và thông tin tham quan, kèm nguồn tham khảo. Bên cạnh phần tra cứu dành cho người xem, hệ thống có khu vực quản trị để cập nhật dữ liệu di tích cùng hai danh mục địa phương và loại di tích.

## 3. Chức năng chính

Phía người dùng:

- Xem trang chủ với giới thiệu tổng quan và một số di tích nổi bật.
- Duyệt di tích theo tỉnh hoặc thành phố.
- Duyệt di tích theo loại di tích.
- Tìm kiếm theo tên và từ khóa trong mô tả, hỗ trợ nhập không dấu.
- Lọc kết hợp nhiều tiêu chí: địa phương, loại di tích, cấp xếp hạng.
- Xem trang chi tiết di tích: tên, bộ ảnh, địa chỉ, lịch sử, mô tả, thông tin tham quan.
- Phân trang danh sách kết quả.
- Giao diện responsive, hiển thị được trên máy tính, máy tính bảng và điện thoại.

Phía quản trị:

- Đăng nhập và đăng xuất tài khoản quản trị.
- Thêm, sửa, xóa và xem danh sách di tích.
- Tải lên, xóa ảnh và đặt ảnh đại diện cho từng di tích.
- Quản lý danh mục tỉnh và thành phố.
- Quản lý danh mục loại di tích.
- Trang thống kê số lượng di tích theo địa phương và theo loại.

## 4. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Nền tảng | .NET 10 |
| Backend | ASP.NET Core MVC |
| Truy xuất dữ liệu | Entity Framework Core, Code First kèm Migrations |
| Cơ sở dữ liệu | SQLite |
| Giao diện | Razor View, HTML, CSS, JavaScript |
| Thư viện giao diện | Bootstrap 5.3 |
| Xác thực và phân quyền | ASP.NET Core Identity |
| Quản lý mã nguồn | Git và GitHub |

## 5. Kiến trúc và cơ sở dữ liệu

Ứng dụng theo mô hình MVC, tách thêm một lớp Service để giữ logic nghiệp vụ ngoài Controller. Luồng xử lý một yêu cầu đi qua các lớp: View (Razor) - Controller - Service - DbContext (EF Core) - SQLite.

Cơ sở dữ liệu gồm bốn bảng chính:

- `Relic` lưu thông tin từng di tích: tên, slug, địa chỉ, lịch sử, mô tả, thông tin tham quan, cấp xếp hạng và nguồn tham khảo.
- `Province` là danh mục tỉnh và thành phố, mỗi bản ghi có tên, slug và vùng miền.
- `RelicType` là danh mục loại di tích.
- `RelicImage` lưu các ảnh của một di tích, trong đó đánh dấu một ảnh làm ảnh đại diện.

Loại di tích phân theo Luật Di sản văn hóa, gồm bốn nhóm: di tích lịch sử, di tích kiến trúc nghệ thuật, di tích khảo cổ và danh lam thắng cảnh.

## 6. Hướng dẫn cài đặt và chạy

Yêu cầu môi trường: .NET SDK 10.0 trở lên, Git, và Visual Studio 2022 hoặc Visual Studio Code kèm C# Dev Kit.

Tải mã nguồn về máy và mở thư mục dự án web:

```bash
git clone <đường dẫn repository>
cd src/DiTichVietNam.Web
```

Khôi phục các package và tạo cơ sở dữ liệu:

```bash
dotnet restore
dotnet tool install --global dotnet-ef   # bỏ qua nếu đã cài
dotnet ef database update
```

Chuỗi kết nối nằm trong `appsettings.json`, mặc định trỏ tới file SQLite trong thư mục `Data`. Dữ liệu mẫu gồm danh mục tỉnh thành, loại di tích và tài khoản quản trị được nạp tự động ở lần chạy đầu tiên.

Chạy ứng dụng và mở địa chỉ hiển thị trong cửa sổ terminal:

```bash
dotnet run
```

Tài khoản quản trị dùng cho môi trường phát triển và demo: `admin@ditich.vn`, mật khẩu `Admin@123`.

## 7. Hướng phát triển

- Hiển thị vị trí di tích trên bản đồ và gợi ý các di tích lân cận.
- Bổ sung giao diện tiếng Anh phục vụ khách quốc tế.
- Xây dựng API để dùng chung dữ liệu cho ứng dụng di động.

## 8. Tài liệu tham khảo

1. Microsoft Docs - ASP.NET Core MVC: https://learn.microsoft.com/aspnet/core/mvc/overview
2. Microsoft Docs - Entity Framework Core: https://learn.microsoft.com/ef/core/
3. Bootstrap 5.3 Documentation: https://getbootstrap.com/docs/5.3/
4. Luật Di sản văn hóa (sửa đổi, bổ sung) - quy định về phân loại và xếp hạng di tích
5. Cục Di sản văn hóa - hệ thống dữ liệu di tích quốc gia
