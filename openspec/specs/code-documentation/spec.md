# Spec: Code Documentation

## Purpose

Standards for how code comments are written in the Tabrixel project, covering XML documentation format for member-level comments and project build configuration for documentation output.

## Requirements

### Requirement: Member-level comments use XML documentation format
All comments that document a type, method, property, or field declaration SHALL use the `/// <summary>` XML documentation syntax rather than plain `//` comments.

#### Scenario: Type declaration has a doc comment
- **WHEN** a class, record, struct, interface, or enum has a comment describing its purpose
- **THEN** that comment SHALL appear as a `/// <summary>` block immediately above the declaration

#### Scenario: Method has a doc comment
- **WHEN** a public or internal method has a comment describing its behavior
- **THEN** that comment SHALL appear as a `/// <summary>` block immediately above the method signature

#### Scenario: Property has a doc comment
- **WHEN** a public or internal property has a comment describing its role
- **THEN** that comment SHALL appear as a `/// <summary>` block immediately above the property declaration

### Requirement: Inline body comments remain as plain comments
Comments inside method bodies that annotate implementation logic (not the member itself) SHALL remain as plain `//` comments and SHALL NOT be converted to XML doc format.

#### Scenario: Implementation comment stays as single-line comment
- **WHEN** a `//` comment appears inside a method body
- **THEN** it SHALL remain a `//` comment and SHALL NOT be changed to `/// <summary>`

### Requirement: Project generates XML documentation file
The project file SHALL have `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and `<NoWarn>` SHALL include `CS1591` so that malformed XML doc syntax is a build error but missing-doc warnings are suppressed.

#### Scenario: Build validates XML doc syntax
- **WHEN** a developer builds the project
- **THEN** the compiler SHALL report errors for malformed `/// <summary>` blocks
- **THEN** the compiler SHALL NOT report CS1591 warnings for undocumented members
