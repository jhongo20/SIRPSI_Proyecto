USE [SIRPSI]
GO

INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId]
           ,[Status]
           ,[RegistrationDate]
           ,[UserRegistration]
           ,[ModifiedDate]
           ,[UserModify]
           ,[Discriminator])
     VALUES
           ('9fb94ffc-135e-4161-b339-0da0405c0258'
           ,'8782edf2-7c4d-4df0-a94d-60e7346ef420'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'987654321'
           ,NULL
           ,NULL
           ,'AspNetUserRoles')
GO

INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId]
           ,[Status]
           ,[RegistrationDate]
           ,[UserRegistration]
           ,[ModifiedDate]
           ,[UserModify]
           ,[Discriminator])
     VALUES
           ('9fb94ffc-135e-4161-b339-0da0405c0258'
           ,'7d38403e-5474-47ae-8cf2-3b873e286090'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'987654321'
           ,NULL
           ,NULL
           ,'AspNetUserRoles')
GO

INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId]
           ,[Status]
           ,[RegistrationDate]
           ,[UserRegistration]
           ,[ModifiedDate]
           ,[UserModify]
           ,[Discriminator])
     VALUES
           ('45f445b2-b6f0-432c-9877-c0937ae90770'
           ,'a4d6e0c2-444a-4b94-8351-0faa8cc51c2b'
           ,'cab25738-41fe-4989-a115-0ac36325dd6c'
           ,GETDATE()
           ,'987654321'
           ,NULL
           ,NULL
           ,'AspNetUserRoles')
GO



SELECT * FROM AspNetUserRoles

select * from AspNetRoles
