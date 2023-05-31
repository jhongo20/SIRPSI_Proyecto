CREATE SCHEMA sirpsi


CREATE TABLE sirpsi.Estados
(
Id NVARCHAR(450) PRIMARY KEY NOT NULL
,IdConsecutivo INT IDENTITY(1,1)
,Nombre NVARCHAR(500) NOT NULL
,Descripcion NVARCHAR(MAX) NULL
,FechaRegistro DATETIME NOT NULL
,UsuarioRegistro NVARCHAR(450)  NOT NULL
,FechaModifico DATETIME NULL
,UsuarioModifico NVARCHAR(450) NULL
)


CREATE TABLE sirpsi.TiposEmpresa
(
Id NVARCHAR(450) PRIMARY KEY NOT NULL
,IdConsecutivo INT IDENTITY(1,1)
,Nombre NVARCHAR(500) NOT NULL
,Descripcion NVARCHAR(MAX) NULL
,IdEstado NVARCHAR(450)
FOREIGN KEY (IdEstado) REFERENCES [sirpsi].[Estados](Id)
,FechaRegistro DATETIME NOT NULL
,UsuarioRegistro NVARCHAR(450)  NOT NULL
,FechaModifico DATETIME NULL
,UsuarioModifico NVARCHAR(450) NULL
)

CREATE TABLE sirpsi.Empresas
(
Id NVARCHAR(450) PRIMARY KEY NOT NULL
,IdConsecutivo INT IDENTITY(1,1)
,TipoDocumento NVARCHAR(450) NULL
,DigitoVerificacion NVARCHAR(10) NULL
,IdTipoEmpresa NVARCHAR(450)
FOREIGN KEY (IdTipoEmpresa) REFERENCES [sirpsi].[TiposEmpresa](Id)
,Documento NVARCHAR(100) NOT NULL
,Nombre NVARCHAR(300) NOT NULL
,Descripcion NVARCHAR(500) NOT NULL
,Observacion NVARCHAR(MAX) NULL
,IdEstado NVARCHAR(450)
FOREIGN KEY (IdEstado) REFERENCES [sirpsi].[Estados](Id)
,FechaRegistro DATETIME NOT NULL
,UsuarioRegistro NVARCHAR(450)  NOT NULL
,FechaModifico DATETIME NULL
,UsuarioModifico NVARCHAR(450) NULL
)

--DROP TABLE sirpsi.Empresas




--DROP TABLE sirpsi.TiposEmpresa



CREATE TABLE sirpsi.Pais
(
Id NVARCHAR(450) PRIMARY KEY NOT NULL
,IdConsecutivo INT IDENTITY(1,1)
,Nombre NVARCHAR(500) NOT NULL
,Descripcion NVARCHAR(MAX) NULL
,IdEstado NVARCHAR(450)
FOREIGN KEY (IdEstado) REFERENCES [sirpsi].[Estados](Id)
,FechaRegistro DATETIME NOT NULL
,UsuarioRegistro NVARCHAR(450)  NOT NULL
,FechaModifico DATETIME NULL
,UsuarioModifico NVARCHAR(450) NULL
)

CREATE TABLE sirpsi.TiposDocumento
(
Id NVARCHAR(450) PRIMARY KEY NOT NULL
,IdConsecutivo INT IDENTITY(1,1)
,Nombre NVARCHAR(500) NOT NULL
,Descripcion NVARCHAR(MAX) NULL
,IdEstado NVARCHAR(450)
FOREIGN KEY (IdEstado) REFERENCES [sirpsi].[Estados](Id)
,FechaRegistro DATETIME NOT NULL
,UsuarioRegistro NVARCHAR(450)  NOT NULL
,FechaModifico DATETIME NULL
,UsuarioModifico NVARCHAR(450) NULL
)

CREATE TABLE sirpsi.AspNetUserRoles
(
	 Id NVARCHAR(450) PRIMARY KEY NOT NULL
	,UserId NVARCHAR(450) NOT NULL
	FOREIGN KEY (UserId) REFERENCES [dbo].[AspNetUsers](Id)
	,RoleId NVARCHAR(450) NOT NULL
	FOREIGN KEY (RoleId) REFERENCES [dbo].[AspNetRoles](Id)
	,IdEstado NVARCHAR(450) NULL
	FOREIGN KEY (IdEstado) REFERENCES [sirpsi].[Estados](Id)
	,FechaRegistro DATETIME NOT NULL
	,UsuarioRegistro NVARCHAR(450)  NOT NULL
	,FechaModifico DATETIME NULL
	,UsuarioModifico NVARCHAR(450) NULL
	,Discriminator NVARCHAR(MAX) NOT NULL
)

--drop table sirpsi.Pais

select * from [sirpsi].[Pais]
select * from [sirpsi].[Estados]
select * from [sirpsi].[TiposEmpresa]
select * from [sirpsi].[TiposDocumento]
select * from  [sirpsi].[Empresas]
select * from  [dbo].[AspNetUsers]
