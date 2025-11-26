-- Danh mục
INSERT INTO Categories (name, display_order) VALUES 
('Burger', 1),
('Gà rán', 2),
('Mì Ý', 3),
('Thức uống', 4),
('Tráng miệng', 5);

-- Món lẻ (đã bỏ hết N)
INSERT INTO Products (category_id, name, price, image, is_available) VALUES
(1, 'Burger Bò Phô Mai', 69000, 'burger1.jpg', 1),
(1, 'Burger Gà Giòn', 59000, 'burger2.jpg', 1),
(2, 'Gà Rán 2 miếng', 75000, 'ga1.jpg', 1),
(2, 'Gà Rán Cay 1 miếng', 39000, 'ga_spicy.jpg', 1),
(3, 'Mì Ý Bò Bằm', 65000, 'spaghetti.jpg', 1),
(4, 'Coca Cola', 15000, 'coke.jpg', 1),
(4, 'Pepsi', 15000, 'pepsi.jpg', 1),
(4, 'Trà Đào', 25000, 'tradao.jpg', 1),
(5, 'Khoai Tây Chiên', 29000, 'fries.jpg', 1),
(5, 'Bánh Mì Kẹp Kem', 20000, 'icecream.jpg', 1);

-- Combo mẫu
INSERT INTO Combos (name, price, image, is_available) VALUES 
('Combo Couple', 179000, 'combo1.jpg', 1),
('Combo Gia Đình 4 Người', 399000, 'combo_family.jpg', 1);

-- Chi tiết combo 1
INSERT INTO ComboDetails (combo_id, product_id, quantity) VALUES
(1, 1, 1),  -- Burger Bò Phô Mai
(1, 2, 1),  -- Burger Gà Giòn
(1, 3, 2),  -- Gà Rán 2 miếng → 2 phần
(1, 9, 1),  -- Khoai Tây Chiên
(1, 6, 2);  -- Coca Cola x2

-- Admin mẫu (mật khẩu ví dụ: admin123 → hash bằng bcrypt nếu dùng thực tế)
INSERT INTO Users (full_name, phone, password_hash, role) VALUES
('Quản trị viên', '0909999999', '$2b$12$z8z8z8z8z8z8z8z8z8z8z8z8z8', 'admin');

-- Khách hàng mẫu
INSERT INTO Users (full_name, phone, email, password_hash, role) VALUES
('Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', '$2b$12$abcabcabcabcabcabcabcabc', 'customer');
