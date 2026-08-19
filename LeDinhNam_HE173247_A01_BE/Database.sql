CREATE TABLE [Category] (
    [CategoryID] smallint NOT NULL IDENTITY,
    [CategoryName] nvarchar(100) NOT NULL,
    [CategoryDesciption] nvarchar(250) NOT NULL,
    [ParentCategoryID] smallint NULL,
    [IsActive] bit NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY ([CategoryID]),
    CONSTRAINT [FK_Category_Category] FOREIGN KEY ([ParentCategoryID]) REFERENCES [Category] ([CategoryID])
);
GO


CREATE TABLE [SystemAccount] (
    [AccountID] smallint NOT NULL,
    [AccountName] nvarchar(100) NULL,
    [AccountEmail] nvarchar(70) NULL,
    [AccountRole] int NULL,
    [AccountPassword] nvarchar(70) NULL,
    CONSTRAINT [PK_SystemAccount] PRIMARY KEY ([AccountID])
);
GO


CREATE TABLE [Tag] (
    [TagID] int NOT NULL,
    [TagName] nvarchar(50) NULL,
    [Note] nvarchar(400) NULL,
    CONSTRAINT [PK_HashTag] PRIMARY KEY ([TagID])
);
GO


CREATE TABLE [NewsArticle] (
    [NewsArticleID] nvarchar(20) NOT NULL,
    [NewsTitle] nvarchar(400) NULL,
    [Headline] nvarchar(150) NOT NULL,
    [CreatedDate] datetime NULL,
    [NewsContent] nvarchar(4000) NULL,
    [NewsSource] nvarchar(400) NULL,
    [CategoryID] smallint NULL,
    [NewsStatus] bit NULL,
    [CreatedByID] smallint NULL,
    [UpdatedByID] smallint NULL,
    [ModifiedDate] datetime NULL,
    CONSTRAINT [PK_NewsArticle] PRIMARY KEY ([NewsArticleID]),
    CONSTRAINT [FK_NewsArticle_Category] FOREIGN KEY ([CategoryID]) REFERENCES [Category] ([CategoryID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_NewsArticle_SystemAccount] FOREIGN KEY ([CreatedByID]) REFERENCES [SystemAccount] ([AccountID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_NewsArticle_SystemAccount_UpdatedBy] FOREIGN KEY ([UpdatedByID]) REFERENCES [SystemAccount] ([AccountID])
);
GO


CREATE TABLE [NewsTag] (
    [NewsArticleID] nvarchar(20) NOT NULL,
    [TagID] int NOT NULL,
    CONSTRAINT [PK_NewsTag] PRIMARY KEY ([NewsArticleID], [TagID]),
    CONSTRAINT [FK_NewsTag_NewsArticle] FOREIGN KEY ([NewsArticleID]) REFERENCES [NewsArticle] ([NewsArticleID]),
    CONSTRAINT [FK_NewsTag_Tag] FOREIGN KEY ([TagID]) REFERENCES [Tag] ([TagID])
);
GO


CREATE INDEX [IX_Category_ParentCategoryID] ON [Category] ([ParentCategoryID]);
GO


CREATE INDEX [IX_NewsArticle_CategoryID] ON [NewsArticle] ([CategoryID]);
GO


CREATE INDEX [IX_NewsArticle_CreatedByID] ON [NewsArticle] ([CreatedByID]);
GO


CREATE INDEX [IX_NewsArticle_UpdatedByID] ON [NewsArticle] ([UpdatedByID]);
GO


CREATE INDEX [IX_NewsTag_TagID] ON [NewsTag] ([TagID]);
GO



-- Seed Data for SystemAccount (Role: 1=Staff, 2=Lecturer, 3=Admin... actually Admin is in config, so 1 or 2 here)
INSERT INTO [SystemAccount] (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES 
(1, 'Staff One', 'staff1@funews.com', 1, '1'),
(2, 'Staff Two', 'staff2@funews.com', 1, '1'),
(3, 'Lecturer One', 'lecturer1@funews.com', 2, '1'),
(4, 'Lecturer Two', 'lecturer2@funews.com', 2, '1'),
(5, 'Staff Three', 'staff3@funews.com', 1, '1');
GO

-- Seed Data for Category
SET IDENTITY_INSERT [Category] ON;
INSERT INTO [Category] (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES 
(1, 'Technology', 'Tech news', NULL, 1),
(2, 'Sports', 'Sports news', NULL, 1),
(3, 'Politics', 'Political news', NULL, 1),
(4, 'Entertainment', 'Entertainment and movies', NULL, 1),
(5, 'AI', 'Artificial Intelligence', 1, 1);
SET IDENTITY_INSERT [Category] OFF;
GO

-- Seed Data for Tag
INSERT INTO [Tag] (TagID, TagName, Note) VALUES 
(1, 'Breaking', 'Breaking news'),
(2, 'Trending', 'Hot topics'),
(3, 'Local', 'Local events'),
(4, 'Global', 'International news'),
(5, 'Exclusive', 'Exclusive content');
GO

-- Seed Data for NewsArticle
INSERT INTO [NewsArticle] (NewsArticleID, NewsTitle, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID, UpdatedByID, ModifiedDate) VALUES 
('N01', 'First Tech Article', 'Tech is booming', '2026-08-01 10:00:00', 'Full content here', 'Source A', 1, 1, 1, NULL, NULL),
('N02', 'Sports Finals', 'Team A wins', '2026-08-02 12:00:00', 'Full content here', 'Source B', 2, 1, 2, 1, '2026-08-03 09:00:00'),
('N03', 'Election Results', 'New president elected', '2026-08-03 08:00:00', 'Full content here', 'Source C', 3, 1, 1, NULL, NULL),
('N04', 'Movie Release', 'Blockbuster out today', '2026-08-04 14:00:00', 'Full content here', 'Source D', 4, 1, 2, NULL, NULL),
('N05', 'AI Breakthrough', 'New AI model', '2026-08-05 16:00:00', 'Full content here', 'Source E', 5, 0, 1, 2, '2026-08-06 10:00:00');
GO

-- Seed Data for NewsTag
INSERT INTO [NewsTag] (NewsArticleID, TagID) VALUES 
('N01', 1),
('N01', 2),
('N02', 3),
('N03', 4),
('N04', 5),
('N05', 1),
('N05', 5);
GO

