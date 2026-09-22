-- =============================================
-- Päiväkirjasovellus - Tietokannan luonti
-- =============================================

-- Luodaan tietokanta, jos sitä ei ole vielä olemassa
CREATE DATABASE IF NOT EXISTS PaivakirjaDB
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- Otetaan tietokanta käyttöön
USE PaivakirjaDB;

-- Poistetaan vanha taulu jos se on olemassa
DROP TABLE IF EXISTS DiaryEntries;

-- Luodaan taulu DiaryEntries
CREATE TABLE DiaryEntries (
    Id          INT AUTO_INCREMENT PRIMARY KEY,   -- Yksilöllinen tunniste (primary key)
    EntryDate   DATETIME NOT NULL,                -- Päivämäärä ja aika
    Title       VARCHAR(200) NOT NULL,            -- Otsikko
    Content     TEXT,                             -- Sisältö
    
    -- Indeksi päivämäärälle
    INDEX idx_entrydate (EntryDate)
);