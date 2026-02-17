# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-17

### Added
- **Initial API Release:** Core RESTful API for the Locus Room Booking System built with ASP.NET Core 8.
- **Gatekeeper Workflow:** Implemented logic allowing multiple overlapping `Pending` booking requests for the same room and time slot.
- **Database Schema:** PostgreSQL integration using Entity Framework Core with support for soft deletions (`IsDeleted`).
- **Audit Documentation:** Technical `README.md` containing architecture overview and installation guides for university audit compliance.

### Fixed
- **Timezone Integrity:** Resolved logic conflicts between .NET `DateTime` kinds and PostgreSQL `+07` offsets by enforcing `.ToUniversalTime()` for all mathematical comparisons in the approval workflow.
- **Validation Leak:** Corrected a bug where overlapping approvals were possible due to missing UTC normalization.
- **Legacy Cleanup:** Performed manual database cleanup to remove pre-existing conflicting records that bypassed the initial logic checks.

### Changed
- **Validation Logic:** Shifted collision detection from the `POST` (Creation) endpoint to the `PATCH` (Approval) stage to support the Gatekeeper workflow.
- **Data Filtering:** Updated the `GetBookings` endpoint to remove strict `!IsDeleted` filters, ensuring historical data is visible for administrative audits.

---
*Generated for the PBL 2026 Audit - Politeknik Elektronika Negeri Surabaya (PENS)*