-- VERIFICACIÓN DE RESERVAS CORRECTA
SELECT EstadoHerramienta FROM dbo.Herramienta WHERE IdHerramienta = 2;
SELECT EstadoReserva FROM dbo.Reserva WHERE IdReserva = 4;

SELECT * FROM dbo.Reserva;

-- VERIFICACIÓN DE ALQUILER CORRECTA
SELECT EstadoHerramienta FROM dbo.Herramienta WHERE IdHerramienta = 2;
SELECT MontoEstimado, EstadoAlquiler FROM dbo.Alquiler WHERE IdAlquiler = 4;
SELECT * FROM dbo.Alquiler;
SELECT EstadoReserva FROM dbo.Reserva WHERE IdReserva = 4;

SELECT * FROM dbo.Herramienta;