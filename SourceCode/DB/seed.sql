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
FROM (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n 
    FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
                 (11),(12),(13),(14),(15),(16),(17),(18),(19),(20),
                 (21),(22),(23),(24),(25),(26),(27),(28),(29),(30),
                 (31),(32),(33),(34),(35),(36),(37),(38),(39),(40),
                 (41),(42),(43),(44),(45),(46),(47),(48),(49),(50),
                 (51),(52),(53),(54),(55),(56),(57),(58),(59),(60),
                 (61),(62),(63),(64),(65),(66),(67),(68),(69),(70),
                 (71),(72),(73),(74),(75),(76),(77),(78),(79),(80),
                 (81),(82),(83),(84),(85),(86),(87),(88),(89),(90),
                 (91),(92),(93),(94),(95),(96),(97),(98),(99),(100),
                 (101),(102),(103),(104),(105),(106),(107),(108),(109),(110),
                 (111),(112),(113),(114),(115),(116),(117),(118),(119),(120),
                 (121),(122),(123),(124),(125),(126),(127),(128),(129),(130),
                 (131),(132),(133),(134),(135),(136),(137),(138),(139),(140),
                 (141),(142),(143),(144),(145),(146),(147),(148),(149),(150)) AS Numbers(n)
) AS T;

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
INSERT INTO Posts (UserID, Title, Content, CreatedAt, PrivacySetting)
SELECT 
    U.ID,
    CONCAT('Trading Update by User ', U.ID), 
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

-- Insert Tags
INSERT INTO Tags (TagName)
VALUES
('Stocks'),
('Trading'),
('Investing'),
('Finance'),
('Markets');

-- Insert PostTags
INSERT INTO PostTags (PostID, TagID)
SELECT 
    P.ID, 
    T.ID
FROM Posts P
JOIN Tags T ON T.ID % 4 = P.ID % 4
WHERE P.ID < 100;
