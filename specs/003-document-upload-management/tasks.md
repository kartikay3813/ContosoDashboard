---
description: "Task list template for feature implementation"
---

# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/003-document-upload-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Included because the approved plan and quickstart require focused automated validation for security, storage compensation, integration, performance, and audit retention.

**Organization**: Tasks are grouped by user story so each increment can be implemented and validated independently after foundational work.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the testable project structure and local document-storage configuration.

- [ ] T001 Create `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj` targeting `net9.0` with xUnit, EF Core SQLite, and project reference to `ContosoDashboard/ContosoDashboard.csproj`
- [ ] T002 [P] Create test directories `ContosoDashboard.Tests/Unit`, `ContosoDashboard.Tests/Integration`, and `ContosoDashboard.Tests/Fixtures`
- [ ] T003 [P] Add `AppData/` and document quarantine/storage configuration keys to `ContosoDashboard/appsettings.Development.json` with roots outside `wwwroot`
- [ ] T004 [P] Add generated local document-storage and test-database artifacts to `.gitignore`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement shared entities, persistence, storage/scanning boundaries, and authorization prerequisites before any story work.

**CRITICAL**: User story work depends on this phase.

- [ ] T005 Create `ContosoDashboard/Models/Document.cs` with integer key, required title/category/uploader/file metadata, optional project/task links, generated storage path, UTC upload time, and scan state; enforce the 25 MB and 255-character file-type constraints from `data-model.md`
- [ ] T006 [P] Create `ContosoDashboard/Models/DocumentTag.cs` with normalized tag text and document relationship
- [ ] T007 [P] Create `ContosoDashboard/Models/DocumentShare.cs` with optional user/team target, grant/revoke fields, and invariant that exactly one target is set
- [ ] T008 [P] Create `ContosoDashboard/Models/Team.cs` and `ContosoDashboard/Models/TeamMembership.cs` with explicit membership and designated Team Lead state
- [ ] T009 [P] Create `ContosoDashboard/Models/DocumentActivity.cs` with actor, document snapshot/reference, action, UTC timestamp, request metadata, and 12-month retention fields
- [ ] T010 Update `ContosoDashboard/Data/ApplicationDbContext.cs` with document/tag/share/team/membership/activity sets, relationships, indexes, uniqueness constraints, integer keys, text categories, and non-cascading audit retention behavior
- [ ] T011 Update `ContosoDashboard/Models/TaskItem.cs` and related mappings in `ContosoDashboard/Data/ApplicationDbContext.cs` for task-document relationships and task/project consistency
- [ ] T012 Update `ContosoDashboard/Models/Notification.cs` with document share and project-document notification types
- [ ] T013 [P] Add `ContosoDashboard/Services/FileStorageService.cs` defining `IFileStorageService` and a local implementation rooted outside `wwwroot` with generated relative keys, quarantine support, safe path handling, and stream-based read/write/delete operations
- [ ] T014 [P] Add `ContosoDashboard/Services/MalwareScanner.cs` defining `IMalwareScanner` and a deterministic local clean/threat/failure test-double implementation for offline training
- [ ] T015 Add `ContosoDashboard/Services/DocumentAuthorizationService.cs` with query predicates and checks for owner, project member/manager, administrator, explicit user share, current team membership, and designated Team Lead management scope
- [ ] T016 Update `ContosoDashboard/Pages/Login.cshtml.cs` to emit the Department claim alongside NameIdentifier, Name, Email, and Role
- [ ] T017 Register storage, scanner, authorization, and document service dependencies in `ContosoDashboard/Program.cs`; configure document storage roots and ensure required directories are created safely
- [ ] T018 [P] Add foundational authorization and storage tests in `ContosoDashboard.Tests/Unit/DocumentAuthorizationServiceTests.cs` and `ContosoDashboard.Tests/Unit/LocalFileStorageServiceTests.cs`
- [ ] T019 Build the solution and run foundational tests from `ContosoDashboard.Tests` to verify the schema, dependency registration, claims, and local storage boundaries before story work

**Checkpoint**: Shared data, storage, scanner, authorization, claims, and test infrastructure are ready.

---

## Phase 3: User Story 1 - Upload and Categorize Documents (Priority: P1) 🎯 MVP

**Goal**: Allow authenticated employees to upload valid files with metadata, scan them, persist them safely, and receive per-file outcomes.

**Independent Test**: Upload a clean supported file under 25 MB with title/category and verify metadata, generated storage path, success feedback, and cleanup for invalid/threat/failure cases.

### Tests for User Story 1

