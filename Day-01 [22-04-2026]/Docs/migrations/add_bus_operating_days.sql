-- Add operating days functionality to bus system
-- Migration: Add bus operating days and owner contact details

-- Add new columns to bus table for owner contact information
ALTER TABLE bus 
ADD COLUMN owner_phone VARCHAR(20),
ADD COLUMN owner_email VARCHAR(100);

-- Create bus_operating_days table
CREATE TABLE bus_operating_days (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bus_id UUID NOT NULL,
    day_of_week INTEGER NOT NULL, -- 1=Monday, 2=Tuesday, ..., 7=Sunday
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (bus_id) REFERENCES bus(id) ON DELETE CASCADE,
    UNIQUE(bus_id, day_of_week)
);

-- Create index for performance
CREATE INDEX idx_bus_operating_days_bus_id ON bus_operating_days(bus_id);

-- Insert default operating days (all days active) for existing buses
INSERT INTO bus_operating_days (bus_id, day_of_week, is_active)
SELECT id, day, true
FROM bus, generate_series(1, 7) AS day
WHERE id IS NOT NULL;

-- Add updated_at trigger
CREATE OR REPLACE FUNCTION update_bus_operating_days_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER bus_operating_days_updated_at
    BEFORE UPDATE ON bus_operating_days
    FOR EACH ROW
    EXECUTE FUNCTION update_bus_operating_days_updated_at();
