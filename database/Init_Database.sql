IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Clientes] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Cedula] nvarchar(max) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [Direccion] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
);

CREATE TABLE [Prestamos] (
    [Id] int NOT NULL IDENTITY,
    [ClienteId] int NOT NULL,
    [Monto] decimal(18,2) NOT NULL,
    [Interes] decimal(18,2) NOT NULL,
    [CantidadCuotas] int NOT NULL,
    [BalancePendiente] decimal(18,2) NOT NULL,
    [FechaPrestamo] datetime2 NOT NULL,
    CONSTRAINT [PK_Prestamos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Prestamos_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Sams] (
    [Id] int NOT NULL IDENTITY,
    [ClienteId] int NOT NULL,
    [Balance] decimal(18,2) NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    CONSTRAINT [PK_Sams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sams_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CuotasPrestamo] (
    [Id] int NOT NULL IDENTITY,
    [PrestamoId] int NOT NULL,
    [MontoCuota] decimal(18,2) NOT NULL,
    [FechaPago] datetime2 NOT NULL,
    [Pagada] bit NOT NULL,
    [FechaRealPago] datetime2 NULL,
    CONSTRAINT [PK_CuotasPrestamo] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CuotasPrestamo_Prestamos_PrestamoId] FOREIGN KEY ([PrestamoId]) REFERENCES [Prestamos] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_CuotasPrestamo_PrestamoId] ON [CuotasPrestamo] ([PrestamoId]);

CREATE INDEX [IX_Prestamos_ClienteId] ON [Prestamos] ([ClienteId]);

CREATE INDEX [IX_Sams_ClienteId] ON [Sams] ([ClienteId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260525182404_InitialCreate', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [NombreUsuario] nvarchar(max) NOT NULL,
    [Contrasena] nvarchar(max) NOT NULL,
    [Rol] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Contrasena', N'NombreUsuario', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
    SET IDENTITY_INSERT [Usuarios] ON;
INSERT INTO [Usuarios] ([Id], [Contrasena], [NombreUsuario], [Rol])
VALUES (1, N'admin123', N'admin', N'Administrador');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Contrasena', N'NombreUsuario', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
    SET IDENTITY_INSERT [Usuarios] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260617142629_AddUsuarioTable', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
UPDATE [Usuarios] SET [Contrasena] = N'admin1234'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260617143122_UpdateAdminPassword', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Sams] ADD [FechaVencimientoSiguiente] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Sams] ADD [TotalPenalidadAcumulada] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Sams] ADD [ValorPenalidadAtraso] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Prestamos] ADD [AplicaMoraFija] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Prestamos] ADD [FechaVencimientoSiguiente] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Prestamos] ADD [TotalMoraAcumulada] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Prestamos] ADD [ValorMora] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260622152235_AgregarMoraYPenalidad', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
EXEC sp_rename N'[Sams].[ValorPenalidadAtraso]', N'TotalPagar', 'COLUMN';

EXEC sp_rename N'[Sams].[TotalPenalidadAcumulada]', N'SaldoRestante', 'COLUMN';

EXEC sp_rename N'[Sams].[Balance]', N'MontoOriginal', 'COLUMN';

ALTER TABLE [Sams] ADD [CantidadSemanas] int NOT NULL DEFAULT 0;

ALTER TABLE [Sams] ADD [CuotaSemanal] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Sams] ADD [Estado] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Sams] ADD [InteresPorcentaje] decimal(18,2) NOT NULL DEFAULT 0.0;

CREATE TABLE [PagosSam] (
    [Id] int NOT NULL IDENTITY,
    [SamId] int NOT NULL,
    [MontoPagado] decimal(18,2) NOT NULL,
    [FechaPago] datetime2 NOT NULL,
    CONSTRAINT [PK_PagosSam] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PagosSam_Sams_SamId] FOREIGN KEY ([SamId]) REFERENCES [Sams] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_PagosSam_SamId] ON [PagosSam] ([SamId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260622160325_AddSamLoanLogic', N'10.0.0');

COMMIT;
GO

