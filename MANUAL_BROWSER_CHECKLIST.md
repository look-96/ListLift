# ListLift — manual browser walkthrough

The automated Domain, Application, Infrastructure, and API unit suites passed. Use this checklist for the UI and live local API flow. Use a disposable draft and a non-sensitive sample image.

The entries marked complete below record the earlier walkthrough. Re-run the new quantity and recovery checks after generating/applying the quantity migration and rebuilding the client.

## Start the client

Keep the API running at `https://localhost:7155`. In another PowerShell terminal from the project root, run:

```powershell
dotnet run --project .\src\ListLift.Client\ListLift.Client.csproj
```

Open `https://localhost:7160` and sign in with your ListLift account.

## Walkthrough

- [x] Create a disposable draft with a simple product name, category, price `0.01`, and currency `GBP`. Confirm it appears in the active-draft list and shows the GBP amount.
- [x] Select the draft, edit product facts, save, refresh the page, and confirm the saved values remain.
- [x] For output encoding, set the product name to `<script>alert('ListLift-XSS')</script>`, save, and confirm it is displayed as text without executing a script or showing an alert. Restore a normal name afterward.
- [x] For SQL-input handling, use a harmless literal such as `camera' OR '1'='1` as the product name. Confirm it is treated as text and the draft list behaves normally. Restore a normal name afterward.
- [x] Upload a non-sensitive JPEG, PNG, or WebP sample. Confirm success and that the photo count increments.
- [x] Generate the local demo description. Confirm it is presented as a suggestion, edit it, save it, then refresh and confirm the edited text remains.
- [x] Search eBay categories. Because eBay Production app settings remain unset, expect a safe unavailable message; do not use the admin `Set` action yet.
- [x] Delete the disposable draft and confirm it disappears from the active list.
- [x] Sign out. Confirm the app returns to the sign-in screen.
- [x] Create a draft with the quantity field left blank; verify the saved quantity is 1.
- [x] Create another draft with a quantity greater than 1; edit and save it, then verify the displayed quantity survives reload.
- [x] Type unsaved text into the create form, reload the page, sign in to the same account in the same tab, and verify the text is restored.
- [x] Change an existing draft's facts and description without saving, reload and sign in again in the same tab, and verify both edits are restored.
- [x] Sign out and sign back in; verify the unsaved recovery data was cleared on sign-out.
- [x] Confirm a pending file-picker selection is not restored and can be selected again; already uploaded photos remain on the server.

The 24-hour expiry boundary has automated Domain coverage. Runtime cleanup after a full day remains a separate observation and need not delay this walkthrough.
