-- Sample data for local development. All names are made up.

DECLARE @Anna INT, @Jonas INT, @Maria INT;
DECLARE @Hatchback INT, @Estate INT, @Van INT;

INSERT INTO dbo.tbl_Customer (FName, LName, Email) VALUES (N'Anna', N'Berg', 'anna.berg@example.com');
SET @Anna = SCOPE_IDENTITY();
INSERT INTO dbo.tbl_Customer (FName, LName, Email) VALUES (N'Jonas', N'Holm', NULL);
SET @Jonas = SCOPE_IDENTITY();
INSERT INTO dbo.tbl_Customer (FName, LName, Email) VALUES (N'Maria', N'Lund', 'maria.lund@example.com');
SET @Maria = SCOPE_IDENTITY();

INSERT INTO dbo.tbl_Car (CarRegNo, ModelDesc, StatusCd, Odometer) VALUES ('KX-1042', N'Compact hatchback, manual', 'R', 42150);
SET @Hatchback = SCOPE_IDENTITY();
INSERT INTO dbo.tbl_Car (CarRegNo, ModelDesc, StatusCd, Odometer) VALUES ('KX-2077', N'Family estate, automatic', 'A', 18300);
SET @Estate = SCOPE_IDENTITY();
INSERT INTO dbo.tbl_Car (CarRegNo, ModelDesc, StatusCd, Odometer) VALUES ('VN-0310', N'Cargo van, 3 seats', 'S', 77020);
SET @Van = SCOPE_IDENTITY();

INSERT INTO dbo.tbl_Rental (CarID, CustID, dtPickup, dtReturn, KmOut, KmIn, DelFlag) VALUES
    (@Hatchback, @Anna,  '2026-03-02T09:00:00', '2026-03-05T17:30:00', 40100, 40620, 'N'),
    (@Hatchback, @Jonas, '2026-04-11T08:15:00', '2026-04-12T19:00:00', 40620, 41380, 'N'),
    (@Hatchback, @Maria, '2026-05-20T10:00:00', NULL,                  41380, NULL,  'Y'), -- entered by mistake, deleted
    (@Hatchback, @Maria, '2026-05-20T10:05:00', NULL,                  42150, NULL,  'N'), -- the car is out now
    (@Estate,    @Anna,  '2026-02-14T12:00:00', '2026-02-16T12:00:00', 17900, 18300, 'N'),
    (@Van,       @Jonas, '2026-01-08T07:30:00', '2026-01-08T18:45:00', 76800, 77020, 'N');
GO
