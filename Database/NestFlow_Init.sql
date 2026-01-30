-- =========================================
-- DATABASE: RENTAL / NESTFLOW SYSTEM
-- =========================================
CREATE DATABASE NestFlowSystem;
GO 
USE NestFlowSystem;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
-- =========================================
-- 1. Users
-- =========================================
CREATE TABLE Users (
    user_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(255),
    phone NVARCHAR(20),
    avatar_url NVARCHAR(500),
    user_type NVARCHAR(20) NOT NULL
        CHECK (user_type IN ('renter', 'landlord', 'admin')),
    is_verified BIT DEFAULT 0,
    status NVARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'banned')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

-- =========================================
-- 2. Properties
-- =========================================
CREATE TABLE Properties (
    property_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    landlord_id BIGINT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    property_type NVARCHAR(30) NOT NULL
        CHECK (property_type IN ('phong_tro', 'chung_cu', 'nha_nguyen_can')),
    address NVARCHAR(255),
    ward NVARCHAR(100),
    district NVARCHAR(100),
    city NVARCHAR(100),
    area DECIMAL(6,2),
    price DECIMAL(12,2),
    deposit DECIMAL(12,2),
    max_occupants INT,
    available_from DATE,
    status NVARCHAR(20) DEFAULT 'available'
        CHECK (status IN ('available', 'rented', 'unavailable')),
    view_count INT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_properties_landlord
        FOREIGN KEY (landlord_id) REFERENCES Users(user_id)
);

-- =========================================
-- 3. PropertyImages
-- =========================================
CREATE TABLE PropertyImages (
    image_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    property_id BIGINT NOT NULL,
    image_url NVARCHAR(500) NOT NULL,
    is_primary BIT DEFAULT 0,
    display_order INT DEFAULT 0,
    uploaded_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_images_property
        FOREIGN KEY (property_id) REFERENCES Properties(property_id)
);

-- =========================================
-- 4. Amenities
-- =========================================
CREATE TABLE Amenities (
    amenity_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    icon_url NVARCHAR(500),
    category NVARCHAR(20)
        CHECK (category IN ('basic', 'furniture', 'security'))
);

-- =========================================
-- 5. PropertyAmenities
-- =========================================
CREATE TABLE PropertyAmenities (
    property_id BIGINT NOT NULL,
    amenity_id BIGINT NOT NULL,
    CONSTRAINT pk_property_amenities PRIMARY KEY (property_id, amenity_id),
    CONSTRAINT fk_pa_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id),
    CONSTRAINT fk_pa_amenity FOREIGN KEY (amenity_id)
        REFERENCES Amenities(amenity_id)
);

-- =========================================
-- 6. Favorites
-- =========================================
CREATE TABLE Favorites (
    favorite_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    property_id BIGINT NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT uq_favorites UNIQUE (user_id, property_id),
    CONSTRAINT fk_favorites_user FOREIGN KEY (user_id)
        REFERENCES Users(user_id),
    CONSTRAINT fk_favorites_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id)
);

-- =========================================
-- 7. Bookings
-- =========================================
CREATE TABLE Bookings (
    booking_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    property_id BIGINT NOT NULL,
    renter_id BIGINT NOT NULL,
    booking_date DATE NOT NULL,
    booking_time TIME NOT NULL,
    status NVARCHAR(20) DEFAULT 'pending'
        CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed')),
    notes NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_bookings_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id),
    CONSTRAINT fk_bookings_renter FOREIGN KEY (renter_id)
        REFERENCES Users(user_id)
);

