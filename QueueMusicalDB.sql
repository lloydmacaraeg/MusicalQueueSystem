-- Musical Instrument Store Queue System Database
-- Run this in MySQL Workbench

CREATE DATABASE IF NOT EXISTS MusicStoreQueue;
USE MusicStoreQueue;

-- Clean up test customer data
-- Run this in MySQL Workbench to reset your test data


-- Delete all test orders (this will cascade properly)
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM Customers;

-- Reset auto-increment counters
ALTER TABLE Customers AUTO_INCREMENT = 1;
ALTER TABLE Orders AUTO_INCREMENT = 1;
ALTER TABLE OrderItems AUTO_INCREMENT = 1;

-- Verify the cleanup
SELECT 'Customers' as TableName, COUNT(*) as RecordCount FROM Customers
UNION ALL
SELECT 'Orders', COUNT(*) FROM Orders
UNION ALL
SELECT 'OrderItems', COUNT(*) FROM OrderItems;
-- Categories Table
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY AUTO_INCREMENT,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255)
);

-- Instruments Table
CREATE TABLE Instruments (
    InstrumentID INT PRIMARY KEY AUTO_INCREMENT,
    InstrumentName VARCHAR(150) NOT NULL,
    CategoryID INT NOT NULL,
    Description TEXT,
    Price DECIMAL(10, 2) NOT NULL,
    StockQuantity INT DEFAULT 0,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Customers Table
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY AUTO_INCREMENT,
    CustomerNumber VARCHAR(50) UNIQUE NOT NULL,
    CustomerName VARCHAR(150) NOT NULL,
    DateAdded DATETIME DEFAULT CURRENT_TIMESTAMP,
    Status ENUM('In Queue', 'Completed') DEFAULT 'In Queue'
);

-- Orders Table
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY AUTO_INCREMENT,
    CustomerID INT NOT NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(10, 2) DEFAULT 0,
    Status ENUM('Pending', 'Completed') DEFAULT 'Pending',
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

-- Order Items Table
CREATE TABLE OrderItems (
    OrderItemID INT PRIMARY KEY AUTO_INCREMENT,
    OrderID INT NOT NULL,
    InstrumentID INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    Sales DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (InstrumentID) REFERENCES Instruments(InstrumentID)
);

-- Insert Sample Categories
INSERT INTO Categories (CategoryName, Description) VALUES
('Drums', 'Percussion instruments including drum kits and accessories'),
('Guitars', 'String instruments - acoustic, electric, and bass guitars'),
('Keyboards', 'Piano, synthesizers, and electronic keyboards'),
('Brass', 'Trumpet, trombone, saxophone, and other brass instruments'),
('Woodwinds', 'Flute, clarinet, oboe, and similar instruments'),
('Strings', 'Violin, viola, cello, and other orchestral strings');

-- Insert Sample Instruments
INSERT INTO Instruments (InstrumentName, CategoryID, Description, Price, StockQuantity) VALUES
-- Drums (CategoryID = 1)
('Pearl Export Series Drum Kit', 1, '5-piece drum set with cymbals', 699.99, 10),
('Yamaha Stage Custom Drums', 1, 'Professional drum kit', 899.99, 5),
('Snare Drum - Ludwig', 1, '14-inch chrome snare', 249.99, 15),
('Drum Sticks Pro Pack', 1, 'Set of 12 pairs', 39.99, 50),

-- Guitars (CategoryID = 2)
('Fender Stratocaster', 2, 'Electric guitar - Sunburst finish', 1299.99, 8),
('Gibson Les Paul', 2, 'Premium electric guitar', 2499.99, 3),
('Yamaha Acoustic Guitar', 2, 'Steel string acoustic', 399.99, 12),
('Ibanez Bass Guitar', 2, '4-string electric bass', 549.99, 6),

-- Keyboards (CategoryID = 3)
('Yamaha P-125 Digital Piano', 3, '88-key weighted keyboard', 649.99, 7),
('Roland Synthesizer', 3, 'Professional synth workstation', 1899.99, 4),
('Casio Keyboard CT-S300', 3, 'Portable 61-key keyboard', 199.99, 20),

-- Brass (CategoryID = 4)
('Yamaha Trumpet YTR-2330', 4, 'Student trumpet', 499.99, 10),
('Bach Stradivarius Trumpet', 4, 'Professional trumpet', 2199.99, 2),
('Conn Trombone', 4, 'Tenor trombone', 799.99, 5),

-- Woodwinds (CategoryID = 5)
('Yamaha Flute YFL-222', 5, 'Student flute', 399.99, 8),
('Buffet Clarinet', 5, 'Professional B-flat clarinet', 1299.99, 4),

-- Strings (CategoryID = 6)
('Stentor Violin 4/4', 6, 'Full-size student violin', 299.99, 10),
('Cecilio Cello', 6, 'Full-size cello with bow', 599.99, 5);

-- Create Views for easier data retrieval
CREATE VIEW QueueSummary AS
SELECT 
    c.CategoryName,
    COUNT(DISTINCT o.CustomerID) AS TotalOrders,
    COUNT(DISTINCT CASE WHEN cust.Status = 'Completed' THEN o.CustomerID END) AS CompletedOrders,
    COUNT(DISTINCT CASE WHEN cust.Status = 'In Queue' THEN o.CustomerID END) AS InQueue
FROM Categories c
LEFT JOIN Instruments i ON c.CategoryID = i.CategoryID
LEFT JOIN OrderItems oi ON i.InstrumentID = oi.InstrumentID
LEFT JOIN Orders o ON oi.OrderID = o.OrderID
LEFT JOIN Customers cust ON o.CustomerID = cust.CustomerID
GROUP BY c.CategoryID, c.CategoryName;

CREATE VIEW CustomerOrderDetails AS
SELECT 
    c.CustomerNumber,
    c.CustomerName,
    c.Status AS CustomerStatus,
    o.OrderID,
    o.OrderDate,
    i.InstrumentName,
    cat.CategoryName,
    oi.Quantity,
    oi.Price,
    oi.Sales,
    o.TotalAmount
FROM Customers c
JOIN Orders o ON c.CustomerID = o.CustomerID
JOIN OrderItems oi ON o.OrderID = oi.OrderID
JOIN Instruments i ON oi.InstrumentID = i.InstrumentID
JOIN Categories cat ON i.CategoryID = cat.CategoryID
ORDER BY c.CustomerID, o.OrderID;