# ListLift — handoff and next steps

## Current status

- Local account, draft, photo, description, eBay category, and administrator settings flows were previously built and manually exercised by the user.
- The user confirmed a clean full-solution build and 37 passing tests after the form-recovery and quantity changes. After the Azure adapter work in this turn, 43 tests pass locally; the API and infrastructure projects build in Release.
- Migration `20261110142117_AddListingDraftQuantity` was generated and applied to the user's Development LocalDB. The second attempt to add it correctly reported the migration name already exists; `database update` confirmed the database was current.
- Unsaved form text is being recovered from tab-scoped browser storage after reload and sign-in. It is cleared on sign-out; unuploaded photo selections are not retained.
- Draft quantity is represented in the domain, API, and client. It defaults to 1 and is limited to 1–999,999.
- GitHub repository `look-96/ListLift` is configured as `origin`. The Mermaid endpoint diagram issue was fixed by quoting the category endpoint node label; the fix is commit `95fff6f`, and the user confirmed it renders on GitHub. Local `develop` matches `origin/develop` at that commit. `origin/main` has one subsequent README-only commit (`95a53e3`); incorporate that small README change when preparing a pull request to `main`.
- Public visibility and the MIT License are selected. The comprehensive README, ignore rules, Dependabot configuration, and GitHub Actions workflow for CI and Azure deployment are present. Build logs and common local secret/key/database files are ignored.
- Azure Blob photo storage and durable Data Protection key storage encrypted with Azure Key Vault are implemented on the local `develop` branch. The App Service path fails closed unless Azure Blob storage is selected and the Blob/Key Vault URIs are configured. Managed identity is used; no storage key or Azure credential is checked in.
- GitHub Actions CI succeeded on `develop` commit `caec8f7`, including dependency audit, full Release build, tests, and API/client package publishing. Azure deployment was skipped because its gate remains off. Updated GitHub Action major versions passed on their Dependabot branches as well.
- Azure deployment remains gated off. No Azure resources, GitHub `Development` environment values, or OIDC credentials have been configured.
- Azure deployment jobs require repository variables `AZURE_DEV_DEPLOY_ENABLED=true` or `AZURE_PROD_DEPLOY_ENABLED=true`; keep the Production variable disabled.
- Do not set eBay Production keys, expose public registration, or enable Production deployment yet.

## Next actions

1. Prepare Restaurants API Final/Local Eats for Azure. When provisioning both apps, place their Development resources in shared `rg-capstone-dev` but keep each app's data, storage, identity, and secrets separate.
2. Start Development hosting setup by following Sections 1 and 3 of [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md): review subscription eligibility and cost limits, set a budget alert, then create shared `rg-capstone-dev` and ListLift resources.
3. Configure the GitHub `Development` environment and OIDC as described in Sections 2, 4, and 5. Configure the documented app settings and role assignments, then enable the Development gate only after reviewing them.
4. Keep Production deployment disabled until suitable paid hosting, isolated services, a migration process, backups, account security, and public-registration/paid-access requirements are ready.

The complete feature backlog and projected listing lifecycle diagram are in the final `NEXT TO DEVELOP` section of [README.md](README.md). The current source-level risk review is in [SECURITY_REVIEW.md](SECURITY_REVIEW.md).
