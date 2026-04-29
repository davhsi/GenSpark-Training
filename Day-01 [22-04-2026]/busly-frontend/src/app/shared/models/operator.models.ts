export interface SeatItemDto {
  seatNumber: number;
  row: number;
  col: number;
  type: string;   // 'window' | 'aisle'
  deck: string;   // 'lower' | 'upper'
}

export interface SeatConfigDto {
  rows: number;
  cols: number;
  decks: string[];
  seats: SeatItemDto[];
}

export interface CreateLayoutRequest {
  layoutName: string;
  seatConfig: SeatConfigDto;
}

export interface LayoutDto {
  id: string;
  layoutName?: string;
  totalSeats?: number;
  seatConfig?: SeatConfigDto;
}

export interface OperatingDayDto {
  dayOfWeek: number; // 1=Monday, 2=Tuesday, ..., 7=Sunday
  isActive: boolean;
}

export interface UpdateOperatingDaysRequest {
  busId: string;
  operatingDays: OperatingDayDto[];
}

export interface RegisterBusRequest {
  routeId: string;
  layoutId: string;
  busNumber: string;
  busName: string;
  ownerName: string;
  ownerPhone?: string;
  ownerEmail?: string;
  driverName?: string;
  driverPhone?: string;
  conductorName?: string;
  conductorPhone?: string;
  basePrice: number;
  operatingDays?: OperatingDayDto[];
}

export interface BusDetailDto {
  id: string;
  busNumber?: string;
  busName?: string;
  ownerName?: string;
  ownerPhone?: string;
  ownerEmail?: string;
  driverName?: string;
  driverPhone?: string;
  conductorName?: string;
  conductorPhone?: string;
  status?: string;
  basePrice?: number;
  routeId?: string;
  sourceCity?: string;
  destinationCity?: string;
  layoutId?: string;
  createdAt?: string;
  operatingDays?: OperatingDayDto[];
  stops?: BusStopDto[];
}

export interface BusStopDto {
  id: string;
  type: string;
  city: string;
  address: string;
  scheduledTime: string;
}

export interface AddBusStopRequest {
  type: string;   // 'BOARDING' | 'DROPPING'
  city: string;
  address: string;
  scheduledTime: string;  // 'HH:mm'
}

export interface UpdatePriceRequest {
  basePrice: number;
}

export interface OperatorProfileDto {
  id: string;
  companyName: string;
  email: string;
  phone?: string;
  status: string;
  approvedAt?: string;
  createdAt?: string;
  isApproved: boolean;
  isPending: boolean;
  isRejected: boolean;
  isDisabled: boolean;
}
