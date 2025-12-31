// Aggiungi questi tipi al file src/types/dto.ts o crea un nuovo file src/types/user-types.ts

import { GameStatus } from "./entities";

/**
 * User profile information
 */
export interface UserProfileDto {
  userId: number;
  email: string;
  profileImageUrl?: string;
  createdAt: string;
}

/**
 * Response containing user profile
 */
export interface GetUserProfileResponse {
  profile: UserProfileDto;
}

/**
 * Game invitation DTO
 */
export interface GameInvitationDto {
  gameId: number;
  gameName: string;
  initialScore: number;
  status: GameStatus;
  playerCount: number;
  createdAt: string;
}

/**
 * Response containing user's game invitations
 */
export interface GetUserGamesResponse {
  games: GameInvitationDto[];
}

/**
 * Request to create a new game with email invites
 */
export interface CreateGameRequest {
  name: string;
  initialScore: number;
  invitedEmails: string[];
}

/**
 * Response after creating a game
 */
export interface CreateGameResponse {
  gameId: number;
  gameName: string;
  creatorPlayerId: number;
  invitedEmails: string[];
  invalidEmails: string[];
}
