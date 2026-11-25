-- Fleet: the legacy database.
--
-- This schema plays the role of a database that other applications, reports and jobs still use,
-- so it can't be changed. The naming is inconsistent on purpose, the way old schemas often are:
-- tbl_ prefixes, short column names, "dt" date prefixes, CHAR(1) flags and soft deletes.
--
-- The scripts don't contain USE statements, so they can be applied to any database name.

CREATE TABLE dbo.tbl_Customer
(
    CustID      INT IDENTITY(1, 1) NOT NULL,
    FName       NVARCHAR(50)       NOT NULL,
    LName       NVARCHAR(50)       NOT NULL,
    Email       VARCHAR(255)       NULL,
    dtCreated   DATETIME           NOT NULL CONSTRAINT DF_tbl_Customer_dtCreated DEFAULT (GETDATE()),
    CONSTRAINT PK_tbl_Customer PRIMARY KEY CLUSTERED (CustID)
);
GO

CREATE TABLE dbo.tbl_Car
(
    CarID       INT IDENTITY(1, 1) NOT NULL,
    CarRegNo    VARCHAR(10)        NOT NULL,
    ModelDesc   NVARCHAR(100)      NOT NULL,
    StatusCd    CHAR(1)            NOT NULL CONSTRAINT DF_tbl_Car_StatusCd DEFAULT ('A'), -- A = available, R = rented, S = in service
    Odometer    INT                NOT NULL CONSTRAINT DF_tbl_Car_Odometer DEFAULT (0),   -- km
    CONSTRAINT PK_tbl_Car PRIMARY KEY CLUSTERED (CarID),
    CONSTRAINT UQ_tbl_Car_CarRegNo UNIQUE (CarRegNo)
);
GO

CREATE TABLE dbo.tbl_Rental
(
    RentalID    INT IDENTITY(1, 1) NOT NULL,
    CarID       INT                NOT NULL,
    CustID      INT                NOT NULL,
    dtPickup    DATETIME           NOT NULL,
    dtReturn    DATETIME           NULL,     -- NULL while the car is out
    KmOut       INT                NOT NULL,
    KmIn        INT                NULL,
    DelFlag     CHAR(1)            NOT NULL CONSTRAINT DF_tbl_Rental_DelFlag DEFAULT ('N'), -- 'Y' = deleted. Rows are never physically deleted.
    CONSTRAINT PK_tbl_Rental PRIMARY KEY CLUSTERED (RentalID),
    CONSTRAINT FK_tbl_Rental_tbl_Car FOREIGN KEY (CarID) REFERENCES dbo.tbl_Car (CarID),
    CONSTRAINT FK_tbl_Rental_tbl_Customer FOREIGN KEY (CustID) REFERENCES dbo.tbl_Customer (CustID)
);
GO

CREATE INDEX IX_tbl_Rental_CarID_dtPickup ON dbo.tbl_Rental (CarID, dtPickup);
GO
