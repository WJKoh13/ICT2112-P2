# Migrations

This directory will contain the initial database migration after ERD finalization.

The single migration file (e.g., `20260101_001_FullSchema.cs`) will define all tables upfront.

Teams fork this repo, run `dotnet ef database update` once during setup, and never modify migrations.
