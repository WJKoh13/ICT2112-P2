-- ============================================================
-- Delivery Routes
-- ============================================================

INSERT INTO delivery_route
(origin_address, destination_address, total_distance_km, is_valid, origin_hub_id, destination_hub_id)
VALUES
-- Route 1: Singapore warehouse -> Hong Kong warehouse via sea
('1 Marina Boulevard, Singapore', '123 Industrial Road, Hong Kong', 2580.0, TRUE, 1, 4),

-- Route 2: Singapore warehouse -> Hong Kong warehouse via air
('1 Marina Boulevard, Singapore', '123 Industrial Road, Hong Kong', 2560.0, TRUE, 1, 4),

-- Route 3: Singapore warehouse -> Port of Singapore outbound
('1 Marina Boulevard, Singapore', 'Port of Singapore, Pasir Ris, Singapore', 18.0, TRUE, 1, 2),

-- Route 4: Singapore warehouse -> Changi Airport outbound
('1 Marina Boulevard, Singapore', 'Changi Airport Terminal 3, Singapore', 22.0, TRUE, 1, 3),

-- Route 5: Singapore warehouse -> customer delivery inside Singapore
('1 Marina Boulevard, Singapore', '456 Marina Bay, Singapore 018972', 14.0, TRUE, 1, NULL),

-- Route 6: Singapore warehouse -> customer delivery, train main leg simulation
('1 Marina Boulevard, Singapore', '225 Bukit Batok Central, Singapore 650225', 28.0, TRUE, 1, NULL);

-- ============================================================
-- Route Legs
-- Rule used:
-- - first mile = pickup / move to long-haul departure point
-- - main transport = neither first mile nor last mile
-- - last mile = final delivery to customer or destination hub
-- ============================================================

INSERT INTO route_leg
(route_id, sequence, transport_mode, start_point, end_point, distance_km, is_first_mile, is_last_mile, transport_id)
VALUES
-- ============================================================
-- Route 1 : SEA route (Truck -> Ship -> Truck)
-- main leg should be SHIP
-- ============================================================
(1, 1, 'TRUCK', '1 Marina Boulevard, Singapore', 'Port of Singapore, Pasir Ris, Singapore', 18.0, TRUE,  FALSE, 1),
(1, 2, 'SHIP',  'Port of Singapore, Pasir Ris, Singapore', 'Hong Kong Seaport Receiving Point', 2520.0, FALSE, FALSE, 3),
(1, 3, 'TRUCK', 'Hong Kong Seaport Receiving Point', '123 Industrial Road, Hong Kong', 42.0, FALSE, TRUE,  2),

-- ============================================================
-- Route 2 : AIR route (Truck -> Plane -> Truck)
-- main leg should be PLANE
-- ============================================================
(2, 1, 'TRUCK', '1 Marina Boulevard, Singapore', 'Changi Airport Terminal 3, Singapore', 22.0, TRUE,  FALSE, 1),
(2, 2, 'PLANE', 'Changi Airport Terminal 3, Singapore', 'Hong Kong Air Cargo Terminal', 2500.0, FALSE, FALSE, 4),
(2, 3, 'TRUCK', 'Hong Kong Air Cargo Terminal', '123 Industrial Road, Hong Kong', 38.0, FALSE, TRUE,  2),

-- ============================================================
-- Route 3 : Port feeder route only
-- no main leg unless your logic treats single-leg as main
-- ============================================================
(3, 1, 'TRUCK', '1 Marina Boulevard, Singapore', 'Port of Singapore, Pasir Ris, Singapore', 18.0, TRUE, TRUE, 1),

-- ============================================================
-- Route 4 : Airport feeder route only
-- no main leg unless your logic treats single-leg as main
-- ============================================================
(4, 1, 'TRUCK', '1 Marina Boulevard, Singapore', 'Changi Airport Terminal 3, Singapore', 22.0, TRUE, TRUE, 1),

-- ============================================================
-- Route 5 : Direct local delivery
-- truck-only route
-- ============================================================
(5, 1, 'TRUCK', '1 Marina Boulevard, Singapore', '456 Marina Bay, Singapore 018972', 14.0, TRUE, TRUE, 1),

-- ============================================================
-- Route 6 : TRAIN-based route simulation
-- main leg should be TRAIN
-- useful for testing train batching
-- ============================================================
(6, 1, 'TRUCK', '1 Marina Boulevard, Singapore', 'Jurong Rail Freight Node', 8.0, TRUE,  FALSE, 1),
(6, 2, 'TRAIN', 'Jurong Rail Freight Node', 'Bukit Batok Rail Transfer Point', 15.0, FALSE, FALSE, 5),
(6, 3, 'TRUCK', 'Bukit Batok Rail Transfer Point', '225 Bukit Batok Central, Singapore 650225', 5.0, FALSE, TRUE,  2);