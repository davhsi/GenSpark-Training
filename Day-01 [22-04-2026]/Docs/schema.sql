-- ======================
-- EXTENSIONS
-- ======================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ======================
-- PLATFORM CONFIG
-- ======================
CREATE TABLE platform_config (
    key TEXT PRIMARY KEY,
    value TEXT
);

-- ======================
-- ADMIN
-- ======================
CREATE TABLE admin (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL
);

-- ======================
-- CUSTOMER
-- ======================
CREATE TABLE customer (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    name TEXT,
    age INT,
    gender TEXT,
    tc_accepted BOOLEAN DEFAULT FALSE,
    tc_version TEXT,
    tc_accepted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ======================
-- BUS OPERATOR
-- ======================
CREATE TABLE bus_operator (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    phone TEXT,
    status TEXT CHECK (status IN ('PENDING','APPROVED','DISABLED','REJECTED')),
    tc_accepted BOOLEAN DEFAULT FALSE,
    tc_version TEXT,
    tc_accepted_at TIMESTAMP,
    approved_by_admin UUID REFERENCES admin(id),
    approved_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ======================
-- ROUTE
-- ======================
CREATE TABLE route (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_city TEXT NOT NULL,
    destination_city TEXT NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_by_admin UUID REFERENCES admin(id),
    UNIQUE(source_city, destination_city)
);

-- ======================
-- BUS LAYOUT
-- ======================
CREATE TABLE bus_layout (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    operator_id UUID NOT NULL REFERENCES bus_operator(id),
    layout_name TEXT,
    total_seats INT,
    seat_config JSONB
);

-- ======================
-- BUS
-- ======================
CREATE TABLE bus (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    operator_id UUID REFERENCES bus_operator(id),
    route_id UUID REFERENCES route(id),
    bus_number TEXT,
    bus_name TEXT,
    owner_name TEXT,
    status TEXT CHECK (status IN ('PENDING','ACTIVE','DISABLED','REMOVED')),
    base_price NUMERIC(10,2),
    layout_id UUID REFERENCES bus_layout(id),
    driver_name TEXT,
    driver_phone TEXT,
    conductor_name TEXT,
    conductor_phone TEXT,
    approved_by_admin UUID REFERENCES admin(id),
    approved_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ======================
-- SEAT
-- ======================
CREATE TABLE seat (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    layout_id UUID REFERENCES bus_layout(id),
    seat_number INT,
    seat_type TEXT,
    deck TEXT,
    row_num INT,
    col_num INT
);

-- ======================
-- BUS STOP (merged boarding + dropping)
-- ======================
CREATE TABLE bus_stop (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    bus_id UUID REFERENCES bus(id),
    type TEXT CHECK (type IN ('BOARDING', 'DROPPING')),
    city TEXT,
    address TEXT,
    scheduled_time TIME
);

-- ======================
-- BOOKING
-- NOTE: coupon_id FK is added via ALTER TABLE after coupon is created,
--       because booking → coupon → cancellation → booking is a circular
--       dependency that must be broken at one point.
-- ======================
CREATE TABLE booking (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID REFERENCES customer(id),
    bus_id UUID REFERENCES bus(id),
    journey_date DATE NOT NULL,
    base_fare NUMERIC(10,2),
    convenience_fee NUMERIC(10,2),
    total_amount NUMERIC(10,2),
    status TEXT CHECK (status IN ('INITIATED','PAYMENT_PENDING','CONFIRMED','CANCELLED','REFUNDED')),
    booked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    coupon_id UUID  -- FK added below after coupon table is created
);

-- ======================
-- CANCELLATION
-- Must come after booking (references booking) and before coupon
-- (coupon references cancellation).
-- ======================
CREATE TABLE cancellation (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id UUID REFERENCES booking(id),
    cancelled_by TEXT CHECK (cancelled_by IN ('customer','operator')),
    cancelled_at TIMESTAMP,
    refund_amount NUMERIC(10,2),
    refund_status TEXT CHECK (refund_status IN ('PENDING','PROCESSED','FAILED'))
);

-- ======================
-- COUPON
-- Must come after cancellation (references cancellation) and after
-- customer (references customer).  booking.coupon_id FK is wired up
-- immediately after this table via ALTER TABLE.
-- ======================
CREATE TABLE coupon (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code TEXT UNIQUE NOT NULL,
    discount_value NUMERIC(10,2),
    discount_type TEXT CHECK (discount_type IN ('flat','percent')),
    issued_to_customer UUID REFERENCES customer(id),
    cancellation_id UUID REFERENCES cancellation(id),
    is_used BOOLEAN DEFAULT FALSE,
    expires_at TIMESTAMP
);

-- Wire up the deferred FK: booking → coupon
ALTER TABLE booking
    ADD CONSTRAINT fk_booking_coupon
    FOREIGN KEY (coupon_id) REFERENCES coupon(id);

-- ======================
-- BOOKED SEAT
-- ======================
CREATE TABLE booked_seat (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id UUID REFERENCES booking(id) ON DELETE CASCADE,
    seat_id UUID REFERENCES seat(id),
    bus_id UUID REFERENCES bus(id),
    journey_date DATE NOT NULL, -- denormalized from booking for index support
    passenger_name TEXT,
    passenger_age INT,
    passenger_gender TEXT
);

-- ======================
-- SEAT LOCK
-- ======================
CREATE TABLE seat_lock (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    seat_id UUID REFERENCES seat(id),
    customer_id UUID REFERENCES customer(id),
    bus_id UUID REFERENCES bus(id),
    journey_date DATE NOT NULL,
    locked_at TIMESTAMP,
    expires_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- ======================
-- PAYMENT
-- ======================
CREATE TABLE payment (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id UUID REFERENCES booking(id),
    amount NUMERIC(10,2),
    status TEXT CHECK (status IN ('PENDING','SUCCESS','FAILED','REFUNDED')),
    transaction_ref TEXT,
    paid_at TIMESTAMP
);

-- ======================
-- NOTIFICATION
-- XOR check: exactly one of customer_id / operator_id must be non-null.
-- ======================
CREATE TABLE notification (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID REFERENCES customer(id),
    operator_id UUID REFERENCES bus_operator(id),
    type TEXT,
    message TEXT,
    status TEXT,
    sent_at TIMESTAMP,
    CHECK (
        (customer_id IS NOT NULL AND operator_id IS NULL) OR
        (customer_id IS NULL AND operator_id IS NOT NULL)
    )
);

-- ======================
-- T&C VERSION
-- ======================
CREATE TABLE tc_version (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    version TEXT NOT NULL UNIQUE,
    content TEXT NOT NULL,
    published_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    effective_at TIMESTAMP,
    published_by_admin UUID REFERENCES admin(id),
    is_active BOOLEAN DEFAULT FALSE
);

-- ======================
-- AUDIT LOG
-- ======================
CREATE TABLE audit_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    actor_id UUID,
    actor_role TEXT CHECK (actor_role IN ('admin','customer','operator')),
    action TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id UUID,
    metadata JSONB,
    performed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ======================
-- INDEXES
-- ======================

-- Fast seat lock lookup (used on every seat map load)
CREATE INDEX idx_seat_lock_lookup
    ON seat_lock (seat_id, bus_id, journey_date, is_active);

-- Prevent double booking at DB level
CREATE UNIQUE INDEX unique_seat_per_trip
    ON booked_seat (seat_id, bus_id, journey_date);

-- Booking history queries
CREATE INDEX idx_booking_customer ON booking (customer_id);
CREATE INDEX idx_booking_bus      ON booking (bus_id);

-- Fuzzy city search (requires pg_trgm extension)
CREATE INDEX idx_route_source_trgm
    ON route USING gin (source_city gin_trgm_ops);
CREATE INDEX idx_route_destination_trgm
    ON route USING gin (destination_city gin_trgm_ops);
