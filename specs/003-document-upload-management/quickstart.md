# Quickstart Validation: Document Upload and Management

## Prerequisites

- .NET 9 SDK.
- Linux, macOS, or Windows development environment.
- No cloud account required.
- A clean local development database and writable application-data directory.

## Setup

From the repository root:

```bash
dotnet restore ContosoDashboard/ContosoDashboard.csproj
dotnet build ContosoDashboard/ContosoDashboard.csproj
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ContosoDashboard/ContosoDashboard.csproj
```

Development uses the configured cross-platform local database and filesystem storage outside `wwwroot`.

## Validation scenarios

1. **Upload**: Log in as a seeded employee, upload a supported file under 25 MB, provide a title/category, and confirm metadata and success feedback.
2. **Validation**: Try an unsupported type and a file over 25 MB; confirm per-file rejection and no document record or accessible file.
3. **Scanner**: Configure the scanner test double for clean, threat, and failure outcomes; verify only clean files become accessible and failed attempts clean up staged state.
4. **Authorization**: Verify owner, project member, project manager, administrator, direct-share recipient, and current team-member access. Verify an unrelated user receives the same not-found result as a missing document.
5. **Share lifetime**: Share directly with a user, remove that user's project membership, confirm access remains until the share is revoked. Remove a user from a shared team and confirm team-share access ends.
6. **Lifecycle**: Edit metadata, replace a file, preview a PDF/image, download a file, and delete after confirmation. Confirm safe filenames and audit records.
7. **Integration**: Attach a document from a task, confirm project association, then verify dashboard recent uploads/count and project/share notifications.
8. **Audit**: Perform upload, download, preview, replacement, share, revocation, and deletion; verify administrator reporting and 12-month retention behavior. Verify non-administrators are denied.
9. **Performance**: Seed up to 500 accessible documents and measure list/search latency against the 2-second goals; measure upload/preview against the stated limits.

## Focused automated validation

The implementation should add tests for:

- File size/type/content validation.
- Clean/threat/failure scanner outcomes.
- Storage/database compensation and replacement cleanup.
- Query-level authorization and direct identifier denial.
- Team and direct-share semantics.
- Task/project/dashboard/notification integration.
- Audit event creation and 12-month retention cleanup.

See [data-model.md](data-model.md) and [contracts](contracts/) for entity and boundary details.
