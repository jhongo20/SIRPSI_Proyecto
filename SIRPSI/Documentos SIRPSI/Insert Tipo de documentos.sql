USE [SIRPSI]
GO

INSERT INTO [sirpsi].[TiposDocumento]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('011b3925-e5ef-4706-abb3-aec6b34c8ef9'
           ,'Cédula de extranjeria'
           ,'Cédula'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,null
           ,null)
GO
INSERT INTO [sirpsi].[TiposDocumento]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('373a1d14-775b-48f7-b033-6f697401ad6c'
           ,'Cédula de cuidadania'
           ,'Cédula'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,null
           ,null)
GO

INSERT INTO [sirpsi].[TiposDocumento]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('cd7ef153-599e-4b9e-8de1-aaa379ff50a3'
           ,'Tarjeta de identidad'
           ,'Tarjeta'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,null
           ,null)
GO

INSERT INTO [sirpsi].[TiposDocumento]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('296748ba-5899-4825-923a-53db8543a300'
           ,'Nit'
           ,'Nit'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,null
           ,null)
GO

select * from [sirpsi].[Estados]
