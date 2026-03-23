-- =============================================
-- Script de Base de Datos para LoginNet
-- Base de datos: SQL Server
-- Zona horaria: Lima, Perú (SA Pacific Standard Time)
-- =============================================

USE master;
GO

-- Eliminar base de datos si existe (para recrear)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'LoginNetDB')
BEGIN
    ALTER DATABASE LoginNetDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LoginNetDB;
END
GO

-- Crear la base de datos
CREATE DATABASE LoginNetDB;
GO

USE LoginNetDB;
GO

-- =============================================
-- Tabla de Usuarios
-- =============================================
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroDocumento NVARCHAR(20) NOT NULL,
    TipoDocumento NVARCHAR(3) NOT NULL DEFAULT 'DNI',
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Correo NVARCHAR(100) NOT NULL,
    ClaveHash NVARCHAR(256) NOT NULL,
    CVF INT NOT NULL DEFAULT 0,
    FechaBloqueo DATETIME NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_NumeroDocumento UNIQUE (NumeroDocumento),
    CONSTRAINT UQ_Correo UNIQUE (Correo)
);
GO

-- =============================================
-- Seed Data: Usuarios de prueba
-- Contraseña para todos: 123456
-- Hash SHA256 en Base64 de "123456":
-- odqe9m6t0ymjOmKQg+aGzwzX1ahvN8oSIMySONbMkgg=
-- =============================================

-- Primero actualizamos las passwords usando el hash calculado
DECLARE @hash NVARCHAR(256) = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=';

INSERT INTO Usuarios (NumeroDocumento, TipoDocumento, Nombre, Apellido, Correo, ClaveHash, CVF, FechaCreacion, Activo)
VALUES 
    ('46844596', 'DNI', 'Juan', 'Perez Garcia', 'juan.perez@ceplan.gob.pe', @hash, 0, GETDATE(), 1),
    ('45678912', 'DNI', 'Maria', 'Gonzales Lopez', 'maria.gonzales@ceplan.gob.pe', @hash, 0, GETDATE(), 1),
    ('CE001234', 'CE', 'Carlos', 'Rodriguez Smith', 'carlos.rodriguez@ceplan.gob.pe', @hash, 0, GETDATE(), 1);
GO

-- =============================================
-- Verificar datos
-- =============================================
SELECT Id, NumeroDocumento, TipoDocumento, Nombre, Correo, ClaveHash FROM Usuarios;
GO
