-- Migration: Add operator_id column to bus_layout table
-- This migration adds operator association to layouts for data isolation

ALTER TABLE bus_layout 
ADD COLUMN operator_id UUID;

-- Add comment to document the column purpose
COMMENT ON COLUMN bus_layout.operator_id IS 'ID of the operator who owns this layout';

-- Add foreign key constraint to ensure data integrity
ALTER TABLE bus_layout 
ADD CONSTRAINT fk_bus_layout_operator 
FOREIGN KEY (operator_id) REFERENCES bus_operator(id);

-- Create index for faster queries by operator
CREATE INDEX idx_bus_layout_operator_id ON bus_layout(operator_id);
