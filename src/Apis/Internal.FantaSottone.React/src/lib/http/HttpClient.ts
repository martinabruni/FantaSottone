import { createHttpError } from "./errors";
import { ITransport } from "./Transport";

export interface HttpClientConfig {
  baseUrl: string;
  getToken?: () => string | null;
}

export class HttpClient implements ITransport {
  private baseUrl: string;
  private getToken?: () => string | null;

  constructor(config: HttpClientConfig) {
    this.baseUrl = config.baseUrl;
    this.getToken = config.getToken;
  }

  private getHeaders(
    customHeaders?: Record<string, string>
  ): Record<string, string> {
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
      ...customHeaders,
    };

    const token = this.getToken?.();
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    return headers;
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      let errorBody: unknown;
      try {
        errorBody = await response.json();
      } catch {
        errorBody = await response.text();
      }
      throw createHttpError(response.status, response.statusText, errorBody);
    }

    const text = await response.text();
    if (!text) {
      return undefined as T;
    }

    try {
      return JSON.parse(text) as T;
    } catch {
      return text as T;
    }
  }

  async get<T>(url: string, headers?: Record<string, string>): Promise<T> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: "GET",
      headers: this.getHeaders(headers),
    });
    return this.handleResponse<T>(response);
  }

  async post<TReq, TRes>(
    url: string,
    data: TReq,
    headers?: Record<string, string>
  ): Promise<TRes> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: "POST",
      headers: this.getHeaders(headers),
      body: JSON.stringify(data),
    });
    return this.handleResponse<TRes>(response);
  }

  async put<TReq, TRes>(
    url: string,
    data: TReq,
    headers?: Record<string, string>
  ): Promise<TRes> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: "PUT",
      headers: this.getHeaders(headers),
      body: JSON.stringify(data),
    });
    return this.handleResponse<TRes>(response);
  }

  async delete<T>(url: string, headers?: Record<string, string>): Promise<T> {
    const response = await fetch(`${this.baseUrl}${url}`, {
      method: "DELETE",
      headers: this.getHeaders(headers),
    });
    return this.handleResponse<T>(response);
  }
}
