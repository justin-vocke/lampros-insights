
-- =========================================
-- AI Analytics / BI Platform Seed Database
-- SQL Server
-- =========================================

-- =========================================
-- DROP TABLES (DEV ONLY)
-- =========================================

IF OBJECT_ID('Payments', 'U') IS NOT NULL DROP TABLE Payments;
IF OBJECT_ID('Invoices', 'U') IS NOT NULL DROP TABLE Invoices;
IF OBJECT_ID('OrderItems', 'U') IS NOT NULL DROP TABLE OrderItems;
IF OBJECT_ID('Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Products', 'U') IS NOT NULL DROP TABLE Products;
IF OBJECT_ID('Customers', 'U') IS NOT NULL DROP TABLE Customers;
IF OBJECT_ID('SalesReps', 'U') IS NOT NULL DROP TABLE SalesReps;
IF OBJECT_ID('Regions', 'U') IS NOT NULL DROP TABLE Regions;

-- =========================================
-- SEED REGIONS
-- =========================================

INSERT INTO Regions (Name)
VALUES
('North America'),
('Europe'),
('Asia Pacific'),
('South America');

-- =========================================
-- SEED SALES REPS
-- =========================================

INSERT INTO SalesReps
(
    FirstName,
    LastName,
    Email,
    RegionId,
    HireDate
)
VALUES
('Sarah', 'Johnson', 'sarah.johnson@company.com', 1, '2021-03-15'),
('Michael', 'Smith', 'michael.smith@company.com', 2, '2020-07-10'),
('David', 'Lee', 'david.lee@company.com', 3, '2022-01-05'),
('Emma', 'Garcia', 'emma.garcia@company.com', 4, '2019-11-20');

-- =========================================
-- SEED CUSTOMERS
-- =========================================

INSERT INTO Customers
(
    Name,
    Email,
    Phone,
    City,
    Country,
    RegionId,
    SalesRepId,
    CreatedAt
)
VALUES
('Acme Corp', 'contact@acme.com', '555-1001', 'New York', 'USA', 1, 1, '2024-01-15'),
('Globex Inc', 'sales@globex.com', '555-1002', 'Toronto', 'Canada', 1, 1, '2024-02-01'),
('TechNova', 'hello@technova.com', '555-1003', 'London', 'UK', 2, 2, '2024-02-10'),
('BlueSky Retail', 'info@bluesky.com', '555-1004', 'Berlin', 'Germany', 2, 2, '2024-03-01'),
('Pacific Solutions', 'contact@pacific.com', '555-1005', 'Tokyo', 'Japan', 3, 3, '2024-03-05'),
('Andes Manufacturing', 'sales@andes.com', '555-1006', 'Santiago', 'Chile', 4, 4, '2024-03-15');

-- =========================================
-- SEED PRODUCTS
-- =========================================

INSERT INTO Products
(
    Name,
    Category,
    SKU,
    UnitPrice,
    IsActive
)
VALUES
('Analytics Dashboard Pro', 'Software', 'SW-1001', 499.99, 1),
('AI Insights Enterprise', 'Software', 'SW-1002', 1299.99, 1),
('Data Integration Suite', 'Software', 'SW-1003', 799.99, 1),
('Cloud Connector', 'Add-On', 'AD-2001', 199.99, 1),
('Premium Support', 'Service', 'SV-3001', 299.99, 1),
('Custom Reporting Module', 'Add-On', 'AD-2002', 399.99, 1);

-- =========================================
-- SEED ORDERS
-- =========================================

INSERT INTO Orders
(
    CustomerId,
    OrderDate,
    Status,
    TotalAmount,
    Notes
)
VALUES
(1, '2025-01-10', 'Completed', 1799.97, 'Enterprise purchase'),
(2, '2025-01-15', 'Completed', 699.98, 'Initial onboarding'),
(3, '2025-02-01', 'Completed', 2499.98, 'Expansion licenses'),
(4, '2025-02-12', 'Pending', 499.99, 'Awaiting payment'),
(5, '2025-03-05', 'Completed', 1599.98, 'Annual renewal'),
(6, '2025-03-18', 'Completed', 999.98, 'Pilot program');

-- =========================================
-- SEED ORDER ITEMS
-- =========================================

INSERT INTO OrderItems
(
    OrderId,
    ProductId,
    Quantity,
    UnitPrice,
    LineTotal
)
VALUES
(1, 2, 1, 1299.99, 1299.99),
(1, 5, 1, 299.99, 299.99),
(1, 4, 1, 199.99, 199.99),

(2, 1, 1, 499.99, 499.99),
(2, 4, 1, 199.99, 199.99),

(3, 2, 1, 1299.99, 1299.99),
(3, 3, 1, 799.99, 799.99),
(3, 6, 1, 399.99, 399.99),

(4, 1, 1, 499.99, 499.99),

(5, 2, 1, 1299.99, 1299.99),
(5, 4, 1, 199.99, 199.99),
(5, 5, 1, 299.99, 299.99),

(6, 3, 1, 799.99, 799.99),
(6, 4, 1, 199.99, 199.99);

-- =========================================
-- SEED INVOICES
-- =========================================

INSERT INTO Invoices
(
    OrderId,
    InvoiceDate,
    DueDate,
    InvoiceAmount,
    Status
)
VALUES
(1, '2025-01-11', '2025-02-10', 1799.97, 'Paid'),
(2, '2025-01-16', '2025-02-15', 699.98, 'Paid'),
(3, '2025-02-02', '2025-03-04', 2499.98, 'Partially Paid'),
(4, '2025-02-13', '2025-03-15', 499.99, 'Open'),
(5, '2025-03-06', '2025-04-05', 1599.98, 'Paid'),
(6, '2025-03-19', '2025-04-18', 999.98, 'Paid');

-- =========================================
-- SEED PAYMENTS
-- =========================================

INSERT INTO Payments
(
    InvoiceId,
    PaymentDate,
    Amount,
    PaymentMethod
)
VALUES
(1, '2025-01-20', 1799.97, 'Wire Transfer'),
(2, '2025-01-25', 699.98, 'Credit Card'),
(3, '2025-02-15', 1000.00, 'Bank Transfer'),
(5, '2025-03-15', 1599.98, 'Credit Card'),
(6, '2025-03-25', 999.98, 'ACH');

-- =========================================
-- SAMPLE ANALYTICS QUERIES
-- =========================================

-- Top customers by revenue
SELECT
    c.Name,
    SUM(o.TotalAmount) AS Revenue
FROM Orders o
JOIN Customers c
    ON c.CustomerId = o.CustomerId
GROUP BY c.Name
ORDER BY Revenue DESC;

-- Revenue by region
SELECT
    r.Name AS Region,
    SUM(o.TotalAmount) AS Revenue
FROM Orders o
JOIN Customers c
    ON c.CustomerId = o.CustomerId
JOIN Regions r
    ON r.RegionId = c.RegionId
GROUP BY r.Name
ORDER BY Revenue DESC;

-- Monthly sales trend
SELECT
    YEAR(OrderDate) AS OrderYear,
    MONTH(OrderDate) AS OrderMonth,
    SUM(TotalAmount) AS Revenue
FROM Orders
GROUP BY
    YEAR(OrderDate),
    MONTH(OrderDate)
ORDER BY
    OrderYear,
    OrderMonth;

-- Top selling products
SELECT
    p.Name,
    SUM(oi.Quantity) AS UnitsSold,
    SUM(oi.LineTotal) AS Revenue
FROM OrderItems oi
JOIN Products p
    ON p.ProductId = oi.ProductId
GROUP BY p.Name
ORDER BY Revenue DESC;
