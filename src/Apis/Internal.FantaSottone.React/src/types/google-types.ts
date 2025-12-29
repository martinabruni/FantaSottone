// Aggiungi questi tipi al file src/types/dto.ts

/**
 * Google authentication request
 */
export interface GoogleAuthRequest {
  idToken: string;
}

/**
 * Google authentication response from backend
 */
export interface GoogleAuthResponse {
  token: string;
  email: string;
  userId: number;
  isFirstLogin: boolean;
}
