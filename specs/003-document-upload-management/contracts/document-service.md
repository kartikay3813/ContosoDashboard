# Document Service Contract

The document service is the application boundary used by Blazor pages, task/project integrations, dashboard queries, notifications, and audit reporting.

## Core operations

```text
UploadAsync(requestingUserId, uploadRequest) -> UploadResult
ListMyDocumentsAsync(requestingUserId, query) -> IReadOnlyList<DocumentSummary>
ListProjectDocumentsAsync(requestingUserId, projectId, query) -> IReadOnlyList<DocumentSummary>
ListSharedDocumentsAsync(requestingUserId, query) -> IReadOnlyList<DocumentSummary>
SearchAsync(requestingUserId, query) -> IReadOnlyList<DocumentSummary>
GetMetadataAsync(requestingUserId, documentId) -> DocumentSummary or not-found
UpdateMetadataAsync(requestingUserId, documentId, metadata) -> success or not-found
ReplaceFileAsync(requestingUserId, documentId, file) -> success or validation/error
DeleteAsync(requestingUserId, documentId) -> success or not-found
ShareAsync(requestingUserId, documentId, targetUserId|targetTeamId) -> success or not-found
RevokeShareAsync(requestingUserId, documentId, shareId) -> success or not-found
GetRecentUploadsAsync(requestingUserId, limit=5) -> IReadOnlyList<DocumentSummary>
GetDocumentCountAsync(requestingUserId) -> integer
GetAuditReportAsync(requestingUserId, reportQuery) -> report or forbidden
```

## Contract rules

- Every operation receives the authenticated requesting user identity and performs service-level authorization.
- List and search operations filter unauthorized rows in the query before materializing results.
- Upload validates size, extension/content type, metadata, project/task scope, and malware status before persistence.
- Upload and replacement generate safe unique storage keys and never use the original file name as a path.
- Failed validation, scan, storage, or metadata persistence leaves no accessible file or incomplete document record.
- Direct shares remain active until revoked; team shares apply only to current team members.
- Unauthorized or missing document access returns an indistinguishable not-found result to retrieval callers.
- Successful upload, download, preview, replacement, share, revocation, and deletion actions create audit records; audit data is retained for 12 months.
