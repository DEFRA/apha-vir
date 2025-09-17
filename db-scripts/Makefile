# Configuration using jenkins environment variables - updated sql folderfile names
DB_SERVER ?= localhost
ENVIRONMENT ?= dev 
DB_NAME ?= virdb
DB_USER ?= virdev
DB_PASSWORD ?= $(if $(DB_PASSWORD),$(DB_PASSWORD),$(error DB_PASSWORD is not set))
SQLCMD=sqlcmd -S $(DB_SERVER) -U $(DB_USER) -P $(DB_PASSWORD) -C -b -r1

# Default target
all: post-deploy

# Run post-deployment scripts
post-deploy:
	@echo ">>> Waiting 180 seconds before starting deployment..."
	@sleep 180
	@echo "Running post-deploy scripts..."
	
	@$(SQLCMD) -d $(DB_NAME) -i 01-tables/modify-tables-post-restore.sql
	
	@$(SQLCMD) -d $(DB_NAME) -i 02-views/modify-views-post-restore.sql
	
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spCharacteristicDelete.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spCharacteristicInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spCharacteristicUpdate.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spDispatchDelete.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spDispatchInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spDispatchUpdate.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateCharacteristicsInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateDelete.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateRelocateByIsolate.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateUpdate.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateViabilityDelete.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateViabilityInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spIsolateViabilityUpdate.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spLogCharacteristicsGetBySearch.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spLogDispatchGetBySearch.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spLogIsolateGetBySearch.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spLogIsolateViabilityGetBySearch.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spLogSampleGetBySearch.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spLogSubmissionGetBySearch.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spSampleDelete.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spSampleInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spSampleUpdate.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spSubmissionDelete.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spSubmissionInsert.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/spSubmissionUpdate.sql

	@$(SQLCMD) -d $(DB_NAME) -i 99-direct-scripts/data_tblSysInfo.sql -v environment=$(ENVIRONMENT)
	@echo "Post-deploy scripts completed."

# Help
help:
	@echo "  make post-deploy   - Run post-deployment SQL scripts"
