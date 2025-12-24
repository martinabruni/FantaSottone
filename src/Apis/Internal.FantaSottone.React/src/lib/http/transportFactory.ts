import { HttpClient } from "@/lib/http/HttpClient";
import { ITransport } from "@/lib/http/Transport";
import { MockTransport } from "@/mocks/MockTransport";

export function createTransport(getToken?: () => string | null): ITransport {
  const useMocks = import.meta.env.VITE_USE_MOCKS === "true";
  const baseUrl = import.meta.env.VITE_API_BASE_URL || "http://localhost:5001";

  if (useMocks) {
    return new MockTransport();
  }

  return new HttpClient({ baseUrl, getToken });
}
