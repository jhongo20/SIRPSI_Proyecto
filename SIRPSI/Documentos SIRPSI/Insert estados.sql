USE [SIRPSI]
GO

INSERT INTO [sirpsi].[Estados]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('cab25738-41fe-4989-a115-0ac36325dd6c'
           ,'ACTIVO'
           ,'Usuarios activos en el sistema'
           ,GETDATE()
           ,'80894664'
           ,NULL
           ,NULL)
GO

INSERT INTO [sirpsi].[Estados]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('c22caee5-aba0-4bd8-abf3-cff6305df919'
           ,'INACTIVO'
           ,'Usuarios inactivos en el sistema'
           ,GETDATE()
           ,'80894664'
           ,NULL
           ,NULL)
GO


