# ListLift Google Cloud Deployment Plan

This file tracks the remaining work to deploy ListLift to Google Cloud. We will complete the steps in order and update the checkboxes as work progresses.

## Current status

The Google Cloud and PostgreSQL work is present in the local working tree. It has not been pushed to GitHub or deployed. The API, `ListLift.Maintenance`, and Infrastructure test projects build in Release. The local VSTest host did not produce test results in this restricted environment, so the suite remains unverified here. The Blazor WebAssembly build also cannot run here because the Windows sandbox blocks the MSBuild task host. CI now uses Linux and includes gated Google Cloud deployment jobs, but it has not yet run on GitHub. No live database migration or cloud job has been run.

### Already started in the repository

- PostgreSQL provider selection and a separate PostgreSQL migrations project with an initial migration.
- Google Cloud Storage photo storage and Data Protection key-ring storage.
- Google Cloud KMS encryption for Data Protection keys.
- A Dockerfile that publishes the API and Blazor client together for a single Cloud Run service.
- A `ListLift.Maintenance` command project packaged into the container image for one-off database migration and scheduled expiry cleanup jobs.
- Relative API URL support for same-origin hosting.
- Google Cloud configuration tests and a Linux CI validation runner.

## Deployment steps

### 1. Finish the application behavior required for Cloud Run

- [x] Add a controlled production migration runner. `ListLift.Maintenance migrate-postgresql` runs migrations explicitly; the API continues to skip automatic migrations outside Development.
- [x] Add a one-shot expiry cleanup command. `ListLift.Maintenance cleanup-expired-drafts` processes batches and exits unsuccessfully if a batch needs a retry.
- [x] Disable the API's continuously running expiry worker on Cloud Run; the scheduled Job will own cloud cleanup while local development retains the worker.
- [x] Confirm the cleanup handler deletes each draft's photos before deleting the database record. Google Cloud Storage deletion is idempotent.
- [ ] Provision the Cloud Run migration and cleanup Jobs and a Cloud Scheduler trigger. Schedule cleanup every 15 minutes so expired data is physically removed shortly after its 24-hour active period.
- [x] Confirm production configuration fails safely: Cloud Run requires the PostgreSQL provider, the database connection string is required, and Google Cloud storage configuration validates its bucket and KMS key. The admin endpoint rejects credentials if its configured hash is missing.

### 2. Finish and verify CI

- [x] Replace the Azure artifact/deployment workflow with gated Google Cloud container deployment jobs.
- [x] Keep restore, dependency audit, solution build, and automated tests in CI.
- [x] Add a Docker image build in CI so the Cloud Run packaging path is checked.
- [ ] Verify a GitHub Actions run succeeds on the Linux runner. The local Windows task-host limitation is not considered resolved until this run passes.

### 3. Create Google Cloud Development project and review costs

- [x] Create a dedicated Google Cloud Development project for ListLift. Project ID: `project-2d1307cc-ca8a-4589-b39`.
- [x] Use `europe-central2` (Warsaw) for the current Development test deployment, matching the user's current location. Decide the Production region separately; Cloud SQL instance regions cannot be changed after creation.
- [x] Create a billing budget and alerts before provisioning resources.
- [ ] Confirm billing is linked to the Development project.
- [ ] Decide whether to use the Cloud SQL PostgreSQL trial for testing. It lasts up to 30 days, requires a new Google Cloud project, uses a preset 8-vCPU/64-GB configuration, and does not provide normal backups during the trial. It is not a permanent free production database.

Google Cloud uses projects as the main resource boundary rather than Azure resource groups. A separate Production project is planned in Step 9.

### 4. Provision Development resources

- [x] Enable the required APIs: Cloud Run, Cloud SQL Admin, Artifact Registry, Cloud Storage, Cloud KMS, Secret Manager, IAM/Workload Identity Federation, and Cloud Scheduler/Jobs for cleanup. Enabled successfully in Cloud Shell on 2026-11-11.
- [x] Create a Cloud SQL for PostgreSQL instance (`listlift-dev-pg`, `europe-central2`) and the `listlift` database. The `listlift_app_role` has database connect and schema usage/create grants; the built-in `listlift_app` login was created with that role.
- [x] Create a private Cloud Storage bucket (`listlift-dev-project-2d1307cc-ca8a-4589-b39`, `europe-central2`) for listing photos and Data Protection keys. Uniform bucket-level access and public access prevention are enabled; soft delete is disabled to honor the 24-hour deletion behavior. Keep the key-ring prefix outside any photo-expiration lifecycle rule.
- [x] Create a symmetric Cloud KMS key for Data Protection key encryption: `projects/project-2d1307cc-ca8a-4589-b39/locations/europe-central2/keyRings/listlift-dev/cryptoKeys/listlift-data-protection` (software protection, enabled primary version).
- [x] Create an Artifact Registry Docker repository (`listlift`) in `europe-central2` for ListLift images.

### 5. Configure identities and secrets

- [x] Create a dedicated Cloud Run runtime service account (`listlift-runtime@project-2d1307cc-ca8a-4589-b39.iam.gserviceaccount.com`) and grant Cloud SQL Client, bucket-scoped Storage Object Admin, key-scoped KMS Encrypter/Decrypter, and secret-scoped Secret Accessor permissions.
- [x] Store the PostgreSQL connection string in Secret Manager (`LISTLIFT_DATABASE`, version 1), configured for the Cloud SQL socket path `/cloudsql/PROJECT:REGION:INSTANCE`.
- [x] Store the existing administrator credential hash in Secret Manager (`LISTLIFT_ADMIN_CREDENTIAL_HASH`). Keep the laptop-bound credential file private and on the laptop.
- [ ] Configure Cloud Run settings, including `Database:Provider=PostgreSql`, `Storage:PhotoProvider=GoogleCloudStorage`, bucket name, object prefixes, KMS key resource name, and production environment.
- [ ] Confirm eBay Production credentials can be entered through the protected administrator endpoint after the service is deployed. Category lookup will remain unavailable until configured.

