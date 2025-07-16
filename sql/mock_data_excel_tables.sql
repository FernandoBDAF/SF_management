-- =====================================================
-- Mock Data Script for Excel and ExcelTransaction Tables
-- =====================================================

-- Declare variables for mock data generation
DECLARE @BaseUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @CurrentDate DATETIME = GETDATE();

-- =====================================================
-- 1. Get existing PokerManager IDs
-- =====================================================
DECLARE @PokerManagerId1 UNIQUEIDENTIFIER, @PokerManagerId2 UNIQUEIDENTIFIER, @PokerManagerId3 UNIQUEIDENTIFIER;

-- Get existing poker manager IDs
SELECT TOP 3 @PokerManagerId1 = Id FROM PokerManagers WHERE DeletedAt IS NULL ORDER BY CreatedAt;
SELECT @PokerManagerId2 = Id FROM PokerManagers WHERE DeletedAt IS NULL AND Id != @PokerManagerId1 ORDER BY CreatedAt OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY;
SELECT @PokerManagerId3 = Id FROM PokerManagers WHERE DeletedAt IS NULL AND Id NOT IN (@PokerManagerId1, ISNULL(@PokerManagerId2, NEWID())) ORDER BY CreatedAt OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY;

-- If we don't have enough poker managers, use the first one for all
IF @PokerManagerId1 IS NULL
BEGIN
    PRINT 'ERROR: No PokerManagers found in database. Please create PokerManagers first.'
    RETURN;
END

IF @PokerManagerId2 IS NULL SET @PokerManagerId2 = @PokerManagerId1;
IF @PokerManagerId3 IS NULL SET @PokerManagerId3 = @PokerManagerId1;

PRINT 'Using PokerManager IDs:'
PRINT 'PokerManagerId1: ' + CAST(@PokerManagerId1 AS VARCHAR(36))
PRINT 'PokerManagerId2: ' + CAST(@PokerManagerId2 AS VARCHAR(36))
PRINT 'PokerManagerId3: ' + CAST(@PokerManagerId3 AS VARCHAR(36))

-- =====================================================
-- 2. Insert Mock Excel Records
-- =====================================================

-- Clear any existing test data first (optional - uncomment if needed)
-- DELETE FROM ExcelTransactions WHERE ExcelId IN (SELECT Id FROM Excels WHERE FileName LIKE '%_2024.xlsx');
-- DELETE FROM Excels WHERE FileName LIKE '%_2024.xlsx';

-- Pre-declare Excel IDs
DECLARE @ExcelId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @ExcelId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @ExcelId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @ExcelId4 UNIQUEIDENTIFIER = NEWID();
DECLARE @ExcelId5 UNIQUEIDENTIFIER = NEWID();
DECLARE @ExcelId6 UNIQUEIDENTIFIER = NEWID();

-- Excel File 1 - PokerStars January 2024
INSERT INTO Excels (Id, PokerManagerId, FileName, FileType, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@ExcelId1, @PokerManagerId1, 'pokerstars_jan_2024.xlsx', 'PokerStars', DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId);

-- Excel File 2 - GGPoker February 2024
INSERT INTO Excels (Id, PokerManagerId, FileName, FileType, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@ExcelId2, @PokerManagerId2, 'ggpoker_feb_2024.xlsx', 'GGPoker', DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId);

-- Excel File 3 - Americas Cardroom March 2024
INSERT INTO Excels (Id, PokerManagerId, FileName, FileType, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@ExcelId3, @PokerManagerId3, 'acr_mar_2024.xlsx', 'AmericasCardroom', DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId);

-- Excel File 4 - PokerStars Tournament Report April 2024
INSERT INTO Excels (Id, PokerManagerId, FileName, FileType, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@ExcelId4, @PokerManagerId1, 'pokerstars_tournaments_apr_2024.xlsx', 'PokerStars', DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId);

