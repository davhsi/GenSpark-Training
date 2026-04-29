export interface PassengerDto {
  seatId: string;
  name: string;
  age: number;
  gender: string;
}

export interface CreateBookingRequest {
  busId: string;
  journeyDate: string;  // 'yyyy-MM-dd'
  passengers: PassengerDto[];
  couponCode?: string;
}

export interface BookedSeatDto {
  seatId: string;
  passengerName?: string;
  passengerAge?: number;
  passengerGender?: string;
}

export interface BookingDto {
  id: string;
  pnr: string;
  busId: string;
  journeyDate: string;
  departureTime?: string;  // 'HH:mm' format for actual bus departure time
  baseFare?: number;
  convenienceFee?: number;
  totalAmount?: number;
  status?: string;
  bookedAt?: string;
  seats: BookedSeatDto[];
}

export interface CancellationDto {
  cancellationId: string;
  bookingId: string;
  cancelledBy?: string;
  refundAmount?: number;
  refundStatus?: string;
  cancelledAt?: string;
}
