-- Insert Users
INSERT INTO Users (Profile_Name, Username, Email, PasswordHash, ProfilePicture, Bio, Is_Admin, Is_Verified, EncryptedAPIKey)
SELECT 
    CONCAT('User', n),
    CONCAT('user', n),
    CONCAT('user', n, '@example.com'),
    HASHBYTES('SHA2_256', CONCAT('password', n)),
    NULL,
    CONCAT('This is bio of User', n),
    CASE WHEN n % 50 = 0 THEN 1 ELSE 0 END, -- 1 Admin for every 50 users
    CASE WHEN n % 20 = 0 THEN 1 ELSE 0 END, -- Verified for every 20 users
    NULL
FROM (SELECT TOP 150 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM master.dbo.spt_values) AS T;

-- Insert Followers
INSERT INTO Followers (FollowerUserID, FollowingUserID)
SELECT 
    F1.ID, 
    F2.ID
FROM Users F1
JOIN Users F2 ON F1.ID <> F2.ID AND F1.ID % 5 = F2.ID % 5 -- Random but connected relationships
WHERE F1.ID < 100;

-- Insert Stocks
INSERT INTO Stock (TickerSymbol, StockPrice, LastUpdated)
VALUES
('AAPL', 175.23, GETDATE()),
('GOOGL', 2834.50, GETDATE()),
('AMZN', 3450.25, GETDATE()),
('TSLA', 730.75, GETDATE()),
('MSFT', 310.22, GETDATE());

-- Insert Trades
INSERT INTO Trade (UserID, TickerSymbol, TradeType, Quantity, EntryPrice, CurrentPrice, LastUpdated)
SELECT 
    U.ID, 
    S.TickerSymbol, 
    CASE WHEN RAND() > 0.5 THEN 'BUY' ELSE 'SELL' END, 
    ROUND(10 + (RAND() * 90), 2), 
    S.StockPrice - (RAND() * 50), 
    S.StockPrice + (RAND() * 50), 
    GETDATE()
FROM Users U
CROSS JOIN Stock S
WHERE U.ID < 100;

-- Insert Posts
INSERT INTO Posts (UserID, Content, CreatedAt, PrivacySetting)
SELECT 
    U.ID, 
    CONCAT('This is a trading update by User ', U.ID, '. Market looks great!'), 
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE()), 
    CASE WHEN RAND() > 0.7 THEN 'Private' ELSE 'Public' END
FROM Users U
WHERE U.ID < 100;

-- Insert Comments
INSERT INTO Comments (PostID, UserID, Content, CreatedAt)
SELECT 
    P.ID, 
    U.ID, 
    CONCAT('Nice insights, User ', U.ID, '!'), 
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE())
FROM Posts P
JOIN Users U ON U.ID <> P.UserID
WHERE U.ID < 100;

-- Insert Likes
INSERT INTO Likes (UserID, PostID, CreatedAt)
SELECT 
    U.ID, 
    P.ID, 
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE())
FROM Users U
JOIN Posts P ON U.ID <> P.UserID
WHERE U.ID < 100;
