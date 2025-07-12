-- =====================================================
-- Mock Data Script for OFX and OfxTransaction Tables
-- =====================================================

-- Declare variables for mock data generation
DECLARE @BaseUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @CurrentDate DATETIME = GETDATE();

-- =====================================================
-- 1. Create sample Banks if none exist
-- =====================================================
DECLARE @BankId1 UNIQUEIDENTIFIER, @BankId2 UNIQUEIDENTIFIER, @BankId3 UNIQUEIDENTIFIER;

-- Try to get existing bank IDs first
SELECT TOP 1 @BankId1 = Id FROM Banks WHERE DeletedAt IS NULL ORDER BY CreatedAt;

-- If no banks exist, we need to create some sample banks
IF @BankId1 IS NULL
BEGIN
    PRINT 'No existing banks found. Creating sample banks...'
    
    -- Create sample BaseAssetHolders first
    DECLARE @BaseAssetHolder1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @BaseAssetHolder2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @BaseAssetHolder3 UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO BaseAssetHolders (Id, Name, Email, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
    VALUES 
    (@BaseAssetHolder1, 'Banco do Brasil S.A.', 'contato@bb.com.br', @CurrentDate, @CurrentDate, NULL, @BaseUserId),
    (@BaseAssetHolder2, 'Itaú Unibanco S.A.', 'contato@itau.com.br', @CurrentDate, @CurrentDate, NULL, @BaseUserId),
    (@BaseAssetHolder3, 'Banco Santander Brasil S.A.', 'contato@santander.com.br', @CurrentDate, @CurrentDate, NULL, @BaseUserId);
    
    -- Create sample Bank entities
    SET @BankId1 = NEWID();
    SET @BankId2 = NEWID();
    SET @BankId3 = NEWID();
    
    INSERT INTO Banks (Id, BaseAssetHolderId, Code, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
    VALUES 
    (@BankId1, @BaseAssetHolder1, '001', @CurrentDate, @CurrentDate, NULL, @BaseUserId),
    (@BankId2, @BaseAssetHolder2, '341', @CurrentDate, @CurrentDate, NULL, @BaseUserId),
    (@BankId3, @BaseAssetHolder3, '033', @CurrentDate, @CurrentDate, NULL, @BaseUserId);
    
    PRINT 'Sample banks created successfully.'
END
ELSE
BEGIN
    PRINT 'Using existing banks from database.'
    
    -- Get existing bank IDs
    SELECT @BankId1 = Id FROM Banks WHERE DeletedAt IS NULL ORDER BY CreatedAt OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;
    SELECT @BankId2 = Id FROM Banks WHERE DeletedAt IS NULL AND Id != @BankId1 ORDER BY CreatedAt OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;
    SELECT @BankId3 = Id FROM Banks WHERE DeletedAt IS NULL AND Id NOT IN (@BankId1, ISNULL(@BankId2, NEWID())) ORDER BY CreatedAt OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;
    
    -- If we don't have enough banks, use the first one for all
    IF @BankId2 IS NULL SET @BankId2 = @BankId1;
    IF @BankId3 IS NULL SET @BankId3 = @BankId1;
END

PRINT 'Bank IDs selected:'
PRINT 'BankId1: ' + CAST(@BankId1 AS VARCHAR(36))
PRINT 'BankId2: ' + CAST(@BankId2 AS VARCHAR(36))
PRINT 'BankId3: ' + CAST(@BankId3 AS VARCHAR(36))

-- =====================================================
-- 2. Insert Mock OFX Records
-- =====================================================

-- Clear any existing test data first (optional - uncomment if needed)
-- DELETE FROM OfxTransactions WHERE OfxId IN (SELECT Id FROM Ofxs WHERE FileName LIKE '%_2024.ofx');
-- DELETE FROM Ofxs WHERE FileName LIKE '%_2024.ofx';

DECLARE @OfxId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @OfxId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @OfxId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @OfxId4 UNIQUEIDENTIFIER = NEWID();
DECLARE @OfxId5 UNIQUEIDENTIFIER = NEWID();
DECLARE @OfxId6 UNIQUEIDENTIFIER = NEWID();

-- OFX File 1 - Bank of Brazil Statement
INSERT INTO Ofxs (Id, BankId, FileName, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@OfxId1, @BankId1, 'banco_brasil_jan_2024.ofx', DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId);

-- OFX File 2 - Itaú Statement  
INSERT INTO Ofxs (Id, BankId, FileName, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@OfxId2, @BankId2, 'itau_checking_feb_2024.ofx', DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId);

-- OFX File 3 - Santander Statement
INSERT INTO Ofxs (Id, BankId, FileName, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@OfxId3, @BankId3, 'santander_business_mar_2024.ofx', DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId);

-- OFX File 4 - Bank of Brazil Credit Card
INSERT INTO Ofxs (Id, BankId, FileName, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@OfxId4, @BankId1, 'bb_credit_card_apr_2024.ofx', DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId);

-- OFX File 5 - Itaú Investment Account
INSERT INTO Ofxs (Id, BankId, FileName, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@OfxId5, @BankId2, 'itau_investment_may_2024.ofx', DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId);

-- OFX File 6 - Caixa Econômica Federal
INSERT INTO Ofxs (Id, BankId, FileName, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
(@OfxId6, @BankId1, 'cef_conta_corrente_jun_2024.ofx', DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId);

PRINT 'OFX files created successfully.'

-- =====================================================
-- 3. Insert Mock OfxTransaction Records
-- =====================================================

-- Transactions for Bank of Brazil (OfxId1) - January 2024
INSERT INTO OfxTransactions (Id, Date, Value, Description, FitId, OfxId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Salary deposit
(NEWID(), '2024-01-05', 8500.00, 'SALARIO EMPRESA XYZ LTDA', 'BB240105001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Rent payment
(NEWID(), '2024-01-10', 2200.00, 'ALUGUEL IMOBILIARIA ABC', 'BB240110001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Utility bills
(NEWID(), '2024-01-12', 185.50, 'CONTA LUZ ENEL SP', 'BB240112001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-01-15', 95.30, 'CONTA AGUA SABESP', 'BB240115001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- ATM withdrawal
(NEWID(), '2024-01-18', 500.00, 'SAQUE CARTAO AG 1234', 'BB240118001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Online purchase
(NEWID(), '2024-01-20', 299.99, 'COMPRA ONLINE AMAZON BR', 'BB240120001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- PIX transfer received
(NEWID(), '2024-01-22', 750.00, 'PIX RECEBIDO JOAO SILVA', 'BB240122001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- PIX transfer sent
(NEWID(), '2024-01-25', 350.00, 'PIX ENVIADO MARIA SANTOS', 'BB240125001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId),
-- Investment transfer
(NEWID(), '2024-01-28', 1000.00, 'APLICACAO TESOURO DIRETO', 'BB240128001', @OfxId1, DATEADD(day, -30, @CurrentDate), DATEADD(day, -30, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Itaú (OfxId2) - February 2024
INSERT INTO OfxTransactions (Id, Date, Value, Description, FitId, OfxId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Freelance payment
(NEWID(), '2024-02-01', 3200.00, 'PAGAMENTO FREELANCE TECH', 'IT240201001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Credit card payment
(NEWID(), '2024-02-05', 1850.75, 'PAGTO CARTAO CREDITO', 'IT240205001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Grocery shopping
(NEWID(), '2024-02-08', 425.80, 'SUPERMERCADO EXTRA', 'IT240208001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Gas station
(NEWID(), '2024-02-10', 120.00, 'POSTO SHELL BR', 'IT240210001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Bank fee
(NEWID(), '2024-02-12', 15.90, 'TARIFA PACOTE SERVICOS', 'IT240212001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Restaurant
(NEWID(), '2024-02-14', 89.50, 'RESTAURANTE BELLA VISTA', 'IT240214001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- TED transfer
(NEWID(), '2024-02-18', 2500.00, 'TED ENVIADA CONTA POUPANCA', 'IT240218001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId),
-- Dividend payment
(NEWID(), '2024-02-20', 456.32, 'DIVIDENDOS PETR4', 'IT240220001', @OfxId2, DATEADD(day, -25, @CurrentDate), DATEADD(day, -25, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Santander (OfxId3) - March 2024
INSERT INTO OfxTransactions (Id, Date, Value, Description, FitId, OfxId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Business revenue
(NEWID(), '2024-03-01', 15000.00, 'RECEITA VENDAS EMPRESA', 'ST240301001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Supplier payment
(NEWID(), '2024-03-03', 5500.00, 'PAGTO FORNECEDOR ABC LTDA', 'ST240303001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Employee salaries
(NEWID(), '2024-03-05', 12000.00, 'FOLHA PAGAMENTO FUNCIONARIOS', 'ST240305001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Tax payment
(NEWID(), '2024-03-10', 2800.00, 'IMPOSTOS DAS SIMPLES', 'ST240310001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Office rent
(NEWID(), '2024-03-12', 4200.00, 'ALUGUEL ESCRITORIO', 'ST240312001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Equipment purchase
(NEWID(), '2024-03-15', 3500.00, 'COMPRA EQUIPAMENTOS', 'ST240315001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Insurance payment
(NEWID(), '2024-03-18', 890.00, 'SEGURO EMPRESARIAL', 'ST240318001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId),
-- Customer payment
(NEWID(), '2024-03-22', 8500.00, 'RECEBIMENTO CLIENTE XYZ', 'ST240322001', @OfxId3, DATEADD(day, -20, @CurrentDate), DATEADD(day, -20, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Bank of Brazil Credit Card (OfxId4) - April 2024
INSERT INTO OfxTransactions (Id, Date, Value, Description, FitId, OfxId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Travel expenses
(NEWID(), '2024-04-02', 1200.00, 'HOTEL COPACABANA RJ', 'BB240402001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-04-03', 350.00, 'PASSAGEM AEREA GOL', 'BB240403001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Dining
(NEWID(), '2024-04-05', 180.50, 'RESTAURANTE FOGO DE CHAO', 'BB240405001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Online subscriptions
(NEWID(), '2024-04-08', 49.90, 'NETFLIX BRASIL', 'BB240408001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-04-08', 29.90, 'SPOTIFY PREMIUM', 'BB240408002', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Shopping
(NEWID(), '2024-04-12', 650.00, 'SHOPPING IGUATEMI SP', 'BB240412001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Pharmacy
(NEWID(), '2024-04-15', 125.30, 'FARMACIA DROGASIL', 'BB240415001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId),
-- Electronics
(NEWID(), '2024-04-20', 899.99, 'MAGAZINELUIZA SMARTPHONE', 'BB240420001', @OfxId4, DATEADD(day, -15, @CurrentDate), DATEADD(day, -15, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Itaú Investment (OfxId5) - May 2024
INSERT INTO OfxTransactions (Id, Date, Value, Description, FitId, OfxId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Investment deposits
(NEWID(), '2024-05-02', 5000.00, 'APLICACAO CDB ITAU', 'IT240502001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-05-05', 3000.00, 'COMPRA ACOES VALE3', 'IT240505001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Investment returns
(NEWID(), '2024-05-08', 125.50, 'RENDIMENTO CDB', 'IT240508001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
(NEWID(), '2024-05-10', 89.75, 'DIVIDENDOS ITUB4', 'IT240510001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Fund investment
(NEWID(), '2024-05-15', 2500.00, 'APLICACAO FUNDO MULTIMERCADO', 'IT240515001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Redemption
(NEWID(), '2024-05-18', 1500.00, 'RESGATE TESOURO SELIC', 'IT240518001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Brokerage fee
(NEWID(), '2024-05-20', 8.90, 'TAXA CORRETAGEM', 'IT240520001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId),
-- Stock sale
(NEWID(), '2024-05-25', 4200.00, 'VENDA ACOES PETR4', 'IT240525001', @OfxId5, DATEADD(day, -10, @CurrentDate), DATEADD(day, -10, @CurrentDate), NULL, @BaseUserId);

-- Transactions for Caixa Econômica Federal (OfxId6) - June 2024
INSERT INTO OfxTransactions (Id, Date, Value, Description, FitId, OfxId, CreatedAt, UpdatedAt, DeletedAt, LastModifiedBy)
VALUES 
-- Government benefits
(NEWID(), '2024-06-01', 600.00, 'AUXILIO EMERGENCIAL', 'CEF240601001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- FGTS withdrawal
(NEWID(), '2024-06-03', 2800.00, 'SAQUE FGTS', 'CEF240603001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Loan payment
(NEWID(), '2024-06-05', 450.00, 'PRESTACAO FINANCIAMENTO', 'CEF240605001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Lottery ticket
(NEWID(), '2024-06-08', 25.00, 'MEGA SENA ONLINE', 'CEF240608001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Transfer to savings
(NEWID(), '2024-06-10', 1000.00, 'TRANSFERENCIA POUPANCA', 'CEF240610001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Public transportation
(NEWID(), '2024-06-12', 150.00, 'RECARGA BILHETE UNICO', 'CEF240612001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Medical expenses
(NEWID(), '2024-06-15', 280.00, 'CONSULTA MEDICA PARTICULAR', 'CEF240615001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId),
-- Cashback
(NEWID(), '2024-06-18', 35.50, 'CASHBACK PROGRAMA FIDELIDADE', 'CEF240618001', @OfxId6, DATEADD(day, -5, @CurrentDate), DATEADD(day, -5, @CurrentDate), NULL, @BaseUserId);

-- =====================================================
-- 4. Summary Information
-- =====================================================

PRINT '=============================================='
PRINT 'Mock Data Insertion Complete!'
PRINT '=============================================='
PRINT 'OFX Files Created: 6'
PRINT 'OfxTransactions Created: 49'
PRINT ''
PRINT 'File Names:'
PRINT '- banco_brasil_jan_2024.ofx (9 transactions)'
PRINT '- itau_checking_feb_2024.ofx (8 transactions)'
PRINT '- santander_business_mar_2024.ofx (8 transactions)'
PRINT '- bb_credit_card_apr_2024.ofx (8 transactions)'
PRINT '- itau_investment_may_2024.ofx (8 transactions)'
PRINT '- cef_conta_corrente_jun_2024.ofx (8 transactions)'
PRINT ''
PRINT 'Transaction Types Include:'
PRINT '- Salary deposits and payments'
PRINT '- PIX transfers (sent and received)'
PRINT '- Credit card transactions'
PRINT '- Investment transactions'
PRINT '- Business transactions'
PRINT '- Utility payments'
PRINT '- Government benefits'
PRINT '- Banking fees and charges'
PRINT '=============================================='

-- =====================================================
-- 5. Verification Queries
-- =====================================================

-- Count OFX files
SELECT 'OFX Files Count' as DataType, COUNT(*) as Count FROM Ofxs WHERE DeletedAt IS NULL AND FileName LIKE '%_2024.ofx';

-- Count OfxTransactions
SELECT 'OfxTransactions Count' as DataType, COUNT(*) as Count FROM OfxTransactions WHERE DeletedAt IS NULL AND OfxId IN (SELECT Id FROM Ofxs WHERE FileName LIKE '%_2024.ofx');

-- OFX files with transaction counts
SELECT 
    o.FileName,
    o.CreatedAt,
    COUNT(ot.Id) as TransactionCount,
    SUM(CASE WHEN ot.Value > 0 THEN ot.Value ELSE 0 END) as TotalCredits,
    SUM(CASE WHEN ot.Value < 0 THEN ABS(ot.Value) ELSE 0 END) as TotalDebits
FROM Ofxs o
LEFT JOIN OfxTransactions ot ON o.Id = ot.OfxId AND ot.DeletedAt IS NULL
WHERE o.DeletedAt IS NULL AND o.FileName LIKE '%_2024.ofx'
GROUP BY o.Id, o.FileName, o.CreatedAt
ORDER BY o.CreatedAt DESC; 