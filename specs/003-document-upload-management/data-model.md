# Data Model: Document Upload and Management

## Document

Represents an uploaded work file and its searchable metadata.

| Field | Rules |
|---|---|
| `DocumentId` | Required integer primary key, consistent with existing entities. |
| `Title` | Required user-facing title. |
| `Description` | Optional description. |
| `Category` | Required text value: Project Documents, Team Resources, Personal Files, Reports, Presentations, or Other. |
| `OriginalFileName` | Required display name; never used as a storage path. |
| `StoragePath` | Required generated relative key; outside `wwwroot`; unique. |
| `FileType` | Required detected/validated MIME type, maximum 255 characters. |
| `FileSize` | Required byte count; maximum 25 MB per file. |
| `UploadedByUserId` | Required relationship to `User`. |
| `ProjectId` | Optional relationship to `Project`. |
| `TaskId` | Optional relationship to `TaskItem`; when present it must match the task's project. |
| `UploadedAt` | Required UTC timestamp. |
| `ScanStatus` | Required state; only clean documents become accessible. |

## DocumentTag

A normalized searchable tag belonging to one document. Enforce uniqueness for `(DocumentId, TagText)` and trim/normalize user input before persistence.

## DocumentShare

An explicit access grant for one document. It contains `DocumentId`, optional `UserId`, optional `TeamId`, `GrantedByUserId`, `GrantedAt`, and nullable `RevokedAt`. Exactly one of `UserId` and `TeamId` must be set. Direct user shares remain active until revoked; team shares follow current membership.

## Team

An explicit group with a name and managed membership. It is not inferred from `User.Department`.

## TeamMembership

Joins `Team` and `User`, with membership status and an explicit `IsTeamLead` or equivalent role. Team Leads manage documents uploaded by members of teams they lead.

## DocumentActivity

Immutable audit event containing `DocumentActivityId`, nullable `DocumentId` or immutable document identifier snapshot, actor/user reference, action, UTC timestamp, and optional request metadata. Retain for 12 months and do not cascade-delete audit history with documents.

## Existing entity changes

- `TaskItem` gains a document collection or join relationship.
- `NotificationType` gains document-share and project-document values.
- Login claims include Department in addition to NameIdentifier, Name, Email, and Role.
- `DashboardSummary` gains document count and the dashboard service exposes five recent uploads.

## Authorization relationships

A document can be read when the requester is an administrator, uploader, current project member/manager, active direct-share recipient, or current member of a targeted team. Management rights are narrower: owners manage their documents, project managers manage documents in managed projects, designated Team Leads manage documents uploaded by members of their teams, and administrators have full access.

## State transitions

1. `Quarantined` -> `Rejected` when validation or malware scanning fails.
2. `Quarantined` -> `Clean` when validation and scanning pass.
3. `Clean` -> `Replaced` through stage/scan/promote of a new file while retaining `DocumentId`.
4. `Clean` -> `Deleted` after authorized confirmation and permanent file removal.
5. `DocumentShare` active -> `Revoked` only through owner/administrator authorization; team membership changes affect team shares without mutating the share record.