- [ ] T020 [P] [US1] Add upload validation tests in `ContosoDashboard.Tests/Unit/DocumentUploadValidationTests.cs` covering supported types, mismatched content, exact 25 MB acceptance, oversized rejection, required title/category, and 255-character MIME values
- [ ] T021 [P] [US1] Add upload compensation tests in `ContosoDashboard.Tests/Integration/DocumentUploadWorkflowTests.cs` covering clean, threat, scanner-failure, storage-failure, database-failure, mixed-validity multi-file, and orphan cleanup outcomes

### Implementation for User Story 1

- [ ] T022 [US1] Create `ContosoDashboard/Services/DocumentService.cs` upload orchestration that validates metadata/content, authorizes project/task scope, stages to quarantine, scans, generates `{userId}/{projectId-or-personal}/{guid}.{extension}`-equivalent safe keys, persists metadata, and compensates failed file/database operations
- [ ] T023 [US1] Add upload request/result DTOs and allowlisted category/type validation in `ContosoDashboard/Services/DocumentService.cs` or `ContosoDashboard/Models/DocumentUploadRequest.cs`
- [ ] T024 [US1] Create `ContosoDashboard/Pages/Documents.razor` with authenticated multi-file `InputFile`, required title/category, optional description/project/tags, progress state, per-file success/error results, and `@key` reset behavior using the MemoryStream upload pattern
- [ ] T025 [US1] Add `Documents` navigation to `ContosoDashboard/Shared/NavMenu.razor` and protect the upload route with authorization
- [ ] T026 [US1] Seed or fixture valid projects, teams, and document-related users in `ContosoDashboard/Data/ApplicationDbContext.cs` or `ContosoDashboard.Tests/Fixtures/TestDataFactory.cs` for repeatable upload validation

**Checkpoint**: US1 independently supports secure clean uploads and rejects invalid, malicious, or incomplete attempts without exposed files or orphaned records.

---

## Phase 4: User Story 2 - Browse and Search Accessible Documents (Priority: P1)

**Goal**: Provide authorized document lists, project documents, sorting/filtering, and search within the required performance target.

**Independent Test**: Seed personal, project, direct-shared, team-shared, and inaccessible documents; verify list/search results contain only authorized documents and honor every sort/filter.

### Tests for User Story 2

- [ ] T027 [P] [US2] Add document query tests in `ContosoDashboard.Tests/Unit/DocumentQueryTests.cs` for ownership, project membership, direct shares, team shares, administrator access, inaccessible direct IDs, sorting, category/project/date filters, and search fields
- [ ] T028 [P] [US2] Add list/search performance coverage in `ContosoDashboard.Tests/Integration/DocumentQueryPerformanceTests.cs` using up to 500 accessible documents and the 2-second requirement

### Implementation for User Story 2

- [ ] T029 [US2] Extend `ContosoDashboard/Services/DocumentService.cs` with query-level `ListMyDocumentsAsync`, `ListProjectDocumentsAsync`, `ListSharedDocumentsAsync`, and `SearchAsync` methods that apply authorization predicates before materialization
- [ ] T030 [US2] Create `ContosoDashboard/Pages/Documents.razor` list state and UI for title/category/date/size/project columns, title/date/category/size sorting, category/project/date filters, search, loading, error, and empty states
- [ ] T031 [US2] Update `ContosoDashboard/Pages/ProjectDetails.razor` to display authorized project documents and links to authorized retrieval operations
- [ ] T032 [US2] Add `ContosoDashboard/Pages/SharedDocuments.razor` or the equivalent view within `Documents.razor` for Shared with Me results

**Checkpoint**: US1 and US2 independently support upload plus secure browsing/search without leaking unauthorized records.

---

## Phase 5: User Story 3 - Manage, Preview, Download, and Share Documents (Priority: P1)

**Goal**: Complete the document lifecycle and controlled sharing with secure retrieval and explicit team/direct-share semantics.

**Independent Test**: As owner, project manager, Team Lead, recipient, administrator, and unrelated user, exercise metadata/file replacement, preview/download, delete, share/revoke, and denied access paths.

### Tests for User Story 3

- [ ] T033 [P] [US3] Add lifecycle and authorization tests in `ContosoDashboard.Tests/Integration/DocumentLifecycleTests.cs` for edit, replacement, owner/project-manager/Team-Lead/admin delete rights, denied operations, safe filenames, and cleanup
- [ ] T034 [P] [US3] Add share semantics tests in `ContosoDashboard.Tests/Integration/DocumentSharingTests.cs` for explicit teams, designated Team Leads, direct shares surviving project-membership loss, team shares following current membership, revoke behavior, Shared with Me, and notifications
- [ ] T035 [P] [US3] Add retrieval endpoint tests in `ContosoDashboard.Tests/Integration/DocumentRetrievalTests.cs` for authorized download, PDF/image inline preview, content type, safe download name, and indistinguishable unauthorized/missing responses

