-- ====================================================
-- MIGRATION V2: Split Name, Link Address & Plain Password
-- ====================================================

-- 1. Create the NEW Users table
-- Changed 'password_hash' to 'password' for plain text storage
CREATE TABLE Users_New (
    user_id        INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name     TEXT,       -- New Column
    last_name      TEXT,       -- New Column
    email          TEXT    UNIQUE,
    phone          TEXT    NOT NULL UNIQUE,
    password       TEXT,       -- Renamed from password_hash (Stores plain text now)
    google_id      TEXT UNIQUE,
    role           TEXT    NOT NULL DEFAULT 'customer' CHECK(role IN ('customer', 'admin')),
    avatar         TEXT,
    gender         TEXT,       
    address_id     INTEGER,    -- New Key to connect to Addresses table
    created_at     DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (address_id) REFERENCES Addresses(address_id) ON DELETE SET NULL
);

-- 2. Migrate existing data
INSERT INTO Users_New (user_id, email, phone, password, google_id, role, avatar, created_at, gender, 
                       last_name, 
                       first_name)
SELECT 
    user_id, email, phone, password_hash, google_id, role, avatar, created_at, gender,
    -- Logic to split "Nguyen Van A" into Last: "Nguyen", First: "Van A"
    CASE 
        WHEN instr(full_name, ' ') > 0 THEN substr(full_name, 1, instr(full_name, ' ') - 1)
        ELSE '' 
    END AS last_name,
    CASE 
        WHEN instr(full_name, ' ') > 0 THEN substr(full_name, instr(full_name, ' ') + 1)
        ELSE full_name 
    END AS first_name
FROM Users;

-- 3. Add Mock Users (Plain text passwords)
INSERT INTO Users_New (first_name, last_name, email, phone, password, role, gender) VALUES 
('An', 'Nguyen Van', 'an.nguyen@test.com', '0911222333', '123456', 'customer', 'Nam'),
('Binh', 'Tran Thi', 'binh.tran@test.com', '0944555666', 'password', 'customer', 'Nu'),
('Cuong', 'Le Hung', 'cuong.le@test.com', '0977888999', 'admin123', 'admin', 'Nam');

-- 4. Swap Tables
DROP TABLE Users;
ALTER TABLE Users_New RENAME TO Users;