-- Excel File 5 - GGPoker Cash Games May 2024
INSERT INTO Excels (Id, PokerManagerId, FileName, FileType, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@ExcelId5, @PokerManagerId2, 'ggpoker_cash_may_2024.xlsx', 'GGPoker', DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId);

-- Excel File 6 - Multi-Site Report June 2024
INSERT INTO Excels (Id, PokerManagerId, FileName, FileType, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@ExcelId6, @PokerManagerId1, 'multi_site_report_jun_2024.xlsx', 'Mixed', DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId);

PRINT 'Excel files created successfully.'

-- =====================================================
-- 3. Insert Mock ExcelTransaction Records
-- =====================================================

-- Transactions for PokerStars (ExcelId1) - January 2024
INSERT INTO ExcelTransactions (Id, Date, Coins, Description, ExcelNickname, ExcelWallet, ExcelId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Tournament winnings
(NEWID(), '2024-01-05', 1250.50, 'Sunday Million Final Table', 'ProPlayer2024', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Cash game sessions
(NEWID(), '2024-01-08', 450.75, 'NL Hold''em 2/5 Cash Game', 'CashKing88', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-01-10', -125.25, 'PLO 1/2 Session Loss', 'OmahaGrinder', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Sit & Go tournaments
(NEWID(), '2024-01-12', 89.50, '$50 Turbo SNG 1st Place', 'SNGPro', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-01-15', 156.00, 'Spin & Go Hot Streak', 'SpinMaster', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Multi-table tournament
(NEWID(), '2024-01-18', 2100.00, 'SCOOP Event #15 Victory', 'MTTChamp', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Losses
(NEWID(), '2024-01-20', -300.00, 'Bad Beat in High Stakes', 'HighRoller', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Rakeback
(NEWID(), '2024-01-25', 75.25, 'Weekly Rakeback Payment', 'RegularGrinder', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Deposit/Withdrawal
(NEWID(), '2024-01-28', 500.00, 'Account Deposit', 'BankrollBuilder', 'PokerStars', @ExcelId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId);

-- Transactions for GGPoker (ExcelId2) - February 2024
INSERT INTO ExcelTransactions (Id, Date, Coins, Description, ExcelNickname, ExcelWallet, ExcelId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Rush & Cash sessions
(NEWID(), '2024-02-02', 325.80, 'Rush & Cash NL25 Session', 'RushExpert', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-02-05', 678.90, 'Rush & Cash NL50 Big Win', 'FastFold Pro', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Battle Royale tournaments
(NEWID(), '2024-02-08', 1450.00, 'Battle Royale Championship', 'BattleWarrior', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- All-in or Fold
(NEWID(), '2024-02-10', 89.25, 'AoF Hyper Turbo Series', 'AllInMaster', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Flip & Go
(NEWID(), '2024-02-12', 234.50, 'Flip & Go Hot Run', 'FlipSpecialist', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Regular MTT
(NEWID(), '2024-02-15', 890.75, 'Daily Main Event Final Table', 'GGChampion', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Losses
(NEWID(), '2024-02-18', -156.30, 'Cooler in High Stakes Cash', 'UnluckyTonight', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Bonus
(NEWID(), '2024-02-22', 125.00, 'Welcome Bonus Release', 'NewPlayer2024', 'GGPoker', @ExcelId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Americas Cardroom (ExcelId3) - March 2024
INSERT INTO ExcelTransactions (Id, Date, Coins, Description, ExcelNickname, ExcelWallet, ExcelId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- OSS Tournament Series
(NEWID(), '2024-03-01', 3200.00, 'OSS Main Event Deep Run', 'OSSGrinder', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Cash games
(NEWID(), '2024-03-03', 445.60, 'PLO Hi-Lo 2/4 Session', 'PLOSpecialist', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-03-05', 278.90, 'NL Hold''em 1/2 Zoom', 'ZoomShark', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Jackpot Sit & Go
(NEWID(), '2024-03-08', 1750.00, 'Jackpot SNG Max Multiplier', 'JackpotHunter', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Regular tournaments
(NEWID(), '2024-03-10', 567.25, '$100 Freezeout Victory', 'TourneyShark', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Beast progressive rake race
(NEWID(), '2024-03-12', 189.50, 'Beast Rake Race Reward', 'RakeRaceKing', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Losses
(NEWID(), '2024-03-15', -234.75, 'Variance Downswing', 'VarianceVictim', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Withdrawal
(NEWID(), '2024-03-20', -1000.00, 'Cashout to Bitcoin', 'CryptoWithdraw', 'AmericasCardroom', @ExcelId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId);

-- Transactions for PokerStars Tournaments (ExcelId4) - April 2024
INSERT INTO ExcelTransactions (Id, Date, Coins, Description, ExcelNickname, ExcelWallet, ExcelId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- WCOOP satellites
(NEWID(), '2024-04-02', 215.00, 'WCOOP Satellite Win', 'SatelliteAce', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Sunday tournaments
(NEWID(), '2024-04-07', 1890.50, 'Sunday Storm Deep Run', 'SundayWarrior', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-04-14', 456.75, 'Sunday Warm-Up Cash', 'WarmUpPro', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Micro stakes grind
(NEWID(), '2024-04-16', 23.45, 'Micro MTT Volume Play', 'MicroGrinder', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- High roller attempt
(NEWID(), '2024-04-18', -530.00, 'High Roller Bust', 'HighStakesHero', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Progressive knockout
(NEWID(), '2024-04-21', 678.25, 'PKO Bounty Hunter Win', 'BountyCollector', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Turbo series
(NEWID(), '2024-04-25', 123.80, 'Turbo Series Event', 'TurboSpecialist', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Heads-up championship
(NEWID(), '2024-04-28', 890.00, 'HU Championship Victory', 'HeadsUpKing', 'PokerStars', @ExcelId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId);

-- Transactions for GGPoker Cash Games (ExcelId5) - May 2024
INSERT INTO ExcelTransactions (Id, Date, Coins, Description, ExcelNickname, ExcelWallet, ExcelId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Regular cash games
(NEWID(), '2024-05-02', 567.80, 'NL100 6-max Session', 'SixMaxShark', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-05-05', 1234.50, 'NL200 Full Ring Win', 'FullRingPro', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Short deck poker
(NEWID(), '2024-05-08', 345.25, 'Short Deck NL50 Session', 'ShortDeckAce', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Mixed games
(NEWID(), '2024-05-10', 189.75, '8-Game Mix Session', 'MixedGamesPro', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- High stakes attempt
(NEWID(), '2024-05-12', -678.90, 'NL500 Downswing', 'HighStakesGrinder', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Rush & Cash marathon
(NEWID(), '2024-05-15', 456.30, 'Rush & Cash Marathon', 'RushMarathoner', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Straddle games
(NEWID(), '2024-05-18', 234.60, 'Straddle NL25 Session', 'StraddleSpecialist', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Fish buffet rewards
(NEWID(), '2024-05-22', 89.40, 'Fish Buffet Cashback', 'RewardsHunter', 'GGPoker', @ExcelId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Multi-Site Report (ExcelId6) - June 2024
INSERT INTO ExcelTransactions (Id, Date, Coins, Description, ExcelNickname, ExcelWallet, ExcelId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Cross-platform tournament
(NEWID(), '2024-06-01', 2340.00, 'Multi-Site Tournament Series', 'MultiSitePro', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Arbitrage opportunities
(NEWID(), '2024-06-03', 156.75, 'Site Arbitrage Profit', 'ArbitrageKing', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Bonus hunting
(NEWID(), '2024-06-05', 345.50, 'Bonus Clearing Profit', 'BonusHunter', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Rakeback comparison
(NEWID(), '2024-06-08', 123.25, 'Optimal Rakeback Route', 'RakebackOptimizer', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Network exclusive events
(NEWID(), '2024-06-10', 567.80, 'Network Exclusive Win', 'ExclusivePlayer', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Cryptocurrency transactions
(NEWID(), '2024-06-12', 789.90, 'Crypto Deposit Bonus', 'CryptoPlayer', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- VIP rewards
(NEWID(), '2024-06-15', 234.40, 'VIP Status Rewards', 'VIPGrinder', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Final summary
(NEWID(), '2024-06-18', 1456.70, 'Monthly Profit Summary', 'ProfitTracker', 'Mixed', @ExcelId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId);

-- =====================================================
-- 4. Summary Information
-- =====================================================

PRINT '=============================================='
PRINT 'Mock Data Insertion Complete!'
PRINT '=============================================='
PRINT 'Excel Files Created: 6'
PRINT 'ExcelTransactions Created: 48'
PRINT ''
PRINT 'File Names:'
PRINT '- pokerstars_jan_2024.xlsx (9 transactions)'
PRINT '- ggpoker_feb_2024.xlsx (8 transactions)'
PRINT '- acr_mar_2024.xlsx (8 transactions)'
PRINT '- pokerstars_tournaments_apr_2024.xlsx (8 transactions)'
PRINT '- ggpoker_cash_may_2024.xlsx (8 transactions)'
PRINT '- multi_site_report_jun_2024.xlsx (8 transactions)'
PRINT ''
PRINT 'Transaction Types Include:'
PRINT '- Tournament winnings and losses'
PRINT '- Cash game sessions (various stakes)'
PRINT '- Sit & Go and Spin & Go results'
PRINT '- Rakeback and bonus payments'
PRINT '- Deposits and withdrawals'
PRINT '- Multi-site arbitrage and bonus hunting'
PRINT '- VIP rewards and cryptocurrency transactions'
PRINT '=============================================='

-- =====================================================
-- 5. Verification Queries
-- =====================================================

-- Count Excel files
SELECT 'Excel Files Count' as DataType, COUNT(*) as Count FROM Excels WHERE DeletedAt IS NULL AND FileName LIKE '%_2024.xlsx';

-- Count ExcelTransactions
SELECT 'ExcelTransactions Count' as DataType, COUNT(*) as Count FROM ExcelTransactions WHERE DeletedAt IS NULL AND ExcelId IN (SELECT Id FROM Excels WHERE FileName LIKE '%_2024.xlsx');

-- Excel files with transaction counts and profit/loss summary
SELECT 
    e.FileName,
    e.FileType,
    e.CreatedAt,
    COUNT(et.Id) as TransactionCount,
    SUM(CASE WHEN et.Coins > 0 THEN et.Coins ELSE 0 END) as TotalWinnings,
    SUM(CASE WHEN et.Coins < 0 THEN ABS(et.Coins) ELSE 0 END) as TotalLosses,
    SUM(et.Coins) as NetProfit,
    MIN(et.Date) as EarliestTransaction,
    MAX(et.Date) as LatestTransaction
FROM Excels e
LEFT JOIN ExcelTransactions et ON e.Id = et.ExcelId AND et.DeletedAt IS NULL
WHERE e.DeletedAt IS NULL AND e.FileName LIKE '%_2024.xlsx'
GROUP BY e.Id, e.FileName, e.FileType, e.CreatedAt
ORDER BY e.CreatedAt DESC;

-- Top performing players by total winnings
SELECT 
    et.ExcelNickname,
    et.ExcelWallet,
    COUNT(*) as TransactionCount,
    SUM(et.Coins) as TotalProfit,
    AVG(et.Coins) as AvgTransaction,
    MAX(et.Coins) as BiggestWin,
    MIN(et.Coins) as BiggestLoss
FROM ExcelTransactions et
INNER JOIN Excels e ON et.ExcelId = e.Id
WHERE et.DeletedAt IS NULL AND e.FileName LIKE '%_2024.xlsx'
GROUP BY et.ExcelNickname, et.ExcelWallet
HAVING SUM(et.Coins) > 0
ORDER BY SUM(et.Coins) DESC; 