### Implementation for User Story 3

- [ ] T036 [US3] Extend `ContosoDashboard/Services/DocumentService.cs` with metadata update, staged/scanned replacement, delete, share, revoke, metadata access, and retrieval authorization operations with audit writes
- [ ] T037 [US3] Add authorized retrieval handler in `ContosoDashboard/Pages/DocumentDownload.cshtml.cs` or an equivalent endpoint that authorizes before opening storage, returns safe content headers, and never exposes filesystem paths
- [ ] T038 [US3] Add document details/edit/share UI in `ContosoDashboard/Pages/DocumentDetails.razor` for metadata updates, valid replacement files, delete confirmation, individual/team sharing, revocation, and PDF/image preview
- [ ] T039 [US3] Extend `ContosoDashboard/Services/NotificationService.cs` and its consumers to notify direct/team recipients and project members after successful share/project-document events
- [ ] T040 [US3] Add team-management data access and authorized UI/service operations in `ContosoDashboard/Services/TeamService.cs` or the smallest existing service boundary required to maintain explicit membership and Team Lead designation

**Checkpoint**: Owners, project managers, designated Team Leads, recipients, and administrators receive only their permitted lifecycle operations; retrieval never bypasses authorization.

---

## Phase 6: User Story 4 - Integrate Documents with Tasks and Dashboard (Priority: P2)

**Goal**: Connect documents to task/project context, dashboard summaries, recent uploads, and notifications.

**Independent Test**: Attach/upload from an authorized task, verify task/project association, then verify dashboard recent uploads/count and event notifications.

### Tests for User Story 4

- [ ] T041 [P] [US4] Add task-document integration tests in `ContosoDashboard.Tests/Integration/TaskDocumentIntegrationTests.cs` for authorized task access, project consistency, denied task access, and visible attachments
- [ ] T042 [P] [US4] Add dashboard integration tests in `ContosoDashboard.Tests/Integration/DashboardDocumentIntegrationTests.cs` for five recent uploads, document count, and project/share notifications

### Implementation for User Story 4

- [ ] T043 [US4] Extend `ContosoDashboard/Services/DocumentService.cs` with task attachment/upload operations that require task authorization and reject task/project mismatches
- [ ] T044 [US4] Add a task detail surface at `ContosoDashboard/Pages/TaskDetails.razor` or extend `ContosoDashboard/Pages/Tasks.razor` with authorized document attachments and task-origin upload
- [ ] T045 [US4] Extend `ContosoDashboard/Services/DashboardService.cs` and `DashboardSummary` with authorized document count and five recent uploads
- [ ] T046 [US4] Update `ContosoDashboard/Pages/Index.razor` with the Recent Documents widget and document count summary card
- [ ] T047 [US4] Ensure project-document creation and sharing paths invoke the correct in-app notification types through `ContosoDashboard/Services/NotificationService.cs`

**Checkpoint**: Document workflows are visible from tasks, projects, dashboard summaries, and notifications without weakening story 1-3 authorization.

---

## Phase 7: User Story 5 - Audit Document Activity (Priority: P2)

**Goal**: Provide administrator-only activity reporting with complete events and 12-month retention.

**Independent Test**: Execute upload, download, preview, replacement, share, revocation, and delete actions; verify complete audit records, administrator reports, retention cleanup, and denied non-administrator access.

### Tests for User Story 5

- [ ] T048 [P] [US5] Add audit event tests in `ContosoDashboard.Tests/Unit/DocumentActivityTests.cs` for actor, document, action, UTC time, successful operations, and retained document identity after deletion
- [ ] T049 [P] [US5] Add audit reporting/retention tests in `ContosoDashboard.Tests/Integration/DocumentAuditTests.cs` for administrator-only reports, non-administrator denial, 12-month boundary, and cleanup of older records

### Implementation for User Story 5

- [ ] T050 [US5] Extend `ContosoDashboard/Services/DocumentService.cs` with administrator-only audit queries, aggregate reports, and 12-month retention cleanup
- [ ] T051 [US5] Create `ContosoDashboard/Pages/DocumentAudit.razor` with administrator authorization, action/type/uploader/access-pattern report views, loading/error/empty states, and retention-aware date filters
- [ ] T052 [US5] Add administrator-only navigation and route protection for `ContosoDashboard/Pages/DocumentAudit.razor`

