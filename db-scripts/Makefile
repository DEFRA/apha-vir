# Configuration using jenkins environment variables - updated sql folderfile names
DB_SERVER ?= localhost
DB_NAME ?= bstdb
DB_USER ?= bstdev
DB_PASSWORD ?= $(if $(DB_PASSWORD),$(DB_PASSWORD),$(error DB_PASSWORD is not set))
SQLCMD=sqlcmd -S $(DB_SERVER) -U $(DB_USER) -P $(DB_PASSWORD) -C -b -r1

# Default target
all: post-deploy

# Run post-deployment scripts
post-deploy:
	@echo ">>> Waiting 180 seconds before starting deployment..."
	@sleep 180
	@echo "Running post-deploy scripts..."
	@$(SQLCMD) -d $(DB_NAME) -i 01-tables/modify_tables_post_restore.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_report_people.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_sites_add.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_site_trainee_get.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_trainee_update.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_trainer_has_trained.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_trainer_trainedby_trained.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_training_select.sql
	@$(SQLCMD) -d $(DB_NAME) -i 04-procedures/sp_user_selectall.sql
	@$(SQLCMD) -d $(DB_NAME) -i 99-direct-scripts/tblDataEntry_add_record.sql
	@$(SQLCMD) -d $(DB_NAME) -i 99-direct-scripts/tblUser_add_record.sql
	@echo "Post-deploy scripts completed."

# Help
help:
	@echo "  make post-deploy   - Run post-deployment SQL scripts"
