-- fastfood.db
PRAGMA foreign_keys = ON;

-- 1. Người dùng (Customer + Admin + Guest không lưu)
CREATE TABLE Users (
    user_id        INTEGER PRIMARY KEY AUTOINCREMENT,
    full_name      TEXT    NOT NULL,
    email          TEXT    UNIQUE,
    phone          TEXT    NOT NULL UNIQUE,
    password_hash  TEXT,                              -- NULL nếu đăng nhập Google
    google_id      TEXT UNIQUE,                       -- cho đăng nhập Google
    role           TEXT    NOT NULL DEFAULT 'customer' CHECK(role IN ('customer', 'admin')),
    avatar         TEXT,
    created_at     DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Địa chỉ giao hàng của khách
CREATE TABLE Addresses (
    address_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id        INTEGER NOT NULL,
    recipient_name TEXT    NOT NULL,
    phone          TEXT    NOT NULL,
    street         TEXT    NOT NULL,
    ward           TEXT,
    district       TEXT    NOT NULL,
    city           TEXT    NOT NULL DEFAULT 'Hồ Chí Minh',
    is_default     INTEGER NOT NULL DEFAULT 0 CHECK(is_default IN (0,1)),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- 3. Danh mục món ăn
CREATE TABLE Categories (
    category_id    INTEGER PRIMARY KEY AUTOINCREMENT,
    name           TEXT    NOT NULL UNIQUE,
    image          TEXT,
    display_order  INTEGER DEFAULT 0
);

-- 4. Món ăn riêng lẻ
CREATE TABLE Products (
    product_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    category_id    INTEGER NOT NULL,
    name           TEXT    NOT NULL,
    description    TEXT,
    price          INTEGER NOT NULL CHECK(price >= 0),   -- lưu theo đồng (VND)
    image          TEXT,
    is_available   INTEGER NOT NULL DEFAULT 1 CHECK(is_available IN (0,1)),
    FOREIGN KEY (category_id) REFERENCES Categories(category_id) ON DELETE CASCADE
);

-- 5. Combo
CREATE TABLE Combos (
    combo_id       INTEGER PRIMARY KEY AUTOINCREMENT,
    name           TEXT    NOT NULL,
    description    TEXT,
    price          INTEGER NOT NULL CHECK(price >= 0),
    image          TEXT,
    is_available   INTEGER NOT NULL DEFAULT 1
);

-- Chi tiết món trong combo
CREATE TABLE ComboDetails (
    combo_id       INTEGER NOT NULL,
    product_id     INTEGER NOT NULL,
    quantity       INTEGER NOT NULL DEFAULT 1 CHECK(quantity > 0),
    PRIMARY KEY (combo_id, product_id),
    FOREIGN KEY (combo_id)   REFERENCES Combos(combo_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);

-- 6. Giỏ hàng / Đơn hàng
CREATE TABLE Orders (
    order_id       INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id        INTEGER,                            -- NULL nếu khách vãng lai (Guest)
    guest_name     TEXT,                               -- bắt buộc nếu user_id NULL
    guest_phone    TEXT,                               -- bắt buộc nếu user_id NULL
    address_id     INTEGER,                            -- có thể NULL nếu nhập tay
    full_address   TEXT    NOT NULL,                   -- luôn lưu địa chỉ đầy đủ lúc đặt
    payment_method TEXT    NOT NULL CHECK(payment_method IN ('cod', 'momo', 'zalo_pay', 'vnpay', 'bank_card')),
    total_amount   INTEGER NOT NULL CHECK(total_amount >= 0),
    status         TEXT    NOT NULL DEFAULT 'pending' 
                   CHECK(status IN ('pending', 'confirmed', 'preparing', 'delivering', 'delivered', 'cancelled')),
    note           TEXT,
    created_at     DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at     DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id)    REFERENCES Users(user_id) ON DELETE SET NULL,
    FOREIGN KEY (address_id) REFERENCES Addresses(address_id)
);

-- Chi tiết đơn hàng (có thể là món lẻ hoặc combo)
CREATE TABLE OrderDetails (
    order_detail_id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id        INTEGER NOT NULL,
    -- Có thể là món lẻ HOẶC combo, không cùng lúc
    product_id      INTEGER,          -- NULL nếu là combo
    combo_id        INTEGER,          -- NULL nếu là món lẻ
    quantity        INTEGER NOT NULL CHECK(quantity > 0),
    unit_price      INTEGER NOT NULL, -- giá tại thời điểm đặt
    FOREIGN KEY (order_id)   REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(product_id),
    FOREIGN KEY (combo_id)   REFERENCES Combos(combo_id),
    CONSTRAINT chk_one_type CHECK (
        (product_id IS NOT NULL AND combo_id IS NULL) OR
        (product_id IS NULL AND combo_id IS NOT NULL)
    )
);

-- 7. Thanh toán (nếu cần lưu lịch sử giao dịch từ cổng)
CREATE TABLE Payments (
    payment_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id       INTEGER NOT NULL UNIQUE,
    method         TEXT    NOT NULL,
    amount         INTEGER NOT NULL,
    transaction_id TEXT,                              -- mã giao dịch từ MoMo, VNPay...
    status         TEXT    NOT NULL DEFAULT 'pending',
    created_at     DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE
);

-- Index để tìm kiếm nhanh
CREATE INDEX idx_orders_user ON Orders(user_id);
CREATE INDEX idx_orders_status ON Orders(status);
CREATE INDEX idx_products_category ON Products(category_id);
CREATE INDEX idx_orderdetails_order ON OrderDetails(order_id);
