USE [SIRPSI]
GO

INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[Name]
           ,[NormalizedName]
           ,[ConcurrencyStamp]
           ,[Status]
           ,[Description]
           ,[RegistrationDate]
           ,[UserRegistration]
           ,[ModifiedDate]
           ,[UserModify]
           ,[Discriminator])
     VALUES
           ('8782edf2-7c4d-4df0-a94d-60e7346ef420'
           ,'Super Administrador'
           ,'JOHN.CULMA@OUTLOOK.COM'
           ,'7bb5a295-472b-4b71-9c8f-1cc94c70484f'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,'Este es el super usuario'
           ,GETDATE()
           ,'987654321'
           ,NULL
           ,NULL
           ,'AspNetRoles')
GO



INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[Name]
           ,[NormalizedName]
           ,[ConcurrencyStamp]
           ,[Status]
           ,[Description]
           ,[RegistrationDate]
           ,[UserRegistration]
           ,[ModifiedDate]
           ,[UserModify]
           ,[Discriminator])
     VALUES
           ('7d38403e-5474-47ae-8cf2-3b873e286090'
           ,'Cajero'
           ,'john.fcl14@gmail.com'
           ,'0594ae9b-2358-4ea1-87cb-071986f0a60e'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,'Este es un un cajero'
           ,GETDATE()
           ,'987654321'
           ,NULL
           ,NULL
           ,'AspNetRoles')
GO

INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[Name]
           ,[NormalizedName]
           ,[ConcurrencyStamp]
           ,[Status]
           ,[Description]
           ,[RegistrationDate]
           ,[UserRegistration]
           ,[ModifiedDate]
           ,[UserModify]
           ,[Discriminator])
     VALUES
           ('a4d6e0c2-444a-4b94-8351-0faa8cc51c2b'
           ,'Administrador empresas'
           ,'adminEmpresas@gmail.com'
           ,'0594ae9b-2358-4ea1-87cb-071986f0a60e'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,'Este es un un administrador empresas'
           ,GETDATE()
           ,'56986598'
           ,NULL
           ,NULL
           ,'AspNetRoles')
GO

select * from [dbo].[AspNetRoles]

UPDATE [dbo].[AspNetRoles] SET NormalizedName = 'jhon16_39@hotmail.com' WHERE ID = 'a4d6e0c2-444a-4b94-8351-0faa8cc51c2b'

select * from [sirpsi].[TiposDocumento]

select * from [sirpsi].[Pais]

select * from [sirpsi].[Empresas]