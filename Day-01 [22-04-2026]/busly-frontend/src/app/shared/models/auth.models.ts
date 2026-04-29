export interface RegisterCustomerRequest {
  username: string;
  email: string;
  password: string;
  acceptedTerms: boolean;
}

export interface RegisterOperatorRequest {
  companyName: string;
  email: string;
  password: string;
  phone?: string;
  acceptedTerms: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// Server no longer returns the token — it is set as an HttpOnly cookie.
export interface LoginResponse {
  role: string;
  email: string;
  userId: string;
}

export interface UserProfile {
  userId: string;
  email: string;
  role: string;
  name?: string;
  tcAccepted: boolean;
  tcVersion?: string;
}

export interface CurrentUser {
  userId: string;
  email: string;
  role: 'Admin' | 'Operator' | 'Customer';
  token: string; // always empty string — token lives in HttpOnly cookie
}

export interface TcVersionDto {
  id: string;
  version: string;
  content: string;
  publishedAt: string;
  effectiveAt?: string;
  isActive: boolean;
}

export interface TcStatusDto {
  hasAcceptedTc: boolean;
  lastAcceptedVersion?: string;
  lastAcceptedAt?: string;
  currentActiveVersion?: string;
  needsToAcceptCurrent: boolean;
}
