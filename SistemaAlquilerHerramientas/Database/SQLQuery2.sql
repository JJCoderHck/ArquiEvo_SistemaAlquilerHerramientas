INSERT INTO dbo.Rol (NombreRol, Descripcion) VALUES ('Administrador', 'Acceso total al sistema');
INSERT INTO dbo.Rol (NombreRol, Descripcion) VALUES ('Cliente', 'Acceso limitado al sistema');
SELECT * FROM dbo.Rol;

-- Usuario administrador
INSERT INTO dbo.Usuario (nombre, correo, usuario, contrasena, estado, idRol)
VALUES ('Admin', 'admin@herratools.com', 'admin', '1234', 'Activo', 1);

-- Usuario cliente
INSERT INTO dbo.Usuario (nombre, correo, usuario, contrasena, estado, idRol)
VALUES ('Cliente', 'cliente@herratools.com', 'cliente', '1234', 'Activo', 2);
SELECT * FROM dbo.Usuario;