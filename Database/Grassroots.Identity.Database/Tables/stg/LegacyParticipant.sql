CREATE TABLE [stg].[LegacyParticipant]
(
ParticipantGuid			UNIQUEIDENTIFIER	NOT NULL CONSTRAINT [PK_LegacyParticipant] PRIMARY KEY DEFAULT NEWID(),
LegacyPlayerId			INT NOT NULL,
FirstName				NVARCHAR(50) NOT NULL,
LastName				NVARCHAR(50) NOT NULL,
IsNameVisible			BIT	NOT NULL,
IfPlayerAlreadyExists	BIT NOT NULL
)