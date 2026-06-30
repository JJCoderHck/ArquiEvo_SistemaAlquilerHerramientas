-- Datos para Proveedor (Contrato 04)
INSERT INTO Proveedor (razonSocial, ruc, telefono, correo, direccion, estado)
VALUES 
('Herramientas Perú SAC', '20547896521', '987123456', 'ventas@herramientasperu.com', 'Av. Industrial 456', 'Activo'),
('Maquinarias Lima E.I.R.L.', '20123456789', '955443322', 'contacto@maqlim.pe', 'Calle Los Talleres 102', 'Activo');

-- Datos para CategoriaHerramienta (Contrato 05)
INSERT INTO CategoriaHerramienta (nombreCategoria, descripcion, estado)
VALUES 
('Herramientas eléctricas', 'Herramientas que funcionan con energía eléctrica', 'Activo'),
('Equipos de Compactación', 'Maquinaria pesada para preparación de suelos', 'Activo'),
('Herramientas de Corte', 'Discos, sierras y equipos de corte de precisión', 'Activo');
-- Datos para Cliente (Contrato 03)
INSERT INTO Cliente (nombres, apellidos, dni, telefono, correo, direccion, estado)
VALUES 
('Juan Carlos', 'Ramírez López', '75648912', '987654321', 'juan@email.com', 'Av. Principal 123', 'Activo'),
('María Fernanda', 'Casas Torres', '44556677', '912345678', 'mfer@email.com', 'Jr. Las Flores 450', 'Activo');

-- Datos para Herramienta (Contrato 06)
-- Asumiendo idCategoria=1 (Eléctricas) e idProveedor=1 (Herramientas Perú)
INSERT INTO Herramienta (nombre, descripcion, precioPorDia, estadoHerramienta, idCategoria, idProveedor)
VALUES 
('Taladro Bosch', 'Taladro percutor de 650W', 35.00, 'Disponible', 1, 1),
('Rotomartillo Makita', 'Rotomartillo SDS Plus 800W', 50.00, 'Disponible', 1, 1),
('Amoladora DeWalt', 'Amoladora angular de 4.5 pulgadas', 25.00, 'Disponible', 3, 2);

-- Datos para Reserva (Contrato 08)
-- Cliente 1 reserva Herramienta 1
INSERT INTO Reserva (idCliente, idHerramienta, fechaInicio, fechaDevolucionEstimada, estadoReserva, fechaRegistro)
VALUES 
(1, 1, '2026-06-20', '2026-06-23', 'Confirmada', GETDATE());

-- Datos para Alquiler (Contrato 09)
-- Alquiler derivado de la Reserva 1
INSERT INTO Alquiler (idCliente, idHerramienta, idReserva, fechaEntrega, fechaDevolucionPactada, montoEstimado, estadoAlquiler, fechaRegistro)
VALUES 
(1, 1, 1, '2026-06-20', '2026-06-23', 105.00, 'Activo', GETDATE());

-- Alquiler directo sin reserva (Contrato 09)
INSERT INTO Alquiler (idCliente, idHerramienta, idReserva, fechaEntrega, fechaDevolucionPactada, montoEstimado, estadoAlquiler, fechaRegistro)
VALUES 
(2, 3, NULL, '2026-06-21', '2026-06-22', 25.00, 'Activo', GETDATE());

-- Datos para Devolución (Contrato 10)
-- Devolución tardía del primer alquiler (pactado para el 23, devuelto el 25)
INSERT INTO Devolucion (idAlquiler, fechaDevolucionReal, estadoRetorno, observacion, fechaRegistro)
VALUES 
(1, '2026-06-25', 'Buen estado', 'Herramienta devuelta sin daños', GETDATE());

-- Datos para Mora (Contrato 11)
-- Basado en el retraso del alquiler 1 (2 días de mora)
INSERT INTO Mora (idAlquiler, diasRetraso, montoMora, estadoPago)
VALUES 
(1, 2, 20.00, 'Pendiente');

SELECT EstadoReserva FROM dbo.Reserva WHERE IdReserva = 2;