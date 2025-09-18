CREATE VIEW [dbo].[vwCharacteristicInfo]
AS
SELECT dbo.tblCharacteristic.CharacteristicValue
	,dbo.tlkpVirusCharacteristic.Name AS CharacteristicName
	,dbo.tblIsolate.IsolateId
	,dbo.tlkpVirusCharacteristic.Prefix AS CharacteristicPrefix
	,dbo.tblCharacteristic.CharacteristicId
FROM dbo.tlkpVirusCharacteristic
INNER JOIN dbo.tblIsolate
INNER JOIN dbo.tblCharacteristic ON dbo.tblIsolate.IsolateId = dbo.tblCharacteristic.CharacteristicIsolateId ON dbo.tlkpVirusCharacteristic.Id = dbo.tblCharacteristic.VirusCharacteristicId ORDER BY dbo.tlkpVirusCharacteristic.CharacteristicIndex

GO

CREATE VIEW [dbo].[vwCharacteristicsGetAll]
AS
SELECT  dbo.tblCharacteristic.CharacteristicValue
	,dbo.tlkpVirusCharacteristic.Name AS CharacteristicName
	,dbo.tlkpVirusCharacteristic.Prefix AS CharacteristicPrefix
	,dbo.tlkpVirusCharacteristicDataType.DataType AS CharacteristicTypeName
FROM dbo.tlkpVirusCharacteristic
INNER JOIN dbo.tblCharacteristic ON dbo.tlkpVirusCharacteristic.Id = dbo.tblCharacteristic.VirusCharacteristicId
INNER JOIN dbo.tlkpVirusCharacteristicDataType ON dbo.tlkpVirusCharacteristic.CharacteristicType = dbo.tlkpVirusCharacteristicDataType.Id
ORDER BY dbo.tlkpVirusCharacteristic.Name

GO

CREATE VIEW [dbo].[vwDispatchInfo]
AS
SELECT  dbo.tblDispatch.NoOfAliquots
	,dbo.tblDispatch.PassageNumber
	,dbo.tblDispatch.RecipientName
	,dbo.tblDispatch.RecipientAddress
	,dbo.tblDispatch.ReasonForDispatch
	,dbo.tblDispatch.DispatchedDate
	,dbo.tlkpStaff.Name AS DispatchedByName
	,dbo.tblDispatch.DispatchIsolateID
FROM dbo.tblDispatch
INNER JOIN dbo.tlkpStaff ON dbo.tblDispatch.DispatchedBy = dbo.tlkpStaff.Id
ORDER BY dbo.tblDispatch.DispatchedDate DESC

GO

CREATE VIEW [dbo].[vwViabilityInfo]
AS
SELECT  dbo.tblIsolateViability.DateChecked
	,dbo.tlkpViability.Name AS ViabilityStatus
	,dbo.tlkpStaff.Name AS CheckedByName
	,dbo.tblIsolateViability.IsolateViabilityIsolateID
FROM dbo.tblIsolateViability
INNER JOIN dbo.tlkpStaff ON dbo.tblIsolateViability.CheckedByID = dbo.tlkpStaff.Id
INNER JOIN dbo.tlkpViability ON dbo.tblIsolateViability.Viable = dbo.tlkpViability.Id
ORDER BY dbo.tblIsolateViability.DateChecked DESC

GO
