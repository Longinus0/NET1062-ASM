CREATE TABLE IF NOT EXISTS Users (
  UserId INTEGER PRIMARY KEY AUTOINCREMENT,
  FullName TEXT NOT NULL,
  Email TEXT NOT NULL UNIQUE COLLATE NOCASE,
  Phone TEXT NULL,
  PasswordHash TEXT NOT NULL,
  Address TEXT NULL,
  AvatarUrl TEXT NULL,
  IsActive INTEGER NOT NULL DEFAULT 0 CHECK (IsActive IN (0,1)),
  ForcePasswordReset INTEGER NOT NULL DEFAULT 0 CHECK (ForcePasswordReset IN (0,1)),
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
  UpdatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);

CREATE TRIGGER IF NOT EXISTS trg_users_updatedat
AFTER UPDATE ON Users
FOR EACH ROW
WHEN NEW.UpdatedAt = OLD.UpdatedAt
BEGIN
  UPDATE Users
  SET UpdatedAt = datetime('now', 'localtime')
  WHERE UserId = OLD.UserId;
END;


CREATE TABLE IF NOT EXISTS Role (
  RoleId INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS UserRole (
  UserId INTEGER REFERENCES Users(UserId),
  RoleId INTEGER REFERENCES Role(RoleId)
);

CREATE TRIGGER IF NOT EXISTS trg_users_default_role
AFTER INSERT ON Users
FOR EACH ROW
BEGIN
  INSERT INTO UserRole (UserId, RoleId)
  SELECT NEW.UserId, 3
  WHERE NOT EXISTS (
    SELECT 1 FROM UserRole WHERE UserId = NEW.UserId
  );
END;

CREATE TRIGGER IF NOT EXISTS trg_orders_init_status
AFTER INSERT ON Orders
FOR EACH ROW
BEGIN
  INSERT INTO OrderStatusHistory (OrderId, FromStatus, ToStatus, ChangedByUserId, ChangedAt, Note)
  VALUES (NEW.OrderId, NEW.Status, NEW.Status, NULL, datetime('now', 'localtime'), 'Tạo đơn hàng');
END;

CREATE TABLE IF NOT EXISTS ExternalLogin (
  ExternalLoginId INTEGER PRIMARY KEY AUTOINCREMENT,
  UserId INTEGER REFERENCES Users(UserId),
  Provider TEXT NOT NULL CHECK (Provider in ('Facebook', 'Google')),
  ProviderId INTEGER NOT NULL DEFAULT 1 CHECK (ProviderId in (1,2)),
  Email TEXT NOT NULL UNIQUE,
  CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Category (
  CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Description TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Product (
  ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
  CategoryId INTEGER REFERENCES Category(CategoryId),
  Name TEXT NOT NULL,
  Description TEXT NOT NULL,
  Price REAL NOT NULL,
  ImageUrl TEXT NOT NULL,
  TopicTag TEXT NOT NULL,
  IsAvailable INTEGER NOT NULL DEFAULT 0 CHECK (IsAvailable in (0,1)),
  StockQty INTEGER NOT NULL DEFAULT 0,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
  UpdatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS ProductTag (
  TagId INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ProductTagMap (
  ProductId INTEGER REFERENCES Product(ProductId),
  TagId INTEGER REFERENCES ProductTag(TagId)
);

CREATE TABLE IF NOT EXISTS Combo (
  ComboId INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Description TEXT NOT NULL,
  Price REAL NOT NULL,
  ImageUrl TEXT NULL,
  IsActive INTEGER NOT NULL DEFAULT 0 CHECK (IsActive in (0,1))
);

CREATE TABLE IF NOT EXISTS ComboItem (
  ComboId INTEGER REFERENCES Combo(ComboId),
  ProductId INTEGER REFERENCES Product(ProductId),
  Quantity INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Orders (
  OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
  UserId INTEGER REFERENCES Users(UserId),
  OrderCode TEXT NOT NULL,
  Status TEXT NOT NULL DEFAULT 'Mới' CHECK (Status in ('Mới', 'Đang chuẩn bị', 'Đang giao', 'Đã giao', 'Đã hủy')),
  SubTotal REAL NOT NULL,
  DiscountTotal REAL NOT NULL,
  DeliveryFee REAL NOT NULL,
  GrandTotal REAL NOT NULL,
  PaymentStatus TEXT NOT NULL DEFAULT 'Chưa thanh toán' CHECK (PaymentStatus in ('Chưa thanh toán', 'Đang xử lý', 'Đã thanh toán', 'Thất bại', 'Đã hoàn tiền')), 
  PaymentMethod TEXT NOT NULL DEFAULT 'Tiền mặt' CHECK (PaymentMethod in ('Tiền mặt', 'Thẻ', 'MoMo', 'VNPay', 'ZaloPay')),
  PromoCode TEXT NULL,
  IdempotencyKey TEXT NULL,
  Note TEXT NULL,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS OrderItem (
  OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
  OrderId INTEGER REFERENCES Orders(OrderId),
  ProductId INTEGER REFERENCES Product(ProductId),
  ProductNameSnapshot TEXT NOT NULL,
  UnitPriceSnapshot REAL NOT NULL,
  Quantity INTEGER NOT NULL,
  LineTotal REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS OrderStatusHistory (
  HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
  OrderId INTEGER REFERENCES Orders(OrderId),
  FromStatus TEXT NOT NULL CHECK (FromStatus in ('Mới', 'Đang chuẩn bị', 'Đang giao', 'Đã giao', 'Đã hủy')),
  ToStatus TEXT NOT NULL CHECK (ToStatus in ('Mới', 'Đang chuẩn bị', 'Đang giao', 'Đã giao', 'Đã hủy')),
  ChangedByUserId INTEGER REFERENCES Users(UserId),
  ChangedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
  Note TEXT NULL
);

CREATE TABLE IF NOT EXISTS Payment (
  PaymentId INTEGER PRIMARY KEY AUTOINCREMENT,
  OrderId INTEGER REFERENCES Orders(OrderId),
  Provider TEXT NOT NULL CHECK (Provider in ('Tiền mặt', 'MoMo', 'VNPay', 'ZaloPay', 'Thẻ')),
  Amount REAL NOT NULL,
  Status TEXT NOT NULL CHECK (Status in ('Đang xử lý', 'Đã thanh toán', 'Thất bại', 'Đã hủy', 'Đã hoàn tiền')),
  TransactionRef TEXT NOT NULL,
  PaidAt TEXT DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS AuditLog (
  AuditLogId INTEGER PRIMARY KEY AUTOINCREMENT,
  ActorUserid INTEGER REFERENCES Users(UserId),
  Action TEXT NOT NULL CHECK (Action in ('CREATE', 'UPDATE', 'DELETE', 'STATUS_CHANGE', 'LOGIN', 'LOGOUT')),
  EntityName TEXT NOT NULL,
  EntityId INTEGER NOT NULL,
  OldValuesJson TEXT NOT NULL,
  NewValuesJson TEXT NOT NULL,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
  IpAddress TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS PromoCode (
  PromoCodeId INTEGER PRIMARY KEY AUTOINCREMENT,
  Code TEXT NOT NULL UNIQUE,
  Type TEXT NOT NULL CHECK (Type in ('Phần trăm', 'Cố định')),
  Value REAL NOT NULL,
  UsageLimit INTEGER NULL,
  UsedCount INTEGER NOT NULL DEFAULT 0,
  ExpiresAt TEXT NOT NULL,
  IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER IF NOT EXISTS trg_product_updatedat
AFTER UPDATE ON Product
FOR EACH ROW
WHEN NEW.UpdatedAt = OLD.UpdatedAt
BEGIN
  UPDATE Product
  SET UpdatedAt = datetime('now', 'localtime')
  WHERE ProductId = OLD.ProductId;
END;

CREATE UNIQUE INDEX IF NOT EXISTS uq_userrole_user_role ON UserRole(UserId, RoleId);
CREATE UNIQUE INDEX IF NOT EXISTS uq_userrole_user ON UserRole(UserId);
CREATE UNIQUE INDEX IF NOT EXISTS uq_producttagmap_product_tag ON ProductTagMap(ProductId, TagId);
CREATE UNIQUE INDEX IF NOT EXISTS uq_comboitem_combo_product ON ComboItem(ComboId, ProductId);
CREATE UNIQUE INDEX IF NOT EXISTS uq_orders_ordercode ON Orders(OrderCode);
CREATE UNIQUE INDEX IF NOT EXISTS uq_orders_idempotency ON Orders(IdempotencyKey) WHERE IdempotencyKey IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_payment_transactionref ON Payment(TransactionRef);
CREATE UNIQUE INDEX IF NOT EXISTS uq_payment_order ON Payment(OrderId);

CREATE INDEX IF NOT EXISTS idx_userrole_userid ON UserRole(UserId);
CREATE INDEX IF NOT EXISTS idx_userrole_roleid ON UserRole(RoleId);
CREATE INDEX IF NOT EXISTS idx_externallogin_userid ON ExternalLogin(UserId);
CREATE INDEX IF NOT EXISTS idx_product_categoryid ON Product(CategoryId);
CREATE INDEX IF NOT EXISTS idx_producttagmap_productid ON ProductTagMap(ProductId);
CREATE INDEX IF NOT EXISTS idx_producttagmap_tagid ON ProductTagMap(TagId);
CREATE INDEX IF NOT EXISTS idx_comboitem_comboid ON ComboItem(ComboId);
CREATE INDEX IF NOT EXISTS idx_comboitem_productid ON ComboItem(ProductId);
CREATE INDEX IF NOT EXISTS idx_orders_userid ON Orders(UserId);
CREATE INDEX IF NOT EXISTS idx_orderitem_orderid ON OrderItem(OrderId);
CREATE INDEX IF NOT EXISTS idx_orderitem_productid ON OrderItem(ProductId);
CREATE INDEX IF NOT EXISTS idx_orderstatushistory_orderid ON OrderStatusHistory(OrderId);
CREATE INDEX IF NOT EXISTS idx_orderstatushistory_changedby ON OrderStatusHistory(ChangedByUserId);
CREATE INDEX IF NOT EXISTS idx_payment_orderid ON Payment(OrderId);
CREATE INDEX IF NOT EXISTS idx_auditlog_actoruserid ON AuditLog(ActorUserid);
