import { ITransport } from "@/lib/http/Transport";

export interface EmailAuthResponse {
  token: string;
  email: string;
  userId: number;
}

export interface EmailAuthRequest {
  email: string;
  password: string;
}

export class EmailAuthStrategy {
  private readonly SESSION_KEY = "email_auth_session";
  private transport: ITransport;

  constructor(transport: ITransport) {
    this.transport = transport;
  }

  async register(email: string, password: string): Promise<EmailAuthResponse> {
    const response = await this.transport.post<
      EmailAuthRequest,
      EmailAuthResponse
    >("/api/Auth/register", { email, password });

    return response;
  }

  async login(email: string, password: string): Promise<EmailAuthResponse> {
    const response = await this.transport.post<
      EmailAuthRequest,
      EmailAuthResponse
    >("/api/Auth/login", { email, password });

    return response;
  }

  async logout(): Promise<void> {
    this.clearSession();
  }

  clearSession(): void {
    localStorage.removeItem(this.SESSION_KEY);
  }
}
