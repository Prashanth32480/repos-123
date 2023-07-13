IF (DB_NAME() ='ac-dev-identity-db')
BEGIN
	IF NOT EXISTS (Select VersionNumber From app.version Where VersionNumber='1.0' AND Environment='DEV')
	BEGIN
		Insert into app.version (VersionNumber, Environment) values('1.0', 'DEV')
	END
END
ELSE IF (DB_NAME() ='ac-sit-identity-db')
BEGIN
	IF NOT EXISTS (Select VersionNumber From app.version Where VersionNumber='1.0' AND Environment='SIT')
	BEGIN
		Insert into app.version (VersionNumber, Environment) values('1.0', 'SIT')
	END
END
ELSE IF (DB_NAME() ='ac-uat-identity-db')
BEGIN
	IF NOT EXISTS (Select VersionNumber From app.version Where VersionNumber='1.0' AND Environment='UAT')
	BEGIN
		Insert into app.version (VersionNumber, Environment) values('1.0', 'UAT')
	END
END
ELSE IF (DB_NAME() ='ac-prod-identity-db')
BEGIN
	IF NOT EXISTS (Select VersionNumber From app.version Where VersionNumber='1.0' AND Environment='Production')
	BEGIN
		Insert into app.version (VersionNumber, Environment) values('1.0', 'Production')
	END
END