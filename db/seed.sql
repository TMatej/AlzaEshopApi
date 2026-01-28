USE ProductCatalog;
GO

-- Insert only if table is empty
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products
    ([Title], [ImageUrl], [Price], [Description], [Quantity])
    VALUES
    ('Wireless Mouse', 'https://cdn.example.com/images/mouse.jpg', 29.99, 'Ergonomic wireless mouse', 150),
    ('Mechanical Keyboard', 'https://cdn.example.com/images/keyboard.jpg', 89.99, 'RGB keyboard', 75),
    ('27-inch Monitor', 'https://cdn.example.com/images/monitor.jpg', 249.99, '144Hz QHD display', 40),
    ('USB-C Hub', 'https://cdn.example.com/images/hub.jpg', 39.99, '7-in-1 USB-C hub', 120),
    ('Laptop Stand', 'https://cdn.example.com/images/stand.jpg', 49.99, 'Aluminum laptop stand', 90);
END
GO