CREATE DATABASE Warehouse;
GO
USE Warehouse;
GO

CREATE TABLE Product (
    IdProduct      INT          IDENTITY PRIMARY KEY,
    Name           NVARCHAR(50) NOT NULL,
    Description    NVARCHAR(100),
    Price          DECIMAL(18,2) NOT NULL
);

CREATE TABLE Warehouse (
    IdWarehouse    INT          IDENTITY PRIMARY KEY,
    Name           NVARCHAR(50) NOT NULL,
    Address        NVARCHAR(100)
);

CREATE TABLE [Order] (
    IdOrder        INT          IDENTITY PRIMARY KEY,
    IdProduct      INT NOT NULL REFERENCES Product(IdProduct),
    Amount         INT NOT NULL,
    CreatedAt      DATETIME NOT NULL,
    FulfilledAt    DATETIME NULL
);

CREATE TABLE Product_Warehouse (
    IdProduct_Warehouse INT IDENTITY PRIMARY KEY,
    IdWarehouse         INT NOT NULL REFERENCES Warehouse(IdWarehouse),
    IdProduct           INT NOT NULL REFERENCES Product(IdProduct),
    IdOrder             INT NOT NULL REFERENCES [Order](IdOrder),
    Amount              INT NOT NULL,
    Price               DECIMAL(18,2) NOT NULL,
    CreatedAt           DATETIME NOT NULL
);

-- Seed
INSERT Product  (Name, Description, Price) VALUES
('Keyboard','Mechanical',150), ('Mouse','Wireless',50);

INSERT Warehouse(Name, Address) VALUES
('Central','123 Main St'), ('Spare','25 West Rd');

INSERT [Order](IdProduct, Amount, CreatedAt, FulfilledAt) VALUES
(1, 100, DATEADD(day,-2,SYSUTCDATETIME()), NULL),
(2,  50, DATEADD(day,-1,SYSUTCDATETIME()), NULL);
