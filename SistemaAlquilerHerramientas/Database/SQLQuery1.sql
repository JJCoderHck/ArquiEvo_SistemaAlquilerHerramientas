-- 1. Tabla Rol
CREATE TABLE Rol (
    idRol INT PRIMARY KEY IDENTITY(1,1),
    nombreRol VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255)
);

-- 2. Tabla Usuario (Relación con Rol)
CREATE TABLE Usuario (
    idUsuario INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(150) NOT NULL,
    correo VARCHAR(150) NOT NULL,
    usuario VARCHAR(50) NOT NULL,
    contrasena VARCHAR(255) NOT NULL,
    estado VARCHAR(50),
    idRol INT NOT NULL,
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (idRol) REFERENCES Rol(idRol)
);

-- 3. Tabla Proveedor
CREATE TABLE Proveedor (
    idProveedor INT PRIMARY KEY IDENTITY(1,1),
    razonSocial VARCHAR(200) NOT NULL,
    ruc VARCHAR(11) NOT NULL,
    telefono VARCHAR(20),
    correo VARCHAR(150),
    direccion VARCHAR(255),
    estado VARCHAR(50)
);

-- 4. Tabla CategoriaHerramienta
CREATE TABLE CategoriaHerramienta (
    idCategoria INT PRIMARY KEY IDENTITY(1,1),
    nombreCategoria VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255),
    estado VARCHAR(50)
);

-- 5. Tabla Cliente
CREATE TABLE Cliente (
    idCliente INT PRIMARY KEY IDENTITY(1,1),
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    dni VARCHAR(8) NOT NULL,
    telefono VARCHAR(20),
    correo VARCHAR(150),
    direccion VARCHAR(255),
    estado VARCHAR(50)
);

-- 6. Tabla Herramienta (Relación con Categoria y Proveedor)
CREATE TABLE Herramienta (
    idHerramienta INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(150) NOT NULL,
    descripcion VARCHAR(MAX),
    precioPorDia DECIMAL(18, 2) NOT NULL,
    estadoHerramienta VARCHAR(50),
    idCategoria INT NOT NULL,
    idProveedor INT NOT NULL,
    CONSTRAINT FK_Herramienta_Categoria FOREIGN KEY (idCategoria) REFERENCES CategoriaHerramienta(idCategoria),
    CONSTRAINT FK_Herramienta_Proveedor FOREIGN KEY (idProveedor) REFERENCES Proveedor(idProveedor)
);

-- 7. Tabla Reserva (Relación con Cliente y Herramienta)
CREATE TABLE Reserva (
    idReserva INT PRIMARY KEY IDENTITY(1,1),
    idCliente INT NOT NULL,
    idHerramienta INT NOT NULL,
    fechaInicio DATETIME NOT NULL,
    fechaDevolucionEstimada DATETIME NOT NULL,
    estadoReserva VARCHAR(50),
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Reserva_Cliente FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente),
    CONSTRAINT FK_Reserva_Herramienta FOREIGN KEY (idHerramienta) REFERENCES Herramienta(idHerramienta)
);

-- 8. Tabla Alquiler (Relación con Cliente, Herramienta y Reserva opcional)
CREATE TABLE Alquiler (
    idAlquiler INT PRIMARY KEY IDENTITY(1,1),
    idCliente INT NOT NULL,
    idHerramienta INT NOT NULL,
    idReserva INT NULL, -- Puede ser nulo si es alquiler directo
    fechaEntrega DATETIME NOT NULL,
    fechaDevolucionPactada DATETIME NOT NULL,
    montoEstimado DECIMAL(18, 2),
    estadoAlquiler VARCHAR(50),
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Alquiler_Cliente FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente),
    CONSTRAINT FK_Alquiler_Herramienta FOREIGN KEY (idHerramienta) REFERENCES Herramienta(idHerramienta),
    CONSTRAINT FK_Alquiler_Reserva FOREIGN KEY (idReserva) REFERENCES Reserva(idReserva)
);

-- 9. Tabla Devolución (Relación con Alquiler)
CREATE TABLE Devolucion (
    idDevolucion INT PRIMARY KEY IDENTITY(1,1),
    idAlquiler INT NOT NULL,
    fechaDevolucionReal DATETIME NOT NULL,
    estadoRetorno VARCHAR(50),
    observacion VARCHAR(MAX),
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Devolucion_Alquiler FOREIGN KEY (idAlquiler) REFERENCES Alquiler(idAlquiler)
);

-- 10. Tabla Mora (Relación con Alquiler)
CREATE TABLE Mora (
    idMora INT PRIMARY KEY IDENTITY(1,1),
    idAlquiler INT NOT NULL,
    diasRetraso INT NOT NULL,
    montoMora DECIMAL(18, 2) NOT NULL,
    estadoPago VARCHAR(50),
    CONSTRAINT FK_Mora_Alquiler FOREIGN KEY (idAlquiler) REFERENCES Alquiler(idAlquiler)
);