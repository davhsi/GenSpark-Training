export interface RouteDto {
  id: string;
  sourceCity: string;
  destinationCity: string;
  isActive: boolean;
}

export interface CreateRouteRequest {
  sourceCity: string;
  destinationCity: string;
}

export interface OperatorDto {
  id: string;
  companyName: string;
  email: string;
  phone?: string;
  contactEmail?: string;
  contactPhone?: string;
  status: 'PENDING' | 'APPROVED' | 'REJECTED' | 'DISABLED';
  createdAt?: string;
  tcVersion?: string;
  tcAcceptedAt?: string;
}

export interface OperatingDayDto {
  dayOfWeek: number; // 1=Monday, 2=Tuesday, ..., 7=Sunday
  isActive: boolean;
}

export interface BusDto {
  id: string;
  busNumber?: string;
  busName?: string;
  ownerName?: string;
  ownerPhone?: string;
  ownerEmail?: string;
  status?: string;
  operatorId?: string;
  routeId?: string;
  basePrice?: number;
  driverName?: string;
  driverPhone?: string;
  conductorName?: string;
  conductorPhone?: string;
  sourceCity?: string;
  destinationCity?: string;
  layoutName?: string;
  createdAt?: string;
  operatingDays?: OperatingDayDto[];
}

export interface MonthlyRevenueDto {
  year: number;
  month: number;
  totalConvenienceFee: number;
}

export interface ConvenienceFeeConfig {
  feeType: 'flat' | 'percent';
  feeValue: number;
}

export interface UpdateConvenienceFeeRequest {
  feeType: 'flat' | 'percent';
  feeValue: number;
}

export interface OperatorRevenueDto {
  operatorName?: string;
  bookingCount: number;
  totalBaseFare: number;
  totalConvenienceFee: number;
}

export interface AuditLogDto {
  id: string;
  actorId: string;
  actorRole: string;
  action: string;
  entityType: string;
  entityId?: string;
  metadata?: string;
  timestamp?: string;
}

// Re-export TcVersionDto from auth.models to avoid duplication
export type { TcVersionDto } from './auth.models';
