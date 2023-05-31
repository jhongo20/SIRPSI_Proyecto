USE [SIRPSI]
GO

INSERT INTO [sirpsi].[Empresas]
           ([Id]
           ,[TipoDocumento]
           ,[DigitoVerificacion]
           ,[IdTipoEmpresa]
           ,[Documento]
           ,[Nombre]
           ,[Descripcion]
           ,[Observacion]
           ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           ('eed6b73b-c7fe-451d-ae31-bd9cd6581034'
           ,'373a1d14-775b-48f7-b033-6f697401ad6c'
           ,'1'
           ,'d9bda3f2-bf2a-4269-9d4f-eed5f44f42f2'
           ,'103365981'
           ,'Prueba'
           ,'Pruebas Desarrollo'
           ,''
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,NULL
           ,NULL)
GO



SELECT * FROM [sirpsi].[TiposDocumento]
SELECT * FROM [sirpsi].[TiposEmpresa]
SELECT * FROM [sirpsi].[Estados]
