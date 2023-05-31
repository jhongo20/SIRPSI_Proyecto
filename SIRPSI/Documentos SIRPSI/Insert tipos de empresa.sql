USE [SIRPSI]
GO

INSERT INTO [sirpsi].[TiposEmpresa]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('d9bda3f2-bf2a-4269-9d4f-eed5f44f42f2'
           ,'Publica'
           ,''
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,NULL
           ,NULL)
GO

INSERT INTO [sirpsi].[TiposEmpresa]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('5693d239-bbc1-4a02-aba4-ddc7c24293e9'
           ,'Privada'
           ,''
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,NULL
           ,NULL)
GO

--select * from [sirpsi].[Estados]
