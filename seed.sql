-- Team 2-3 Seed Data

INSERT INTO Category (Name, Description) VALUES
('Camera', 'Digital and mirrorless cameras'),
('Lens', 'Camera lenses'),
('Tripod', 'Camera tripods and stands'),
('Gimbal', 'Camera stabilizers'),
('Lighting', 'Studio and portable lighting'),
('Microphone', 'Audio recording equipment'),
('Memory Card', 'SD and CFExpress storage cards'),
('Camera Bag', 'Carrying bags and backpacks');

INSERT INTO Product (CategoryId, Sku, Threshold) VALUES
(1, 'CAM-CANON-R5', 0.10),
(1, 'CAM-SONY-A7IV', 0.10),
(2, 'LEN-SONY-2470GM', 0.10),
(2, 'LEN-CANON-70200RF', 0.10),
(3, 'TRI-MANFROTTO-BEFREE', 0.10),
(4, 'GIM-DJI-RS3', 0.10),
(6, 'MIC-RODE-VIDEOMICPRO', 0.10);

INSERT INTO ProductDetails 
(ProductId, TotalQuantity, Name, Description, Weight, Image, Price, DepositRate)
VALUES
(1, 5, 'Canon EOS R5', '45MP full-frame mirrorless camera', 0.74, 'canon_r5.jpg', 150.00, 0.30),
(2, 4, 'Sony A7 IV', '33MP hybrid mirrorless camera', 0.66, 'sony_a7iv.jpg', 130.00, 0.30),
(3, 6, 'Sony FE 24-70mm f2.8 GM', 'Professional standard zoom lens', 0.88, 'sony_2470gm.jpg', 90.00, 0.25),
(4, 3, 'Canon RF 70-200mm f2.8 L', 'Telephoto zoom lens', 1.07, 'canon_70200.jpg', 110.00, 0.25),
(5, 8, 'Manfrotto Befree Advanced Tripod', 'Portable travel tripod', 1.50, 'manfrotto_befree.jpg', 25.00, 0.10),
(6, 4, 'DJI RS 3 Gimbal Stabilizer', '3-axis camera stabilizer', 1.30, 'dji_rs3.jpg', 60.00, 0.20),
(7, 10, 'Rode VideoMic Pro+', 'Shotgun microphone for cameras', 0.12, 'rode_videomic.jpg', 20.00, 0.10);

INSERT INTO InventoryItem (ProductId, SerialNumber) VALUES
(1, 'R5-0001'),
(1, 'R5-0002'),
(1, 'R5-0003'),
(1, 'R5-0004'),
(1, 'R5-0005'),
(2, 'A7IV-0001'),
(2, 'A7IV-0002'),
(2, 'A7IV-0003'),
(2, 'A7IV-0004'),
(3,'2470GM-0001'),
(3,'2470GM-0002'),
(3,'2470GM-0003'),
(3,'2470GM-0004'),
(3,'2470GM-0005'),
(3,'2470GM-0006'),
(4,'70200RF-0001'),
(4,'70200RF-0002'),
(4,'70200RF-0003'),
(5,'TRI-0001'),
(5,'TRI-0002'),
(5,'TRI-0003'),
(5,'TRI-0004'),
(5,'TRI-0005'),
(5,'TRI-0006'),
(5,'TRI-0007'),
(5,'TRI-0008'),
(6,'RS3-0001'),
(6,'RS3-0002'),
(6,'RS3-0003'),
(6,'RS3-0004'),
(7,'RVP-0001'),
(7,'RVP-0002'),
(7,'RVP-0003'),
(7,'RVP-0004'),
(7,'RVP-0005'),
(7,'RVP-0006'),
(7,'RVP-0007'),
(7,'RVP-0008'),
(7,'RVP-0009'),
(7,'RVP-0010');