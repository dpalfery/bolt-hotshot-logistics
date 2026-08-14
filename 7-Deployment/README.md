# Deployment

Terraform under `Azure-deploy/` is the infrastructure-as-code for this repository.
Docker Compose files and deployment scripts also live here. These assets deploy
the full system and show how Clean Architecture layers can be hosted in
containers or in the cloud.

Agents must not run `terraform apply` or `terraform destroy` locally. Apply runs
through GitHub Actions.

