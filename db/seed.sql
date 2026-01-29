-- =========================================
-- TEAR DOWN (ALWAYS START CLEAN)
-- =========================================

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ProductCatalog')
BEGIN
    ALTER DATABASE ProductCatalog SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ProductCatalog;
END
GO

-- =========================================
-- CREATE DATABASE
-- =========================================

CREATE DATABASE ProductCatalog;
GO

USE ProductCatalog;
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    CREATE TABLE Products (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Title NVARCHAR(200) NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(1000) NULL,
        Quantity INT NOT NULL DEFAULT 0,
        CreatedOnUtc DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        ModifiedOnUtc DATETIMEOFFSET NULL
    );
END
GO

-- Seed sample data
IF NOT EXISTS (SELECT TOP 1 1 FROM Products)
BEGIN
    INSERT INTO Products
    ([Title], [ImageUrl], [Price], [Description], [Quantity])
    VALUES
    ('Wireless Mouse', 'https://cdn.example.com/images/mouse.jpg', 29.99, 'Ergonomic wireless mouse', 150),
    ('Mechanical Keyboard', 'https://cdn.example.com/images/keyboard.jpg', 89.99, 'RGB mechanical keyboard with blue switches', 75),
    ('27-inch Monitor', 'https://cdn.example.com/images/monitor.jpg', 249.99, '144Hz QHD display', 40),
    ('USB-C Hub', 'https://cdn.example.com/images/hub.jpg', 39.99, '7-in-1 USB-C hub', 120),
    ('Laptop Stand', 'https://cdn.example.com/images/stand.jpg', 49.99, 'Aluminum laptop stand', 90);
END
GO