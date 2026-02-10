# Changelog

## [1.0.0] - 2026-02-10
### Added
- Strict room overlap validation for new booking requests.
- Administrative approval gate to prevent double-booking.
- Global Query Filter for Soft Delete (IsDeleted).
- Automatic CreatedAt and UpdatedAt timestamps in DatabaseContext.
- DatabaseSeeder for initial Room data.

### Changed
- Refactored `GetBookings` to display Status as text labels.