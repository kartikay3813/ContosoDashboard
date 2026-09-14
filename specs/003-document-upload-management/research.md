# Research: Document Upload and Management

## Decision 1: Stage, scan, then promote uploads

**Decision**: Stream each Blazor upload to a quarantine file outside `wwwroot`, validate metadata and file type, run an injectable malware scanner, and promote the file to a generated permanent storage key only after a clean result.

**Rationale**: The feature requires that files are scanned before access and that scan failures never expose content. Staging also allows deterministic cleanup when storage or database persistence fails.

**Alternatives considered**:
- Scan after final storage: rejected because an unscanned file could be reachable during the gap.
- Background scanning: rejected for the synchronous offline training workflow and harder failure semantics.
- Require a locally installed scanner: rejected because it prevents reproducible offline setup.

## Decision 2: Abstract filesystem storage and malware scanning

**Decision**: Add `IFileStorageService` and `IMalwareScanner` abstractions. Implement local filesystem storage and a deterministic local scanner test double for training; leave production scanner/cloud implementations replaceable through dependency injection.

**Rationale**: This satisfies offline operation and the project constitution's replaceable-design and portability principles without coupling business logic to `System.IO` or a vendor scanner.

**Alternatives considered**:
- Direct `System.IO.File` calls from pages: rejected because it couples UI and business logic and weakens testing.
- Azure SDK in the training project: rejected because core workflows must work offline without cloud services.

## Decision 3: Centralize authorization in service and retrieval boundaries

**Decision**: Implement document authorization predicates/service methods used by list, search, upload, edit, replace, share, delete, preview, download, task attachment, and audit operations. Retrieval endpoints open streams only after authorization succeeds and use indistinguishable not-found/denied results where appropriate.

**Rationale**: Page `[Authorize]` attributes alone cannot protect direct identifiers or Blazor event calls. Query-level filtering prevents unauthorized documents from entering result sets.

**Alternatives considered**:
- UI-only checks: rejected because users can call direct routes and service methods still need defense in depth.
- Authorization only after loading the file: rejected because it risks disclosure and unnecessary I/O.

## Decision 4: Model explicit teams and two target types for sharing

**Decision**: Add `Team`, `TeamMembership`, and a `DocumentShare` with exactly one active target: a user or a team. Team Leads are explicitly designated on team membership. Direct user shares remain valid until revoked; team shares follow current team membership.

**Rationale**: This matches the accepted clarifications and avoids inferring teams from departments or projects.

**Alternatives considered**:
- Treat departments as teams: rejected because it grants overly broad access.
- Support individual users only: rejected because team sharing is a stated requirement.
- Separate user-share and team-share tables: possible, but a single share table keeps lifecycle logic consistent; application validation plus database indexes enforce the target invariant.

## Decision 5: Compensating transaction for files and metadata

**Decision**: Track quarantine and permanent paths through the upload workflow. Save metadata only after successful promotion; delete staged/permanent files if scanning, promotion, or database persistence fails. For replacement, stage and scan the new file before changing the existing record.

**Rationale**: EF Core transactions cannot atomically include local filesystem writes. Explicit compensation prevents orphaned files and metadata.

**Alternatives considered**:
- Database-first then file write: rejected because failed writes create orphaned records.
- Filesystem-only state: rejected because browsing, search, authorization, and audit require database metadata.

## Decision 6: Audit retention and test scope

**Decision**: Record successful document actions and security-relevant denied operations where useful, preserve actor/document/action/UTC timestamp, and retain audit records for 12 months. Add focused unit/integration coverage for authorization, validation, cleanup, claim propagation, and retrieval.

**Rationale**: The feature has explicit audit and retention requirements, and the constitution requires focused validation for security and persistence changes.

**Alternatives considered**:
- Delete audit rows with documents: rejected because audit history must remain useful.
- Indefinite retention: rejected because the specification sets a 12-month policy.
