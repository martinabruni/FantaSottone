// Typed HTTP errors for consistent error handling

export class HttpError extends Error {
  constructor(
    public statusCode: number,
    public statusText: string,
    public body?: unknown
  ) {
    super(`HTTP ${statusCode}: ${statusText}`);
    this.name = "HttpError";
  }
}

export class UnauthorizedError extends HttpError {
  constructor(message = "Unauthorized") {
    super(401, message);
    this.name = "UnauthorizedError";
  }
}

export class ForbiddenError extends HttpError {
  constructor(message = "Forbidden") {
    super(403, message);
    this.name = "ForbiddenError";
  }
}

export class NotFoundError extends HttpError {
  constructor(message = "Not Found") {
    super(404, message);
    this.name = "NotFoundError";
  }
}

export class ConflictError extends HttpError {
  constructor(message = "Conflict") {
    super(409, message);
    this.name = "ConflictError";
  }
}

export class ServerError extends HttpError {
  constructor(message = "Internal Server Error") {
    super(500, message);
    this.name = "ServerError";
  }
}

export function createHttpError(
  status: number,
  statusText: string,
  body?: unknown
): HttpError {
  switch (status) {
    case 401:
      return new UnauthorizedError(statusText);
    case 403:
      return new ForbiddenError(statusText);
    case 404:
      return new NotFoundError(statusText);
    case 409:
      return new ConflictError(statusText);
    case 500:
      return new ServerError(statusText);
    default:
      return new HttpError(status, statusText, body);
  }
}
