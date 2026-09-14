# Implementation Plan: Document Upload and Management

**Branch**: `003-document-upload-management` | **Date**: 2026-09-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-document-upload-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Add secure, searchable document management to the existing ContosoDashboard training application.
The feature uses Blazor Server upload flows, EF Core metadata, staged local filesystem storage,
an injectable malware scanner, explicit team/share authorization, authorized retrieval endpoints,
and notification/dashboard/task integrations. The local implementation remains offline-capable and
provider-replaceable.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# on .NET 9; nullable reference types and implicit usings enabled  
**Primary Dependencies**: ASP.NET Core Blazor Server, Razor Pages, EF Core 9, SQLite development provider, SQL Server provider  
**Storage**: EF Core relational metadata plus local filesystem outside `wwwroot`; storage and malware scanning behind interfaces  
**Testing**: Focused unit/integration tests to be added for services, authorization, storage compensation, and Blazor-adjacent workflows; `dotnet build` and quickstart validation  
**Target Platform**: Cross-platform local development; ASP.NET Core server deployment; offline training operation  
**Project Type**: Single web application  
**Performance Goals**: 95% of lists/searches within 2 seconds for up to 500 accessible documents; uploads up to 25 MB within 30 seconds; previews within 3 seconds  
**Constraints**: 25 MB per file; allowlisted file types; malware scan before access; secure generated storage keys; integer document IDs; text categories; 12-month audit retention  
**Scale/Scope**: Existing seeded users/projects/tasks plus document, tag, share, team, membership, and audit workflows; initial web-only release

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The design passes all constitution gates:

- **Layered and Replaceable Design**: Storage and scanning are interfaces; document rules remain in services; Blazor pages do not access files directly.
- **Security by Default**: Authorization is enforced in service/query/retrieval boundaries, with explicit owner/project/team/share/admin rules and allowed/denied validation scenarios.
- **Small, Verifiable Increments**: User stories map to service/UI increments and quickstart scenarios; each task will name files and a focused check.
- **Offline-First Portability**: Local SQLite/filesystem/scanner test double work without cloud services; configuration isolates SQL Server/cloud replacements.
- **Clarity over Production Theater**: The plan distinguishes training doubles from production integrations and avoids unnecessary external dependencies.

No violations require complexity justification.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
ContosoDashboard/
├── Data/ApplicationDbContext.cs
├── Models/
│   ├── Document.cs
│   ├── DocumentTag.cs
│   ├── DocumentShare.cs
│   ├── DocumentActivity.cs
│   ├── Team.cs
│   └── TeamMembership.cs
├── Services/
│   ├── DocumentService.cs
│   ├── DocumentAuthorizationService.cs
│   ├── FileStorageService.cs
│   ├── MalwareScanner.cs
│   ├── DashboardService.cs
│   └── NotificationService.cs
├── Pages/
│   ├── Documents.razor
│   ├── DocumentDetails.razor
│   ├── DocumentDownload.cshtml.cs or authorized endpoint
│   ├── ProjectDetails.razor
│   ├── Tasks.razor and task detail surface
│   ├── Index.razor
│   └── Login.cshtml.cs
├── Shared/NavMenu.razor
└── appsettings*.json

Tests/
├── unit/
└── integration/

Feature documentation:
specs/003-document-upload-management/
├── research.md
├── data-model.md
├── contracts/
└── quickstart.md
```

**Structure Decision**: Extend the existing single ASP.NET Core project using its established
Models, Services, Data, Pages, and Shared directories. Add a test project only when implementation
begins because the repository currently has no test project. Keep feature contracts and validation
guidance under this feature directory.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | The feature uses the existing single web project and adds only boundaries required by security, portability, and testability. |

## Phase 0: Research Summary

Research decisions are recorded in [research.md](research.md): staged quarantine/scanning,
replaceable storage/scanning interfaces, service/query authorization, explicit teams and shares,
compensating cleanup, and 12-month audit retention.

## Phase 1: Design Summary

- [data-model.md](data-model.md) defines document, tag, share, team, membership, activity, state transitions, and existing-entity changes.
- [contracts/document-service.md](contracts/document-service.md) defines the application service boundary.
- [contracts/storage-and-scanning.md](contracts/storage-and-scanning.md) defines storage, scanning, and retrieval behavior.
- [quickstart.md](quickstart.md) defines runnable setup and validation scenarios.

## Post-Design Constitution Check

PASS. The design maintains layered boundaries, explicit authorization, offline portability, small
validation increments, and documented training limitations. The only production dependency left
abstracted is the real malware scanner/cloud storage implementation, which is intentional and
covered by the local test double and interfaces.
