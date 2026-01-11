# API Error Codes and Response Formats

This document provides a comprehensive guide to error handling in the TestNest Admin API, including HTTP status codes, error response formats, and common error scenarios.

## Table of Contents

- [Error Response Format](#error-response-format)
- [HTTP Status Codes](#http-status-codes)
- [Error Types](#error-types)
- [Common Error Scenarios](#common-error-scenarios)
- [Validation Errors](#validation-errors)
- [Authentication and Authorization Errors](#authentication-and-authorization-errors)
- [Resource Errors](#resource-errors)
- [Server Errors](#server-errors)

---

## Error Response Format

All API errors follow the [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) specification and return a consistent `ProblemDetails` object.

### Standard Error Response Structure

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00",
  "errors": [
    {
      "code": "InvalidEmployeeNumber",
      "message": "Employee number must be in the format EMP-YYYY-NNN"
    }
  ]
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `type` | string | A URI reference that identifies the problem type |
| `title` | string | A short, human-readable summary of the problem type |
| `status` | integer | The HTTP status code |
| `detail` | string | A human-readable explanation specific to this occurrence |
| `instance` | string | A URI reference that identifies the specific occurrence |
| `traceId` | string | Unique identifier for tracing the request through the system |
| `errors` | array | Collection of error details (optional, present for validation errors) |

---

## HTTP Status Codes

The API uses standard HTTP status codes to indicate the success or failure of requests.

### Success Codes (2xx)

| Code | Name | Description | Usage |
|------|------|-------------|-------|
| `200` | OK | Request succeeded | GET, PUT, PATCH requests returning data |
| `201` | Created | Resource created successfully | POST requests |
| `204` | No Content | Request succeeded with no response body | DELETE requests |

### Client Error Codes (4xx)

| Code | Name | Description | Usage |
|------|------|-------------|-------|
| `400` | Bad Request | Invalid request data or validation failure | Malformed JSON, validation errors |
| `401` | Unauthorized | Authentication required or failed | Missing/invalid JWT token |
| `403` | Forbidden | Authenticated but lacks permissions | Insufficient role/policy permissions |
| `404` | Not Found | Resource does not exist | Invalid ID, deleted resource |
| `409` | Conflict | Request conflicts with current state | Duplicate entries, constraint violations |
| `429` | Too Many Requests | Rate limit exceeded | Exceeding API rate limits |

### Server Error Codes (5xx)

| Code | Name | Description | Usage |
|------|------|-------------|-------|
| `500` | Internal Server Error | Unexpected server error | Unhandled exceptions |
| `503` | Service Unavailable | Service temporarily unavailable | Database connection failures, circuit breaker open |

---

## Error Types

The API categorizes errors into the following types:

### 1. Validation Errors

**HTTP Status Code:** `400 Bad Request`

**Error Type Enum:** `ErrorType.Validation`

**Description:** Input data fails validation rules.

**Example Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "Validation failed.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-abc123-00",
  "errors": [
    {
      "code": "InvalidEmailAddress",
      "message": "Email address must be in a valid format"
    },
    {
      "code": "RequiredField",
      "message": "First name is required"
    }
  ]
}
```

### 2. Not Found Errors

**HTTP Status Code:** `404 Not Found`

**Error Type Enum:** `ErrorType.NotFound`

**Description:** Requested resource does not exist or has been deleted (soft delete).

**Example Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Resource not found.",
  "instance": "/api/v1.0/employees/550e8400-e29b-41d4-a716-446655440000",
  "traceId": "00-def456-00",
  "errors": [
    {
      "code": "EmployeeNotFound",
      "message": "Employee with ID '550e8400-e29b-41d4-a716-446655440000' was not found"
    }
  ]
}
```

### 3. Conflict Errors

**HTTP Status Code:** `409 Conflict`

**Error Type Enum:** `ErrorType.Conflict`

**Description:** Request conflicts with existing data or business rules.

**Example Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Conflict occurred.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-ghi789-00",
  "errors": [
    {
      "code": "DuplicateEmployeeNumber",
      "message": "An employee with number 'EMP-2024-001' already exists"
    }
  ]
}
```

### 4. Unauthorized Errors

**HTTP Status Code:** `401 Unauthorized`

**Error Type Enum:** `ErrorType.Unauthorized`

**Description:** Authentication failed or token is invalid/expired.

**Example Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed.",
  "instance": "/api/v1.0/auth/login",
  "traceId": "00-jkl012-00",
  "errors": [
    {
      "code": "InvalidCredentials",
      "message": "Invalid email or password"
    }
  ]
}
```

---

## Common Error Scenarios

### Employee Management Errors

#### Creating Employee

**Duplicate Employee Number**
```json
{
  "status": 409,
  "title": "Conflict",
  "errors": [{
    "code": "DuplicateEmployeeNumber",
    "message": "Employee number 'EMP-2024-001' is already in use"
  }]
}
```

**Duplicate Email Address**
```json
{
  "status": 409,
  "title": "Conflict",
  "errors": [{
    "code": "DuplicateEmailAddress",
    "message": "Email address 'john.doe@testnest.com' is already registered"
  }]
}
```

**Invalid Role ID**
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [{
    "code": "InvalidEmployeeRoleId",
    "message": "Employee role with ID '550e8400-e29b-41d4-a716-446655440000' does not exist"
  }]
}
```

#### Updating Employee

**Employee Not Found**
```json
{
  "status": 404,
  "title": "Not Found",
  "errors": [{
    "code": "EmployeeNotFound",
    "message": "Employee with ID '550e8400-e29b-41d4-a716-446655440000' was not found"
  }]
}
```

#### Deleting Employee

**Employee Has Dependencies**
```json
{
  "status": 409,
  "title": "Conflict",
  "errors": [{
    "code": "EmployeeHasDependencies",
    "message": "Cannot delete employee as they have active assignments"
  }]
}
```

### Establishment Management Errors

#### Creating Establishment

**Duplicate Establishment Name**
```json
{
  "status": 409,
  "title": "Conflict",
  "errors": [{
    "code": "DuplicateEstablishmentName",
    "message": "Establishment with name 'TestNest Main Branch' already exists"
  }]
}
```

**Invalid Email Format**
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [{
    "code": "InvalidEmailFormat",
    "message": "Email address must be in a valid format (e.g., user@example.com)"
  }]
}
```

### Authentication Errors

#### Login

**Invalid Credentials**
```json
{
  "status": 401,
  "title": "Unauthorized",
  "errors": [{
    "code": "InvalidCredentials",
    "message": "Invalid email or password"
  }]
}
```

**Account Locked**
```json
{
  "status": 401,
  "title": "Unauthorized",
  "errors": [{
    "code": "AccountLocked",
    "message": "Account is locked due to multiple failed login attempts"
  }]
}
```

#### Token Refresh

**Invalid Refresh Token**
```json
{
  "status": 401,
  "title": "Unauthorized",
  "errors": [{
    "code": "InvalidRefreshToken",
    "message": "The provided refresh token is invalid or expired"
  }]
}
```

**Token Revoked**
```json
{
  "status": 401,
  "title": "Unauthorized",
  "errors": [{
    "code": "TokenRevoked",
    "message": "The refresh token has been revoked"
  }]
}
```

---

## Validation Errors

### Common Validation Error Codes

| Code | Message | Field Type | Example |
|------|---------|------------|---------|
| `RequiredField` | "{Field} is required" | All | "First name is required" |
| `InvalidFormat` | "{Field} must be in a valid format" | String | "Email must be in a valid format" |
| `StringTooLong` | "{Field} must not exceed {MaxLength} characters" | String | "Last name must not exceed 50 characters" |
| `StringTooShort` | "{Field} must be at least {MinLength} characters" | String | "Password must be at least 8 characters" |
| `InvalidRange` | "{Field} must be between {Min} and {Max}" | Numeric | "Page size must be between 1 and 100" |
| `InvalidGuid` | "{Field} must be a valid GUID" | ID | "Employee ID must be a valid GUID" |
| `InvalidPhoneNumber` | "Phone number must be in a valid format" | Phone | "Phone number must start with +63" |
| `InvalidDate` | "Date must be in a valid format" | DateTime | "Date must be in ISO 8601 format" |

### Field-Specific Validation

#### Employee Fields

| Field | Validation Rules | Example Error |
|-------|------------------|---------------|
| EmployeeNumber | Required, Format: `EMP-YYYY-NNN` | "Employee number must be in the format EMP-YYYY-NNN" |
| FirstName | Required, MaxLength: 50 | "First name must not exceed 50 characters" |
| LastName | Required, MaxLength: 50 | "Last name must not exceed 50 characters" |
| EmailAddress | Required, Valid email format | "Email address must be in a valid format" |
| EmployeeRoleId | Required, Valid GUID, Must exist | "Employee role must be selected" |
| EstablishmentId | Required, Valid GUID, Must exist | "Establishment must be selected" |

#### Establishment Fields

| Field | Validation Rules | Example Error |
|-------|------------------|---------------|
| EstablishmentName | Required, MaxLength: 100 | "Establishment name must not exceed 100 characters" |
| EmailAddress | Required, Valid email format | "Email address must be in a valid format" |

#### Address Fields

| Field | Validation Rules | Example Error |
|-------|------------------|---------------|
| Street | Optional, MaxLength: 200 | "Street must not exceed 200 characters" |
| City | Required, MaxLength: 100 | "City is required" |
| Province | Required, MaxLength: 100 | "Province is required" |
| Region | Required, MaxLength: 100 | "Region is required" |
| PostalCode | Optional, MaxLength: 20 | "Postal code must not exceed 20 characters" |

---

## Authentication and Authorization Errors

### JWT Token Errors

| Scenario | Status Code | Error Code | Message |
|----------|-------------|------------|---------|
| Missing token | 401 | `MissingToken` | "Authorization header is missing" |
| Invalid token format | 401 | `InvalidTokenFormat` | "Authorization header must be in 'Bearer {token}' format" |
| Expired token | 401 | `TokenExpired` | "JWT token has expired" |
| Invalid signature | 401 | `InvalidSignature` | "JWT token signature is invalid" |
| Token not yet valid | 401 | `TokenNotYetValid` | "JWT token is not yet valid" |

### Authorization Policy Errors

| Policy | Required Role | Status Code | Error Message |
|--------|---------------|-------------|---------------|
| RequireAdmin | Admin | 403 | "This action requires Administrator privileges" |
| RequireManager | Manager | 403 | "This action requires Manager privileges" |
| RequireManagerOrAdmin | Manager or Admin | 403 | "This action requires Manager or Administrator privileges" |
| RequireStaff | Staff | 403 | "This action requires Staff privileges" |

---

## Resource Errors

### Pagination Errors

```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [{
    "code": "InvalidPageNumber",
    "message": "Page number must be greater than 0"
  }]
}
```

```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [{
    "code": "InvalidPageSize",
    "message": "Page size must be between 1 and 100"
  }]
}
```

### Sorting Errors

```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [{
    "code": "InvalidSortField",
    "message": "Sort field 'InvalidField' is not supported. Supported fields: EmployeeId, EmployeeNumber, FirstName, LastName, EmailAddress"
  }]
}
```

### Filtering Errors

```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [{
    "code": "InvalidFilterValue",
    "message": "Filter value for 'EmployeeRoleId' must be a valid GUID"
  }]
}
```

---

## Server Errors

### Database Errors

**Connection Failure**
```json
{
  "status": 503,
  "title": "Service Unavailable",
  "detail": "Database connection is currently unavailable. Please try again later.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-xyz789-00"
}
```

**Timeout**
```json
{
  "status": 500,
  "title": "Internal Server Error",
  "detail": "The database query timed out. Please try again or contact support if the issue persists.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-abc123-00"
}
```

### Circuit Breaker Errors

**Circuit Open**
```json
{
  "status": 503,
  "title": "Service Unavailable",
  "detail": "Service is temporarily unavailable due to high error rates. Please try again in a few moments.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-def456-00"
}
```

### Rate Limiting Errors

**Rate Limit Exceeded**
```json
{
  "status": 429,
  "title": "Too Many Requests",
  "detail": "API rate limit exceeded. Please reduce your request rate.",
  "instance": "/api/v1.0/employees",
  "traceId": "00-ghi789-00",
  "headers": {
    "X-RateLimit-Limit": "100",
    "X-RateLimit-Remaining": "0",
    "X-RateLimit-Reset": "2024-01-11T10:30:00Z"
  }
}
```

---

## Best Practices for Error Handling

### For API Consumers

1. **Always check the HTTP status code** before parsing the response body
2. **Use the `traceId`** when reporting issues to support
3. **Implement retry logic** for 5xx errors with exponential backoff
4. **Don't retry 4xx errors** (except 429 Too Many Requests) without fixing the request
5. **Handle token expiration** by implementing automatic token refresh
6. **Parse the `errors` array** for detailed validation error information

### For API Developers

1. **Always return consistent error formats** using `ProblemDetails`
2. **Include meaningful error codes** in the `errors` array
3. **Log all errors** with full context using structured logging
4. **Never expose sensitive information** in error messages
5. **Use appropriate HTTP status codes** for different error types
6. **Include correlation IDs** (`traceId`) for request tracing

---

## Error Handling Examples

### Client-Side Example (TypeScript)

```typescript
async function createEmployee(employeeData: EmployeeForCreationRequest) {
  try {
    const response = await fetch('/api/v1.0/employees', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify(employeeData)
    });

    if (!response.ok) {
      const problemDetails = await response.json();

      switch (response.status) {
        case 400:
          // Handle validation errors
          const validationErrors = problemDetails.errors;
          console.error('Validation failed:', validationErrors);
          break;
        case 401:
          // Handle unauthorized - refresh token or redirect to login
          await refreshToken();
          break;
        case 403:
          // Handle forbidden - show permission error
          console.error('Insufficient permissions');
          break;
        case 409:
          // Handle conflict - show duplicate error
          console.error('Employee already exists');
          break;
        case 503:
          // Handle service unavailable - retry with backoff
          setTimeout(() => createEmployee(employeeData), 5000);
          break;
        default:
          // Handle unexpected errors
          console.error(`Error ${response.status}: ${problemDetails.detail}`);
      }

      return null;
    }

    return await response.json();
  } catch (error) {
    console.error('Network error:', error);
    return null;
  }
}
```

### Server-Side Logging Example

```csharp
private IActionResult HandleErrorResponse(ErrorType errorType, IEnumerable<Error> errors)
{
    List<Error> safeErrors = errors?.ToList() ?? [];

    // Log the error with context
    _logger.LogWarning(
        "Request failed with error type {ErrorType}. Errors: {@Errors}",
        errorType,
        safeErrors
    );

    return errorType switch
    {
        ErrorType.Validation =>
            _errorResponseService.CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "Validation failed.",
                new Dictionary<string, object> { { "errors", safeErrors } }
            ),
        ErrorType.NotFound =>
            _errorResponseService.CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Not Found",
                "Resource not found.",
                new Dictionary<string, object> { { "errors", safeErrors } }
            ),
        // ... other error types
    };
}
```

---

## Support and Troubleshooting

When encountering errors:

1. **Check the API documentation** for endpoint-specific requirements
2. **Verify your request format** matches the expected schema
3. **Ensure proper authentication** with a valid JWT token
4. **Check your permissions** for the requested operation
5. **Use the `traceId`** to search logs when reporting issues
6. **Review the `errors` array** for specific validation failures

For persistent issues, contact the development team with:
- The `traceId` from the error response
- Full error response body
- Request details (endpoint, method, headers, body)
- Timestamp of the request
