-- Optional default for testing in SSMS with SQLCMD mode
--:setvar ENVIRONMENT Test

-- T-SQL variable to hold the environment
DECLARE @Environment VARCHAR(20);

-- Assign value from SQLCMD variable
SET @Environment = '$(ENVIRONMENT)';

-- Insert into sys info table
INSERT INTO [dbo].[tblSysInfo]
           ([SystemName]
           ,[DatabaseVersion]
           ,[ReleaseDate]
           ,[Environment]
           ,[Live]
           ,[ReleaseNotes])
SELECT 
    'VIR-' + @Environment AS SystemName,
    'SQL 2022' AS DatabaseVersion,
    GETDATE() AS ReleaseDate,
    @Environment AS Environment,
    0 AS Live,
    'VIR modernized to .Net Core, and hosted on AWS' AS ReleaseNotes;
