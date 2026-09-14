<!--
Sync Impact Report
- Version change: template placeholder -> 1.0.0
- Modified principles: none; replaced five template placeholders with initial project principles
- Added sections: Security and Training Constraints; Spec-Driven Development Workflow
- Removed sections: none
- Follow-up TODOs: confirm the original ratification date
-->

# ContosoDashboard Constitution

## Core Principles

### I. Layered and Replaceable Design
Features MUST preserve the separation between presentation, services, models, and data access.
Infrastructure dependencies MUST be accessed through configuration or interfaces when a future
provider change is a stated goal. Business rules MUST remain testable without coupling them to
the UI or a specific cloud service. This keeps the training architecture understandable and
demonstrates a practical migration path.

### II. Security by Default
Every authenticated feature MUST enforce authorization at the page or endpoint boundary and at
the service or data-access boundary. User-owned and project-scoped data MUST be filtered using
the requesting user's identity, and direct identifiers MUST NOT bypass those checks. New data
flows MUST define validation, access rules, and failure behavior before implementation. Training
authentication MUST remain clearly documented as non-production authentication.

### III. Small, Verifiable Increments
Each feature MUST be expressed as independently testable user scenarios with observable acceptance
criteria. Implementation MUST proceed in small increments, and every code change MUST have a
focused validation step such as a build, automated test, or runnable workflow check. Known gaps,
warnings, and training-only shortcuts MUST be documented rather than silently treated as complete.

### IV. Offline-First Portability
Core training workflows MUST run without cloud services or external accounts. Local development
configuration MUST use a supported cross-platform implementation, while provider-specific options
MUST remain isolated behind configuration or abstractions. File and database paths MUST be safe,
portable, and excluded from source control when they are local runtime artifacts.

### V. Clarity over Production Theater
The application MUST favor simple, readable solutions appropriate to a training repository. A new
abstraction or dependency MUST have a concrete teaching, testability, portability, or security
benefit. Documentation MUST distinguish implemented behavior from planned behavior and MUST call
out production requirements that are intentionally omitted.

## Security and Training Constraints

ContosoDashboard is training material, not a production security reference. The project MUST
retain its explicit warnings about mock login, absent production identity controls, and known
limitations. Production-oriented work MUST identify the required replacement for mock
authentication, including password protection or an identity provider, MFA, secure transport,
audit logging, rate limiting, and appropriate compliance review.

The application MUST support the repository's .NET and Blazor Server architecture, use dependency
injection for application services, and keep development data local and reproducible. Changes to
security headers, authentication, authorization, persistence, or user data isolation MUST include
an explicit validation scenario for both an allowed and a denied access path.

## Spec-Driven Development Workflow

Feature work MUST follow the repository's Spec Kit flow: specify the user value and acceptance
scenarios, clarify material ambiguity, create a technical plan, generate dependency-ordered tasks,
and implement against those tasks. The generated specification, plan, and tasks MUST remain
consistent with one another and with this constitution. Review gates MUST be resolved before the
next planning or implementation stage.

Implementation tasks MUST name the affected files or boundaries and MUST be marked complete only
after their focused validation succeeds. Cross-cutting changes MUST include an update to relevant
documentation or an explicit statement that no documentation change is needed.

## Governance

This constitution governs feature specifications, plans, tasks, implementation, and reviews in
this repository. When another document conflicts with it, the conflict MUST be resolved in favor
of this constitution or recorded as an approved amendment before implementation proceeds.

Amendments MUST update the constitution directly, include a Sync Impact Report, explain the reason
for the change, and update the semantic version. A MAJOR version removes or redefines a principle;
a MINOR version adds a principle or materially expands governance; a PATCH version clarifies
wording without changing obligations. The Last Amended date MUST use ISO format and change for
every amendment. The original adoption date remains unchanged once confirmed.

Reviews MUST check security boundaries, offline execution, scenario coverage, and focused
validation. Any intentional exception MUST name the affected principle, provide a rationale, and
state the follow-up required to remove or revisit the exception.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date is unknown | **Last Amended**: 2026-09-14
