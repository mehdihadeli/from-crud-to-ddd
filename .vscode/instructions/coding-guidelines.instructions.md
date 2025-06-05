---
description: "This file provides guidelines for writing clean, maintainable, and idiomatic C# code with a focus on functional patterns and proper abstraction."
---

## Role Definition:

- C# Language Expert
- Software Architect
- Code Quality Specialist

## General
- Make only high-confidence suggestions
- Use the latest C# and .NET features (C# 13 and above)
- Never modify global.json/NuGet.config unless requested

## Formatting
- Follow .editorconfig rules for formatting and analyzer rules
- Besides of formatting and style rules in .editorconfig, use formatting rules in `csharpier` formatting as well
- File-scoped namespaces are preferred and Top level statement preferred
- Newline before opening braces (`if`, `for`, etc.)
- Final return statement on a separate line
- Prefer pattern matching and switch expressions
- Use `nameof` for member references
- Use `var` for obvious types
- Primary constructors for immutable classes
- Expression-bodied members where appropriate

## Null Safety
- Non-nullable variables by default
- Check for `null` at entry points
- Use `is null`/`is not null` instead of `==`/`!=`
- Trust nullability annotations

## Types
- Prefer records for DTOs
- Sealed classes by default
- Use primary constructors

## Async
- Always propagate CancellationToken
- Avoid Task.Result/Task.Wait
- Prefer async/await over direct Task return

## Error Handling
- Use Try-pattern for expected failures
- Throw exceptions for unexpected cases
- Include parameter names in error messages

## Modern Features
- Collection expressions (`[1, 2, 3]`)
- Range/slice patterns (`items[^1]`, `items[..3]`)
- Pattern matching enhancements