CREATE TABLE [dbo].[Users] (
    [Id]                 INT         IDENTITY (1, 1) NOT NULL,
    [Username]           NCHAR (16)  NOT NULL,
    [SaltedPasswordHash] BINARY (32) NOT NULL,
    [Salt]               BINARY (8)  NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_User_username] UNIQUE NONCLUSTERED ([Username] ASC)
);