-- =========================================
-- 8. Rentals
-- =========================================
CREATE TABLE Rentals (
    rental_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    property_id BIGINT NOT NULL,
    landlord_id BIGINT NOT NULL,
    renter_id BIGINT NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NULL,
    monthly_rent DECIMAL(12,2),
    deposit_amount DECIMAL(12,2),
    payment_due_date INT,
    electric_price DECIMAL(10,2),
    water_price DECIMAL(10,2),
    internet_fee DECIMAL(10,2),
    other_fees NVARCHAR(MAX), -- JSON
    status NVARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'expired', 'terminated')),
    termination_date DATE,
    termination_reason NVARCHAR(MAX),
    notes NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_rentals_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id),
    CONSTRAINT fk_rentals_landlord FOREIGN KEY (landlord_id)
        REFERENCES Users(user_id),
    CONSTRAINT fk_rentals_renter FOREIGN KEY (renter_id)
        REFERENCES Users(user_id)
);

-- =========================================
-- 9. Invoices
-- =========================================
CREATE TABLE Invoices (
    invoice_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    rental_id BIGINT NOT NULL,
    invoice_month CHAR(7),
    room_rent DECIMAL(12,2),
    electric_old_reading INT,
    electric_new_reading INT,
    electric_usage INT,
    electric_amount DECIMAL(12,2),
    water_old_reading INT,
    water_new_reading INT,
    water_usage INT,
    water_amount DECIMAL(12,2),
    internet_fee DECIMAL(10,2),
    other_fees NVARCHAR(MAX), -- JSON
    total_amount DECIMAL(14,2),
    due_date DATE,
    payment_date DATE,
    payment_method NVARCHAR(30)
        CHECK (payment_method IN ('cash', 'bank_transfer', 'momo', 'zalopay')),
    payment_proof_url NVARCHAR(500),
    status NVARCHAR(20) DEFAULT 'pending'
        CHECK (status IN ('pending', 'paid', 'overdue', 'cancelled')),
    notes NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_invoices_rental FOREIGN KEY (rental_id)
        REFERENCES Rentals(rental_id)
);

-- =========================================
-- 10. Reviews
-- =========================================
CREATE TABLE Reviews (
    review_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    property_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    rating INT CHECK (rating BETWEEN 1 AND 5),
    comment NVARCHAR(MAX),
    images NVARCHAR(MAX), -- JSON array
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_reviews_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id),
    CONSTRAINT fk_reviews_user FOREIGN KEY (user_id)
        REFERENCES Users(user_id)
);

-- =========================================
-- 11. Reports
-- =========================================
CREATE TABLE Reports (
    report_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    property_id BIGINT NOT NULL,
    reporter_id BIGINT NOT NULL,
    reason NVARCHAR(255),
    description NVARCHAR(MAX),
    status NVARCHAR(20) DEFAULT 'pending'
        CHECK (status IN ('pending', 'reviewing', 'resolved', 'rejected')),
    created_at DATETIME DEFAULT GETDATE(),
    resolved_at DATETIME,
    CONSTRAINT fk_reports_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id),
    CONSTRAINT fk_reports_user FOREIGN KEY (reporter_id)
        REFERENCES Users(user_id)
);

-- =========================================
-- 12. Messages
-- =========================================
CREATE TABLE Messages (
    message_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    sender_id BIGINT NOT NULL,
    receiver_id BIGINT NOT NULL,
    property_id BIGINT NULL,
    content NVARCHAR(MAX) NOT NULL,
    is_read BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_messages_sender FOREIGN KEY (sender_id)
        REFERENCES Users(user_id),
    CONSTRAINT fk_messages_receiver FOREIGN KEY (receiver_id)
        REFERENCES Users(user_id),
    CONSTRAINT fk_messages_property FOREIGN KEY (property_id)
        REFERENCES Properties(property_id)
);

-- =========================================
-- 13. Notifications
-- =========================================
CREATE TABLE Notifications (
    notification_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    type NVARCHAR(20)
        CHECK (type IN ('booking', 'payment', 'message', 'review')),
    title NVARCHAR(255),
    content NVARCHAR(MAX),
    link_url NVARCHAR(500),
    is_read BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_notifications_user FOREIGN KEY (user_id)
        REFERENCES Users(user_id)
);