**Checkpoint**: Administrators can review the required document activity and reports while non-administrators cannot access audit data.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature, document training limitations, and confirm performance/security requirements.

- [ ] T053 [P] Add end-to-end authorization coverage for allowed and denied paths in `ContosoDashboard.Tests/Integration/DocumentSecurityTests.cs`, including direct identifier access and file disclosure prevention
- [ ] T054 [P] Add upload/list/search/preview timing checks and representative 25 MB fixtures in `ContosoDashboard.Tests/Integration/DocumentPerformanceTests.cs`
- [ ] T055 [P] Update `README.md` with cross-platform document-storage setup, scanner test-double behavior, supported file types, limits, security limitations, and validation commands
- [ ] T056 [P] Update `StakeholderDocs/document-upload-and-management-feature.md` only if implementation decisions materially change stakeholder-facing scope; otherwise document no change needed in the feature plan
- [ ] T057 Run `specs/003-document-upload-management/quickstart.md` end-to-end on the configured .NET 9 development environment and record any remaining known gaps
- [ ] T058 Run `dotnet build ContosoDashboard/ContosoDashboard.csproj` and all `ContosoDashboard.Tests` tests; resolve feature-caused warnings/failures without changing unrelated training limitations

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; establishes test project and local configuration.
- **Foundational (Phase 2)**: Depends on Setup; blocks every user story.
- **User Story 1 (Phase 3)**: Depends on Foundational; MVP upload increment.
- **User Story 2 (Phase 4)**: Depends on Foundational and the Document entity/service from US1; query UI can be developed in parallel after shared contracts exist.
- **User Story 3 (Phase 5)**: Depends on Foundational and DocumentService from US1; retrieval/share UI builds on document queries and notification boundaries.
- **User Story 4 (Phase 6)**: Depends on US1-3 integration boundaries and existing task/dashboard services.
- **User Story 5 (Phase 7)**: Depends on activity recording established by US1-3 and administrator authorization.
- **Polish (Phase 8)**: Depends on all desired user stories.

### User Story Dependencies

- **US1 (P1)**: Foundational only; MVP.
- **US2 (P1)**: Foundational plus US1 document metadata/storage contract.
- **US3 (P1)**: Foundational plus US1 storage and US2 query/authorization surfaces.
- **US4 (P2)**: US1-3 service contracts and existing task/dashboard/notification services.
- **US5 (P2)**: US1-3 audit events and administrator authorization.

### Within Each User Story

- Write the story's tests before implementation and confirm they fail for missing behavior.
- Implement models/boundaries before services, services before UI/endpoints, and integrations after core operations.
- Run the story checkpoint independently before starting the next dependent story.

## Parallel Opportunities

- **Setup**: T002-T004 can run in parallel after T001 establishes the test project.
- **Foundational**: T006-T009, T013-T014, and T018 can run in parallel where files do not overlap; T010-T012 and T015-T017 follow their model/boundary dependencies.
- **US1**: T020 and T021 can run in parallel; T023 and T025 can run in parallel after the service contract is established.
- **US2**: T027 and T028 can run in parallel; list/search UI and project-document UI can be split after T029.
- **US3**: T033-T035 can run in parallel; retrieval endpoint, details UI, notification extension, and team management can be split after T036's contract is agreed.
- **US4**: T041 and T042 can run in parallel; task UI and dashboard UI can be split after service methods are available.
- **US5**: T048 and T049 can run in parallel; audit service/UI/navigation can be split after T050's contract is defined.
- **Polish**: T053-T056 can run in parallel; T057-T058 are final sequential validation.

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup.
2. Complete Phase 2 foundational models, storage, scanner, authorization, claims, and registration.
3. Complete Phase 3 US1 upload and categorize workflows.
4. Run the US1 tests and quickstart upload/validation scenarios.
5. Stop for review/demo before adding browsing or management.

### Incremental Delivery

1. Setup + Foundational -> validated offline foundation.
2. US1 -> secure upload MVP.
3. US2 -> browsing/search and project document discovery.
4. US3 -> lifecycle, retrieval, sharing, and team permissions.
5. US4 -> task/dashboard/notification integration.
6. US5 -> administrator audit/reporting.
7. Polish -> performance, security, documentation, and complete quickstart validation.

### Notes

- Every task follows the required checklist format: checkbox, sequential ID, optional `[P]`, required story label in story phases, and an explicit file path.
- No task assumes cloud services or a Windows-only LocalDB environment.
- Existing unrelated nullable warnings and training-only authentication limitations are not scope for this feature unless a task explicitly names them.
