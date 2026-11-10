# ListLift — handoff and next steps

## Current status

- Local account, draft, photo, description, eBay category, and administrator settings flows were previously built and manually exercised by the user.
- The user confirmed a clean full-solution build and 37 passing tests after the latest form-recovery and quantity changes.
- Migration `20261110142117_AddListingDraftQuantity` was generated and applied to the user's Development LocalDB. The second attempt to add it correctly reported the migration name already exists; `database update` confirmed the database was current.
- Unsaved form text is being recovered from tab-scoped browser storage after reload and sign-in. It is cleared on sign-out; unuploaded photo selections are not retained.
- Draft quantity is represented in the domain, API, and client. It defaults to 1 and is limited to 1–999,999.
- Local Git repository is on `main` with commits `Prepare ListLift for GitHub` and `Update GitHub readiness documentation`. Public visibility and the MIT License are selected. The comprehensive README, ignore rules, Dependabot configuration, and GitHub Actions workflow for CI and Azure deployment are present. Build logs and common local secret/key/database files are ignored. The Azure workflow and setup guide are local changes awaiting commit. No remote is configured and nothing has been pushed.
- Azure deployment jobs require repository variables `AZURE_DEV_DEPLOY_ENABLED=true` or `AZURE_PROD_DEPLOY_ENABLED=true`; keep the Production variable disabled.
- Do not set eBay Production keys, expose public registration, or enable Production deployment yet.

## Next actions

1. Create the selected public GitHub repository, add it as `origin`, create/push `develop`, push `main`, and review the first CI/dependency audit run.
2. Follow [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) to configure Development Azure resources, GitHub OIDC, and environment settings. Complete Blob-backed photos and persistent encrypted Data Protection keys before enabling Development deployment.
3. Keep Production deployment disabled until suitable paid hosting, isolated services, a migration process, backups, account security, and public-registration/paid-access requirements are ready.

The complete feature backlog and projected listing lifecycle diagram are in the final `NEXT TO DEVELOP` section of [README.md](README.md). The current source-level risk review is in [SECURITY_REVIEW.md](SECURITY_REVIEW.md).
