-- Stored procedures of the legacy Fleet database.
-- Some business rules live only here (for example "deleted rentals are hidden"),
-- which is why every adapter is checked against the same contract tests.

-- Rental history of one car, newest first. Deleted rentals are not shown.
CREATE PROCEDURE dbo.CarHistoryGet
    @CarId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  r.RentalID                AS RentalId,
            c.FName + N' ' + c.LName  AS Customer,
            r.dtPickup                AS PickedUp,
            r.dtReturn                AS Returned
    FROM    dbo.tbl_Rental r
    JOIN    dbo.tbl_Customer c ON c.CustID = r.CustID
    WHERE   r.CarID = @CarId
      AND   r.DelFlag = 'N'
    ORDER BY r.dtPickup DESC, r.RentalID DESC;
END
GO

-- Cars that can be rented now: status "available" and no open rental.
CREATE PROCEDURE dbo.AvailableCarsGet
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  c.CarID      AS CarId,
            c.CarRegNo   AS RegNo,
            c.ModelDesc  AS Model,
            c.Odometer   AS Odometer
    FROM    dbo.tbl_Car c
    WHERE   c.StatusCd = 'A'
      AND   NOT EXISTS (SELECT 1
                        FROM   dbo.tbl_Rental r
                        WHERE  r.CarID = c.CarID
                          AND  r.dtReturn IS NULL
                          AND  r.DelFlag = 'N')
    ORDER BY c.CarRegNo;
END
GO

-- Registers that a rented car came back.
-- Legacy error convention: the procedure doesn't raise errors for business problems.
-- It sets @ErrMsg and returns a non-zero code. @ErrMsg is NULL when everything went well.
CREATE PROCEDURE dbo.CarReturn
    @RentalId  INT,
    @KmIn      INT,
    @dtReturn  DATETIME = NULL,
    @ErrMsg    NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ErrMsg = NULL;

    DECLARE @CarId INT, @KmOut INT, @dtPickup DATETIME, @dtReturned DATETIME;

    SELECT  @CarId = CarID, @KmOut = KmOut, @dtPickup = dtPickup, @dtReturned = dtReturn
    FROM    dbo.tbl_Rental
    WHERE   RentalID = @RentalId
      AND   DelFlag = 'N';

    IF @CarId IS NULL
    BEGIN
        SET @ErrMsg = N'Rental not found.';
        RETURN 1;
    END

    IF @dtReturned IS NOT NULL
    BEGIN
        SET @ErrMsg = N'The car has already been returned.';
        RETURN 2;
    END

    IF @KmIn < @KmOut
    BEGIN
        SET @ErrMsg = N'Km in can''t be lower than km out.';
        RETURN 3;
    END

    SET @dtReturn = ISNULL(@dtReturn, GETDATE());

    IF @dtReturn < @dtPickup
    BEGIN
        SET @ErrMsg = N'Return time can''t be before pickup time.';
        RETURN 4;
    END

    BEGIN TRANSACTION;

        UPDATE dbo.tbl_Rental
        SET    dtReturn = @dtReturn,
               KmIn     = @KmIn
        WHERE  RentalID = @RentalId;

        UPDATE dbo.tbl_Car
        SET    StatusCd = 'A',
               Odometer = @KmIn
        WHERE  CarID = @CarId;

    COMMIT TRANSACTION;

    RETURN 0;
END
GO