### 6. Apply PostgreSQL migration and deploy Development

- [ ] Build and push the container image to Artifact Registry.
- [ ] Create the migration Cloud Run Job using command `dotnet maintenance/ListLift.Maintenance.dll migrate-postgresql`.
- [ ] Run the migration job before serving the first revision.
- [ ] Create the cleanup Cloud Run Job using command `dotnet maintenance/ListLift.Maintenance.dll cleanup-expired-drafts`, then schedule it through Cloud Scheduler.
- [ ] Deploy the container to Cloud Run with the runtime service account, Cloud SQL connection, secret bindings, region, port, and an appropriate maximum instance count.
- [ ] Keep Cloud Run's public HTTP access enabled for the web app; user authentication and authorization remain enforced by ListLift.
- [ ] Confirm `/health` and the app root return successfully.

### 7. Run Development smoke checks

- [ ] Register/login and verify the authenticated session.
- [ ] Create, edit, and retrieve a listing draft.
- [ ] Upload a photo and verify it remains available after a new Cloud Run revision.
- [ ] Verify Data Protection keys persist and decrypt through Cloud KMS after a new revision.
- [ ] Verify expired drafts are cleaned up, including their photos in the bucket.
- [ ] Verify eBay category search reports a controlled unavailable response before credentials are configured, then test it after setup.
- [ ] Review Cloud Run logs, Cloud SQL connectivity, and billing usage.

### 8. Configure GitHub Actions authentication and Development deployment

- [ ] Configure Workload Identity Federation between the GitHub repository and Google Cloud, restricted to the intended repository and branch/environment.
- [ ] Avoid long-lived service-account JSON keys.
- [ ] Add Development-specific GitHub environment variables for project, region, service, Artifact Registry, runtime service account, bucket, KMS key, Cloud SQL instance, and Secret Manager references.
- [ ] Set repository variables `GCP_DEV_DEPLOY_ENABLED` and `GCP_PROD_DEPLOY_ENABLED` to `false` until each environment is ready.
- [ ] Add these variables to both GitHub environments (`Development` and `Production`): `GCP_PROJECT_ID`, `GCP_REGION`, `GCP_ARTIFACT_REPOSITORY`, `GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_DEPLOY_SERVICE_ACCOUNT`, `GCP_RUNTIME_SERVICE_ACCOUNT`, `GCP_CLOUD_SQL_INSTANCE`, `GCP_DATABASE_SECRET`, `GCP_ADMIN_CREDENTIAL_HASH_SECRET`, `GCP_GCS_BUCKET`, `GCP_KMS_KEY_NAME`, `GCP_RUN_SERVICE`, `GCP_MIGRATION_JOB`, and `GCP_CLEANUP_JOB`.
- [ ] Enable deployment from `develop` only after CI passes.
- [ ] Verify a complete push-to-Development deployment and health check.

### 9. Prepare Production separately

- [ ] Create a separate Production Google Cloud project with separate database, bucket, KMS key, service account, and secrets.
- [ ] Configure Production GitHub environment protection and a manual approval gate.
- [ ] Configure database backups and retention appropriate for production; the Cloud SQL trial does not provide normal backups.
- [ ] Confirm administrator-only settings, registration policy, HTTPS behavior, rate limits, monitoring, and budget alerts.
- [ ] Deploy Production from `main` only after Development checks pass.
- [ ] Test rollback to a previous Cloud Run revision and database restore procedure.

### 10. Update project documentation

- [ ] Replace Azure deployment instructions with this Google Cloud process.
- [ ] Document required configuration names, migration procedure, cleanup schedule, GitHub OIDC setup, and Development/Production separation.
- [ ] Document Cloud SQL trial limits and expected post-trial costs.
- [ ] Update the README architecture diagram to show Cloud Run, PostgreSQL, Cloud Storage, Cloud KMS, Secret Manager, and the scheduled cleanup path.

## Important references

- [Deploy container images to Cloud Run](https://docs.cloud.google.com/run/docs/deploying)
- [Connect Cloud Run to Cloud SQL for PostgreSQL](https://docs.cloud.google.com/sql/docs/postgres/connect-run)
- [Cloud SQL PostgreSQL free-trial instance](https://docs.cloud.google.com/sql/docs/postgres/free-trial-instance)
- [Cloud Run autoscaling behavior](https://docs.cloud.google.com/run/docs/about-instance-autoscaling)
- [Schedule Cloud Run Jobs with Cloud Scheduler](https://docs.cloud.google.com/run/docs/execute/jobs-on-schedule)
- [Configure Cloud Run Job command and arguments](https://docs.cloud.google.com/run/docs/configuring/jobs/containers)
- [Google authentication action and Workload Identity Federation](https://github.com/google-github-actions/auth)
- [Google Cloud Free Program and billing](https://docs.cloud.google.com/free/docs/free-cloud-features)

## Resume point

Steps 1-4 are implemented locally or provisioned in the Development project. The runtime service account, scoped IAM grants, PostgreSQL connection string secret, and admin credential hash secret are in place. Google Cloud code and workflow changes remain uncommitted in the local `develop` branch; push them to GitHub and verify CI before building/deploying the image. The local VSTest host and WebAssembly task host cannot complete in this restricted Windows environment. Cloud Run Jobs and the Cloud Scheduler trigger are provisioned in Step 6.
