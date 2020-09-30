#!/bin/bash
#Azure CLI template deployment
az group create --name AZ204-ResourceGroup --location "South Central US"
#az deployment groupcreate \        -- DEPRECATED for future versions
az group deployment create \
  --name AZ204DemoDeployment \
  --resource-group AZ204-ResourceGroup \
  --template-file az204-template.json \
  --parameters @az204-parameter.json