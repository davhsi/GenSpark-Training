export interface BusSearchResultDto {
  busId: string;
  busName?: string;
  busNumber?: string;
  operatorName?: string;
  sourceCity?: string;
  destinationCity?: string;
  basePrice?: number;
  availableSeats: number;
}

export interface SeatAvailabilityDto {
  seatId: string;
  seatNumber?: number;
  seatType?: string;
  deck?: string;
  status: string;  // 'AVAILABLE' | 'LOCKED' | 'BOOKED'
  passengerGender?: string;
}

export interface SeatMapResponse {
  layoutConfig?: string;  // raw JSON string of seat_config
  seatStatuses: SeatAvailabilityDto[];
}

export interface CreateSeatLockRequest {
  seatId: string;
  busId: string;
  journeyDate: string;  // 'yyyy-MM-dd'
}

export interface SeatLockDto {
  lockId: string;
  seatId: string;
  busId: string;
  journeyDate: string;
  expiresAt?: string;
}
