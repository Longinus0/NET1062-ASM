PRAGMA foreign_keys = ON;

INSERT INTO Users (UserId, FullName, Email, Phone, PasswordHash, Address, AvatarUrl, IsActive, ForcePasswordReset, CreatedAt, UpdatedAt) VALUES
  (1, 'Nguyễn Minh Anh', 'minhanh.nguyen@example.vn', '0909123456', 'hash_minhanh', '12 Nguyễn Huệ, Quận 1, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (2, 'Trần Thanh Tùng', 'tung.tran@example.vn', '0912345678', 'hash_tung', '55 Lê Lợi, Quận 1, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (3, 'Lê Thu Hà', 'ha.le@example.vn', '0987000111', 'hash_ha', '88 Trần Hưng Đạo, Quận 5, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (4, 'Phạm Quang Huy', 'huy.pham@example.vn', '0978123456', 'hash_huy', '21 Hai Bà Trưng, Quận 1, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (5, 'Võ Ngọc Linh', 'linh.vo@example.vn', '0933555777', 'hash_linh', '9 Pasteur, Quận 3, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (6, 'Đỗ Minh Khoa', 'khoa.do@example.vn', '0909555999', 'hash_khoa', '10 Võ Văn Tần, Quận 3, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (7, 'Bùi Thanh An', 'an.bui@example.vn', '0903000111', 'hash_an', '3 Nguyễn Trãi, Quận 5, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (8, 'Đào Khánh Vy', 'vy.dao@example.vn', '0911000222', 'hash_vy', '17 Lý Tự Trọng, Quận 1, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (9, 'Hoài My Nguyễn', 'my.nguyen@example.vn', '0908111222', 'hash_my', '44 Điện Biên Phủ, Quận 10, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (10, 'Phan Bảo Minh', 'minh.phan@example.vn', '0908777666', 'hash_minh', '102 Cách Mạng Tháng 8, Quận 3, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime'));

INSERT INTO Role (RoleId, Name) VALUES
  (1, 'Quản trị'),
  (2, 'Nhân viên'),
  (3, 'Khách hàng');

INSERT INTO UserRole (UserId, RoleId) VALUES
  (1, 1),
  (2, 2),
  (3, 3),
  (4, 3),
  (5, 3),
  (6, 3),
  (7, 3),
  (8, 3),
  (9, 3),
  (10, 3);

INSERT INTO ExternalLogin (ExternalLoginId, UserId, Provider, ProviderId, Email, CreatedAt) VALUES
  (1, 3, 'Google', 2, 'lethuha@gmail.com', datetime('now','localtime')),
  (2, 4, 'Facebook', 1, 'phamquanghuy@facebook.com', datetime('now','localtime'));

INSERT INTO Category (CategoryId, Name, Description) VALUES
  (1, 'Bánh kẹp', 'Các loại bánh kẹp và bánh mì kẹp'),
  (2, 'Gà', 'Gà rán và gà nướng'),
  (3, 'Món ăn kèm', 'Khoai tây, salad và món phụ'),
  (4, 'Đồ uống', 'Nước giải khát và cà phê'),
  (5, 'Tráng miệng', 'Món ngọt và kem');

INSERT INTO Product (ProductId, CategoryId, Name, Description, Price, ImageUrl, TopicTag, IsAvailable, StockQty, CreatedAt, UpdatedAt) VALUES
  (1, 1, 'Bánh kẹp Bò Cổ Điển', 'Bò, rau, cà chua, sốt nhà làm', 59000, 'https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=900&q=80', 'bán-chạy', 1, 120, datetime('now','localtime'), datetime('now','localtime')),
  (2, 1, 'Bánh kẹp Bò Phô Mai', '2 miếng bò, phô mai, dưa leo', 69000, 'https://images.unsplash.com/photo-1550317138-10000687a72b?auto=format&fit=crop&w=900&q=80', 'mới', 1, 80, datetime('now','localtime'), datetime('now','localtime')),
  (3, 1, 'Bánh kẹp Bò Cay', 'Sốt cay, ớt jalapeno, phô mai', 72000, 'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=900&q=80', 'cay', 1, 70, datetime('now','localtime'), datetime('now','localtime')),
  (4, 2, 'Bánh kẹp Gà Giòn', 'Gà giòn, bắp cải, dưa leo', 62000, 'https://images.unsplash.com/photo-1606755962773-d324e0a13086?auto=format&fit=crop&w=900&q=80', 'bán-chạy', 1, 90, datetime('now','localtime'), datetime('now','localtime')),
  (5, 2, 'Cuốn Gà Nướng', 'Gà nướng, rau romaine, sốt béo', 58000, 'https://images.unsplash.com/photo-1551782450-17144efb9c50?auto=format&fit=crop&w=900&q=80', 'nhẹ', 1, 60, datetime('now','localtime'), datetime('now','localtime')),
  (6, 2, 'Cánh Gà Sốt Cay 6 miếng', 'Cánh gà giòn sốt cay', 55000, 'https://images.unsplash.com/photo-1516685018646-549d3f4076d7?auto=format&fit=crop&w=900&q=80', 'cay', 1, 150, datetime('now','localtime'), datetime('now','localtime')),
  (7, 3, 'Khoai Tây Chiên', 'Khoai tây chiên muối biển', 29000, 'https://images.unsplash.com/photo-1541592106381-b31e9677c0e5?auto=format&fit=crop&w=900&q=80', 'món-phụ', 1, 200, datetime('now','localtime'), datetime('now','localtime')),
  (8, 3, 'Khoai Tây Phô Mai', 'Phô mai, thịt xông khói', 42000, 'https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=900&q=80', 'bán-chạy', 1, 110, datetime('now','localtime'), datetime('now','localtime')),
  (9, 3, 'Salad Vườn', 'Rau trộn, cà chua bi, sốt dầu', 39000, 'https://images.unsplash.com/photo-1546069901-eacef0df6022?auto=format&fit=crop&w=900&q=80', 'nhẹ', 1, 50, datetime('now','localtime'), datetime('now','localtime')),
  (10, 4, 'Coca 330ml', 'Nước coca lạnh', 15000, 'https://images.unsplash.com/photo-1510626176961-4b57d4fbad03?auto=format&fit=crop&w=900&q=80', 'đồ-uống', 1, 300, datetime('now','localtime'), datetime('now','localtime')),
  (11, 4, 'Trà Chanh Đá', 'Trà chanh 400ml', 18000, 'https://images.unsplash.com/photo-1497534446932-c925b458314e?auto=format&fit=crop&w=900&q=80', 'đồ-uống', 1, 260, datetime('now','localtime'), datetime('now','localtime')),
  (12, 4, 'Cà Phê Cold Brew', 'Cà phê cold brew 350ml', 30000, 'https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=900&q=80', 'mới', 1, 140, datetime('now','localtime'), datetime('now','localtime')),
  (13, 5, 'Kem Socola', 'Kem vani với sốt socola', 25000, 'https://images.unsplash.com/photo-1505252585461-04db1eb84625?auto=format&fit=crop&w=900&q=80', 'tráng-miệng', 1, 90, datetime('now','localtime'), datetime('now','localtime')),
  (14, 5, 'Bánh Táo Nướng', 'Một miếng bánh táo nóng', 22000, 'https://images.unsplash.com/photo-1519681393784-d120267933ba?auto=format&fit=crop&w=900&q=80', 'tráng-miệng', 1, 120, datetime('now','localtime'), datetime('now','localtime')),
  (15, 5, 'Bánh Donut 6 cái', 'Donut quế đường quế', 28000, 'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=900&q=80', 'trẻ-em', 1, 100, datetime('now','localtime'), datetime('now','localtime')),
  (16, 1, 'Bánh kẹp Double Cheese', 'Hai lớp bò, phô mai chảy, sốt đặc biệt.', 79000, 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=900&q=80', 'bán-chạy', 1, 60, datetime('now','localtime'), datetime('now','localtime')),
  (17, 2, 'Gà Nướng Mật Ong', 'Gà nướng mật ong thơm ngọt.', 69000, 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=900&q=80', 'mới', 1, 50, datetime('now','localtime'), datetime('now','localtime')),
  (18, 3, 'Khoai Lắc Phô Mai', 'Khoai tây lắc phô mai béo.', 35000, 'https://images.unsplash.com/photo-1488900128323-21503983a07e?auto=format&fit=crop&w=900&q=80', 'món-phụ', 1, 120, datetime('now','localtime'), datetime('now','localtime')),
  (19, 4, 'Matcha Latte Đá', 'Matcha latte mát lạnh.', 32000, 'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=900&q=80', 'mới', 1, 80, datetime('now','localtime'), datetime('now','localtime')),
  (20, 5, 'Kem Vani', 'Kem vani mịn mát.', 22000, 'https://images.unsplash.com/photo-1504754524776-8f4f37790ca0?auto=format&fit=crop&w=900&q=80', 'tráng-miệng', 1, 120, datetime('now','localtime'), datetime('now','localtime')),
  (21, 3, 'Salad Ức Gà', 'Salad rau xanh cùng ức gà nướng.', 45000, 'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=900&q=80', 'nhẹ', 1, 70, datetime('now','localtime'), datetime('now','localtime'));

INSERT INTO ProductTag (TagId, Name) VALUES
  (1, 'Bán chạy'),
  (2, 'Cay nồng'),
  (3, 'Mới'),
  (4, 'Trẻ em'),
  (5, 'Nhẹ');

INSERT INTO ProductTagMap (ProductId, TagId) VALUES
  (1, 1),
  (2, 3),
  (3, 2),
  (4, 1),
  (6, 2),
  (8, 1),
  (12, 3),
  (9, 5),
  (15, 4);

INSERT INTO Combo (ComboId, Name, Description, Price, IsActive) VALUES
  (1, 'Combo Cổ Điển', 'Bánh kẹp bò + khoai tây + coca', 99000, 1),
  (2, 'Combo Gà Giòn', 'Bánh kẹp gà + khoai tây + trà chanh', 99000, 1),
  (3, 'Combo Ăn Nhẹ', 'Cuốn gà nướng + salad + trà chanh', 95000, 1),
  (4, 'Combo Phô Mai Party', 'Bánh kẹp double cheese + khoai lắc + coca.', 119000, 1),
  (5, 'Combo Mật Ong', 'Gà nướng mật ong + salad ức gà + trà chanh.', 109000, 1),
  (6, 'Combo Sweet Chill', 'Cuốn gà nướng + matcha latte + kem vani.', 99000, 1),
  (7, 'Combo Gia Đình', '2 bánh kẹp bò cổ điển + khoai tây chiên + 2 coca.', 175000, 1),
  (8, 'Combo Năng Lượng', 'Bánh kẹp bò phô mai + khoai lắc + cold brew.', 129000, 1),
  (9, 'Combo Trưa Nhanh', 'Bánh kẹp gà giòn + khoai tây + coca.', 105000, 1),
  (10, 'Combo Vegetarian', 'Salad vườn + khoai tây + trà chanh.', 89000, 1),
  (11, 'Combo Đêm Muộn', 'Bánh kẹp bò cay + coca + kem socola.', 99000, 1);

INSERT INTO ComboItem (ComboId, ProductId, Quantity) VALUES
  (1, 1, 1),
  (1, 7, 1),
  (1, 10, 1),
  (2, 4, 1),
  (2, 7, 1),
  (2, 11, 1),
  (3, 5, 1),
  (3, 9, 1),
  (3, 11, 1),
  (4, 16, 1),
  (4, 18, 1),
  (4, 10, 1),
  (5, 17, 1),
  (5, 21, 1),
  (5, 11, 1),
  (6, 5, 1),
  (6, 19, 1),
  (6, 20, 1),
  (7, 1, 2),
  (7, 7, 1),
  (7, 10, 2),
  (8, 2, 1),
  (8, 18, 1),
  (8, 12, 1),
  (9, 4, 1),
  (9, 7, 1),
  (9, 10, 1),
  (10, 9, 1),
  (10, 7, 1),
  (10, 11, 1),
  (11, 3, 1),
  (11, 10, 1),
  (11, 13, 1);

INSERT INTO Orders (OrderId, UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, Note, CreatedAt) VALUES
  (1, 3, 'FF-2024-0001', 'Đã giao', 103000, 5000, 15000, 113000, 'Đã thanh toán', 'Tiền mặt', 'Không hành', datetime('now','localtime')),
  (2, 4, 'FF-2024-0002', 'Đang chuẩn bị', 104000, 0, 15000, 119000, 'Đang xử lý', 'MoMo', NULL, datetime('now','localtime')),
  (3, 5, 'FF-2024-0003', 'Đã hủy', 72000, 0, 15000, 87000, 'Đã hoàn tiền', 'Thẻ', 'Khách hủy đơn', datetime('now','localtime'));

INSERT INTO OrderItem (OrderItemId, OrderId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal) VALUES
  (1, 1, 1, 'Bánh kẹp Bò Cổ Điển', 59000, 1, 59000),
  (2, 1, 7, 'Khoai Tây Chiên', 29000, 1, 29000),
  (3, 1, 10, 'Coca 330ml', 15000, 1, 15000),
  (4, 2, 4, 'Bánh kẹp Gà Giòn', 62000, 1, 62000),
  (5, 2, 8, 'Khoai Tây Phô Mai', 42000, 1, 42000),
  (6, 3, 3, 'Bánh kẹp Bò Cay', 72000, 1, 72000);

INSERT INTO Users (UserId, FullName, Email, Phone, PasswordHash, Address, AvatarUrl, IsActive, ForcePasswordReset, CreatedAt, UpdatedAt) VALUES
  (13, 'Ngô Nhật Long', 'long.ngo@example.vn', '0904333222', 'hash_long', '25 Tôn Đức Thắng, Quận 1, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (14, 'Lưu Phương Thảo', 'thao.luu@example.vn', '0911222333', 'hash_thao', '77 Nguyễn Văn Cừ, Quận 5, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (15, 'Phạm Hải Yến', 'yen.pham@example.vn', '0988333444', 'hash_yen', '18 Phan Xích Long, Phú Nhuận, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (16, 'Đặng Quốc Bảo', 'bao.dang@example.vn', '0977555666', 'hash_bao', '66 Lý Thường Kiệt, Quận 10, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime'));

INSERT INTO Orders (OrderId, UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, Note, CreatedAt) VALUES
  (4, 6, 'FF-2024-0004', 'Đã giao', 123000, 5000, 15000, 133000, 'Đã thanh toán', 'VNPay', 'Giao nhanh buổi sáng', datetime('now','localtime')),
  (5, 7, 'FF-2024-0005', 'Đang chuẩn bị', 94000, 0, 15000, 109000, 'Đang xử lý', 'MoMo', NULL, datetime('now','localtime')),
  (6, 8, 'FF-2024-0006', 'Đang giao', 91000, 10000, 15000, 96000, 'Đã thanh toán', 'Thẻ', 'Ưu tiên giữ nóng', datetime('now','localtime')),
  (7, 9, 'FF-2024-0007', 'Mới', 85000, 0, 15000, 100000, 'Chưa thanh toán', 'Tiền mặt', NULL, datetime('now','localtime')),
  (8, 10, 'FF-2024-0008', 'Đã hủy', 94000, 5000, 15000, 104000, 'Đã hoàn tiền', 'ZaloPay', 'Khách đổi ý', datetime('now','localtime'));

INSERT INTO OrderItem (OrderItemId, OrderId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal) VALUES
  (7, 4, 16, 'Bánh kẹp Double Cheese', 79000, 1, 79000),
  (8, 4, 10, 'Coca 330ml', 15000, 1, 15000),
  (9, 4, 7, 'Khoai Tây Chiên', 29000, 1, 29000),
  (10, 5, 5, 'Cuốn Gà Nướng', 58000, 1, 58000),
  (11, 5, 11, 'Trà Chanh Đá', 18000, 2, 36000),
  (12, 6, 12, 'Cà Phê Cold Brew', 30000, 1, 30000),
  (13, 6, 9, 'Salad Vườn', 39000, 1, 39000),
  (14, 6, 14, 'Bánh Táo Nướng', 22000, 1, 22000),
  (15, 7, 18, 'Khoai Lắc Phô Mai', 35000, 2, 70000),
  (16, 7, 10, 'Coca 330ml', 15000, 1, 15000),
  (17, 8, 17, 'Gà Nướng Mật Ong', 69000, 1, 69000),
  (18, 8, 13, 'Kem Socola', 25000, 1, 25000);

INSERT INTO Users (UserId, FullName, Email, Phone, PasswordHash, Address, AvatarUrl, IsActive, ForcePasswordReset, CreatedAt, UpdatedAt) VALUES
  (17, 'Tạ Bảo Trâm', 'tram.ta@example.vn', '0905666777', 'hash_tram', '22 Lê Thánh Tôn, Quận 1, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (18, 'Nguyễn Quốc Hưng', 'hung.nguyen@example.vn', '0915444333', 'hash_hung', '18 Nguyễn Thị Minh Khai, Quận 3, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (19, 'Phạm Gia Hân', 'han.pham@example.vn', '0972111222', 'hash_han', '45 Trần Não, TP.Thủ Đức, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (20, 'Vũ Trí Đức', 'duc.vu@example.vn', '0933444555', 'hash_duc', '9 Hoàng Văn Thụ, Tân Bình, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (21, 'Đỗ Hoàng Nam', 'nam.do@example.vn', '0987333555', 'hash_nam', '63 Lê Văn Sỹ, Phú Nhuận, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime')),
  (22, 'Lê Ngọc Mai', 'mai.le@example.vn', '0908444999', 'hash_mai', '27 Phan Đăng Lưu, Bình Thạnh, TP.HCM', NULL, 1, 0, datetime('now','localtime'), datetime('now','localtime'));

INSERT INTO Orders (OrderId, UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, Note, CreatedAt) VALUES
  (9, 11, 'FF-2024-0009', 'Đang chuẩn bị', 118000, 0, 15000, 133000, 'Đang xử lý', 'MoMo', NULL, datetime('now','localtime')),
  (10, 12, 'FF-2024-0010', 'Đã giao', 142000, 12000, 15000, 145000, 'Đã thanh toán', 'VNPay', 'Giao lúc trưa', datetime('now','localtime')),
  (11, 13, 'FF-2024-0011', 'Đang giao', 97000, 0, 15000, 112000, 'Đã thanh toán', 'Thẻ', NULL, datetime('now','localtime')),
  (12, 14, 'FF-2024-0012', 'Mới', 88000, 0, 15000, 103000, 'Chưa thanh toán', 'Tiền mặt', NULL, datetime('now','localtime')),
  (13, 15, 'FF-2024-0013', 'Đã hủy', 76000, 0, 15000, 91000, 'Đã hoàn tiền', 'ZaloPay', 'Khách đổi địa chỉ', datetime('now','localtime')),
  (14, 16, 'FF-2024-0014', 'Đã giao', 126000, 5000, 15000, 136000, 'Đã thanh toán', 'Thẻ', 'Ưu tiên gói kỹ', datetime('now','localtime'));

INSERT INTO OrderItem (OrderItemId, OrderId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal) VALUES
  (19, 9, 2, 'Bánh kẹp Bò Phô Mai', 69000, 1, 69000),
  (20, 9, 7, 'Khoai Tây Chiên', 29000, 1, 29000),
  (21, 9, 11, 'Trà Chanh Đá', 18000, 1, 18000),
  (22, 10, 16, 'Bánh kẹp Double Cheese', 79000, 1, 79000),
  (23, 10, 8, 'Khoai Tây Phô Mai', 42000, 1, 42000),
  (24, 10, 10, 'Coca 330ml', 15000, 1, 15000),
  (25, 11, 4, 'Bánh kẹp Gà Giòn', 62000, 1, 62000),
  (26, 11, 19, 'Matcha Latte Đá', 32000, 1, 32000),
  (27, 12, 5, 'Cuốn Gà Nướng', 58000, 1, 58000),
  (28, 12, 9, 'Salad Vườn', 39000, 1, 39000),
  (29, 13, 3, 'Bánh kẹp Bò Cay', 72000, 1, 72000),
  (30, 13, 10, 'Coca 330ml', 15000, 1, 15000),
  (31, 14, 17, 'Gà Nướng Mật Ong', 69000, 1, 69000),
  (32, 14, 7, 'Khoai Tây Chiên', 29000, 1, 29000),
  (33, 14, 12, 'Cà Phê Cold Brew', 30000, 1, 30000);

INSERT INTO OrderStatusHistory (HistoryId, OrderId, FromStatus, ToStatus, ChangedByUserId, ChangedAt, Note) VALUES
  (1, 1, 'Mới', 'Đang chuẩn bị', 2, datetime('now','localtime'), NULL),
  (2, 1, 'Đang chuẩn bị', 'Đang giao', 2, datetime('now','localtime'), NULL),
  (3, 1, 'Đang giao', 'Đã giao', 2, datetime('now','localtime'), 'Giao thành công'),
  (4, 2, 'Mới', 'Đang chuẩn bị', 2, datetime('now','localtime'), NULL),
  (5, 3, 'Mới', 'Đã hủy', 2, datetime('now','localtime'), 'Khách yêu cầu');

INSERT INTO Payment (PaymentId, OrderId, Provider, Amount, Status, TransactionRef, PaidAt) VALUES
  (1, 1, 'Tiền mặt', 113000, 'Đã thanh toán', 'CASH-0001', datetime('now','localtime')),
  (2, 2, 'MoMo', 119000, 'Đang xử lý', 'MOMO-0002', datetime('now','localtime')),
  (3, 3, 'Thẻ', 87000, 'Đã hoàn tiền', 'CARD-0003', datetime('now','localtime'));

INSERT INTO AuditLog (AuditLogId, ActorUserid, Action, EntityName, EntityId, OldValuesJson, NewValuesJson, CreatedAt, IpAddress) VALUES
  (1, 1, 'CREATE', 'Product', 2, '{}', '{"Name":"Bánh kẹp Bò Phô Mai"}', datetime('now','localtime'), '127.0.0.1'),
  (2, 2, 'UPDATE', 'Orders', 2, '{"Status":"Mới"}', '{"Status":"Đang chuẩn bị"}', datetime('now','localtime'), '127.0.0.1'),
  (3, 2, 'STATUS_CHANGE', 'Orders', 1, '{"Status":"Đang giao"}', '{"Status":"Đã giao"}', datetime('now','localtime'), '127.0.0.1');

INSERT INTO PromoCode (PromoCodeId, Code, Type, Value, UsageLimit, UsedCount, ExpiresAt, IsActive) VALUES
  (1, 'TET2025', 'Phần trăm', 15, 200, 0, 0, datetime('now','localtime','+120 days'), 1),
  (2, 'FRESH50', 'Cố định', 50000, 100, 0, 0, datetime('now','localtime','+90 days'), 1),
  (3, 'SUMMER10', 'Phần trăm', 10, NULL, 0, 0, datetime('now','localtime','+180 days'), 1);
