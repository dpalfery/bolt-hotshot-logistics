# Validation Rule

Enforces input validation practices for ASP.NET Core applications in Hotshot Logistics, ensuring security and consistent error handling.

## When to Apply
Apply when implementing input validation in ASP.NET Core APIs, controllers, or services (.NET 8+).

## Input Validation
- Validate all user inputs on client and server sides.
- Use built-in ASP.NET Core validation attributes or FluentValidation.
- Never trust client data; validate and sanitize inputs.
- Implement comprehensive validation for endpoints accepting user input.

## Error Handling
- Return ProblemDetails consistently for error responses.
- Use appropriate HTTP status codes (400 for validation errors, 500 for server errors).
- Include meaningful error messages without exposing sensitive information.
- Standardize error response format across APIs.

## Validation Implementation
- Use Data Annotations for simple rules.
- Implement custom validation attributes for complex business rules.
- Validate inputs in controller actions or application services.
- Provide clear validation feedback to clients.

## Security Considerations
- Prevent injection attacks through validation.
- Validate file uploads and size limits.
- Implement rate limiting to prevent abuse.
- Log validation failures for monitoring.