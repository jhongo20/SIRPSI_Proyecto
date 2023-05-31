USE [SIRPSI]
GO

INSERT INTO [sirpsi].[Pais]
           ([Id]
           ,[Nombre]
           ,[Descripcion]
		   ,[IdEstado]
           ,[FechaRegistro]
           ,[UsuarioRegistro]
           ,[FechaModifico]
           ,[UsuarioModifico])
     VALUES
           (
		   'f3125ac0-13ae-495f-a581-44e4203a11b9'
           ,'Colombia'
           ,''
		   ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'80894664'
           ,null
           ,null)
GO


