IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [Households] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Households] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE TABLE [Members] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [IdentityUserId] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [PreferredCulture] nvarchar(max) NULL,
        [JoinedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Members] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Members_Households_HouseholdId] FOREIGN KEY ([HouseholdId]) REFERENCES [Households] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Members_HouseholdId] ON [Members] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161126_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723161126_InitialCreate', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE TABLE [Tags] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE TABLE [Tasks] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [DueDate] datetime2 NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [AssigneeId] int NULL,
        [RecurrenceRule] nvarchar(max) NULL,
        [ParentTaskId] int NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Tasks] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE TABLE [SubTasks] (
        [Id] int NOT NULL IDENTITY,
        [TaskItemId] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [IsDone] bit NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_SubTasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubTasks_Tasks_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [Tasks] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE TABLE [TaskTags] (
        [TaskItemId] int NOT NULL,
        [TagId] int NOT NULL,
        CONSTRAINT [PK_TaskTags] PRIMARY KEY ([TaskItemId], [TagId]),
        CONSTRAINT [FK_TaskTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TaskTags_Tasks_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [Tasks] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE INDEX [IX_SubTasks_TaskItemId] ON [SubTasks] ([TaskItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE INDEX [IX_Tags_HouseholdId] ON [Tags] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE INDEX [IX_Tasks_HouseholdId] ON [Tasks] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    CREATE INDEX [IX_TaskTags_TagId] ON [TaskTags] ([TagId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723161159_AddTasks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723161159_AddTasks', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723193457_AddReminders'
)
BEGIN
    CREATE TABLE [Reminders] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [TriggerAtUtc] datetime2 NOT NULL,
        [RecurrenceRule] nvarchar(max) NULL,
        [SourceType] int NOT NULL,
        [SourceId] int NULL,
        [IsResolved] bit NOT NULL,
        [SnoozedUntilUtc] datetime2 NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Reminders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723193457_AddReminders'
)
BEGIN
    CREATE TABLE [ReminderRecipients] (
        [ReminderId] int NOT NULL,
        [MemberId] int NOT NULL,
        [NotifiedViaEmail] bit NOT NULL,
        [NotifiedInAppAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ReminderRecipients] PRIMARY KEY ([ReminderId], [MemberId]),
        CONSTRAINT [FK_ReminderRecipients_Reminders_ReminderId] FOREIGN KEY ([ReminderId]) REFERENCES [Reminders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723193457_AddReminders'
)
BEGIN
    CREATE INDEX [IX_Reminders_HouseholdId] ON [Reminders] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723193457_AddReminders'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723193457_AddReminders', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723194855_AddCalendar'
)
BEGIN
    CREATE TABLE [Events] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [StartsAtUtc] datetime2 NOT NULL,
        [EndsAtUtc] datetime2 NOT NULL,
        [Location] nvarchar(200) NULL,
        [RecurrenceRule] nvarchar(max) NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723194855_AddCalendar'
)
BEGIN
    CREATE TABLE [EventAttendees] (
        [EventId] int NOT NULL,
        [MemberId] int NOT NULL,
        CONSTRAINT [PK_EventAttendees] PRIMARY KEY ([EventId], [MemberId]),
        CONSTRAINT [FK_EventAttendees_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723194855_AddCalendar'
)
BEGIN
    CREATE INDEX [IX_Events_HouseholdId] ON [Events] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723194855_AddCalendar'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723194855_AddCalendar', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724055007_AddModuleStates'
)
BEGIN
    CREATE TABLE [ModuleStates] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [ModuleKey] nvarchar(450) NOT NULL,
        [IsEnabled] bit NOT NULL,
        CONSTRAINT [PK_ModuleStates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724055007_AddModuleStates'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ModuleStates_HouseholdId_ModuleKey] ON [ModuleStates] ([HouseholdId], [ModuleKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724055007_AddModuleStates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724055007_AddModuleStates', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724082902_AddModulePermissions'
)
BEGIN
    CREATE TABLE [ModulePermissionStates] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [ModuleKey] nvarchar(450) NOT NULL,
        [Permission] nvarchar(450) NOT NULL,
        [IsGranted] bit NOT NULL,
        CONSTRAINT [PK_ModulePermissionStates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724082902_AddModulePermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ModulePermissionStates_HouseholdId_ModuleKey_Permission] ON [ModulePermissionStates] ([HouseholdId], [ModuleKey], [Permission]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724082902_AddModulePermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724082902_AddModulePermissions', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724204002_AddMemberInvites'
)
BEGIN
    ALTER TABLE [Members] ADD [Email] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724204002_AddMemberInvites'
)
BEGIN
    ALTER TABLE [Members] ADD [IsOwner] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724204002_AddMemberInvites'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724204002_AddMemberInvites', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724205315_AddMemberModuleAccess'
)
BEGIN
    CREATE TABLE [MemberModuleAccesses] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [MemberId] int NOT NULL,
        [ModuleKey] nvarchar(450) NOT NULL,
        [CanAccess] bit NOT NULL,
        CONSTRAINT [PK_MemberModuleAccesses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724205315_AddMemberModuleAccess'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MemberModuleAccesses_HouseholdId_MemberId_ModuleKey] ON [MemberModuleAccesses] ([HouseholdId], [MemberId], [ModuleKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724205315_AddMemberModuleAccess'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724205315_AddMemberModuleAccess', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE TABLE [Boards] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Boards] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE TABLE [Columns] (
        [Id] int NOT NULL IDENTITY,
        [BoardId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        [MappedStatus] int NOT NULL,
        CONSTRAINT [PK_Columns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Columns_Boards_BoardId] FOREIGN KEY ([BoardId]) REFERENCES [Boards] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE TABLE [Cards] (
        [Id] int NOT NULL IDENTITY,
        [ColumnId] int NOT NULL,
        [TaskItemId] int NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_Cards] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cards_Columns_ColumnId] FOREIGN KEY ([ColumnId]) REFERENCES [Columns] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Cards_Tasks_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [Tasks] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE INDEX [IX_Boards_HouseholdId] ON [Boards] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE INDEX [IX_Cards_ColumnId] ON [Cards] ([ColumnId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE INDEX [IX_Cards_TaskItemId] ON [Cards] ([TaskItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    CREATE INDEX [IX_Columns_BoardId] ON [Columns] ([BoardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724211927_AddKanban'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724211927_AddKanban', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213509_AddNotes'
)
BEGIN
    CREATE TABLE [Notes] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NULL,
        [Content] nvarchar(max) NOT NULL,
        [IsJournalEntry] bit NOT NULL,
        [JournalDate] date NULL,
        [LinkType] int NOT NULL,
        [LinkedEntityId] int NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Notes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213509_AddNotes'
)
BEGIN
    CREATE TABLE [NoteTags] (
        [NoteId] int NOT NULL,
        [TagId] int NOT NULL,
        CONSTRAINT [PK_NoteTags] PRIMARY KEY ([NoteId], [TagId]),
        CONSTRAINT [FK_NoteTags_Notes_NoteId] FOREIGN KEY ([NoteId]) REFERENCES [Notes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_NoteTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213509_AddNotes'
)
BEGIN
    CREATE INDEX [IX_Notes_HouseholdId] ON [Notes] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213509_AddNotes'
)
BEGIN
    CREATE INDEX [IX_NoteTags_TagId] ON [NoteTags] ([TagId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213509_AddNotes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724213509_AddNotes', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214538_AddShoppingLists'
)
BEGIN
    CREATE TABLE [ShoppingLists] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ShoppingLists] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214538_AddShoppingLists'
)
BEGIN
    CREATE TABLE [ShoppingListItems] (
        [Id] int NOT NULL IDENTITY,
        [ShoppingListId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Quantity] nvarchar(max) NULL,
        [IsChecked] bit NOT NULL,
        [AddedByMemberId] int NULL,
        CONSTRAINT [PK_ShoppingListItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShoppingListItems_ShoppingLists_ShoppingListId] FOREIGN KEY ([ShoppingListId]) REFERENCES [ShoppingLists] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214538_AddShoppingLists'
)
BEGIN
    CREATE INDEX [IX_ShoppingListItems_ShoppingListId] ON [ShoppingListItems] ([ShoppingListId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214538_AddShoppingLists'
)
BEGIN
    CREATE INDEX [IX_ShoppingLists_HouseholdId] ON [ShoppingLists] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214538_AddShoppingLists'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724214538_AddShoppingLists', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725102444_RemoveKanbanBoards'
)
BEGIN
    DROP TABLE [Cards];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725102444_RemoveKanbanBoards'
)
BEGIN
    DROP TABLE [Columns];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725102444_RemoveKanbanBoards'
)
BEGIN
    DROP TABLE [Boards];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725102444_RemoveKanbanBoards'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725102444_RemoveKanbanBoards', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725104324_AddNotificationPreferences'
)
BEGIN
    CREATE TABLE [MemberNotificationPreferences] (
        [Id] int NOT NULL IDENTITY,
        [MemberId] int NOT NULL,
        [Category] int NOT NULL,
        [IsEnabled] bit NOT NULL,
        CONSTRAINT [PK_MemberNotificationPreferences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MemberNotificationPreferences_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725104324_AddNotificationPreferences'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MemberNotificationPreferences_MemberId_Category] ON [MemberNotificationPreferences] ([MemberId], [Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725104324_AddNotificationPreferences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725104324_AddNotificationPreferences', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725105437_AddItemShares'
)
BEGIN
    CREATE TABLE [ItemShares] (
        [Id] int NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [ItemId] int NOT NULL,
        [MemberId] int NOT NULL,
        CONSTRAINT [PK_ItemShares] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725105437_AddItemShares'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ItemShares_Type_ItemId_MemberId] ON [ItemShares] ([Type], [ItemId], [MemberId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725105437_AddItemShares'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725105437_AddItemShares', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE TABLE [Bills] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [DueDate] date NOT NULL,
        [RecurrenceRule] nvarchar(max) NULL,
        [IsPaid] bit NOT NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Bills] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE TABLE [Budgets] (
        [Id] int NOT NULL IDENTITY,
        [HouseholdId] int NOT NULL,
        [CategoryId] int NOT NULL,
        [MonthlyLimit] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Budgets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Budgets_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE TABLE [Transactions] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Type] int NOT NULL,
        [CategoryId] int NULL,
        [OccurredOn] date NOT NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Transactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE TABLE [ExpenseShares] (
        [Id] int NOT NULL IDENTITY,
        [TransactionId] int NOT NULL,
        [MemberId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ExpenseShares] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExpenseShares_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExpenseShares_Transactions_TransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_Bills_HouseholdId] ON [Bills] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_Budgets_CategoryId] ON [Budgets] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Budgets_HouseholdId_CategoryId] ON [Budgets] ([HouseholdId], [CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_Categories_HouseholdId] ON [Categories] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_ExpenseShares_MemberId] ON [ExpenseShares] ([MemberId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_ExpenseShares_TransactionId] ON [ExpenseShares] ([TransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_Transactions_CategoryId] ON [Transactions] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    CREATE INDEX [IX_Transactions_HouseholdId] ON [Transactions] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725133815_AddFinance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725133815_AddFinance', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725135117_AddLifeAdmin'
)
BEGIN
    CREATE TABLE [Contacts] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Role] nvarchar(100) NULL,
        [Phone] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [Notes] nvarchar(1000) NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725135117_AddLifeAdmin'
)
BEGIN
    CREATE TABLE [Documents] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NULL,
        [ExpiryDate] date NULL,
        [Notes] nvarchar(1000) NULL,
        [FilePath] nvarchar(400) NULL,
        [HouseholdId] int NOT NULL,
        [OwnerId] int NOT NULL,
        [Visibility] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725135117_AddLifeAdmin'
)
BEGIN
    CREATE INDEX [IX_Contacts_HouseholdId] ON [Contacts] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725135117_AddLifeAdmin'
)
BEGIN
    CREATE INDEX [IX_Documents_HouseholdId] ON [Documents] ([HouseholdId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725135117_AddLifeAdmin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725135117_AddLifeAdmin', N'8.0.10');
END;
GO

COMMIT;
GO

