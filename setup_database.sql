-- Create the LearnMarathi database
CREATE DATABASE IF NOT EXISTS LearnMarathi;
USE LearnMarathi;

-- Create the MarathiCharacters table
CREATE TABLE IF NOT EXISTS MarathiCharacters (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    MarathiChar VARCHAR(10) NOT NULL,
    EnglishTranslation VARCHAR(50) NOT NULL,
    Pronunciation VARCHAR(100) NOT NULL,
    CharacterType VARCHAR(20) NOT NULL
);

-- Clear existing data if any
TRUNCATE TABLE MarathiCharacters;

-- Insert Vowels (स्वर)
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('अ', 'a', 'uh (like in "about")', 'Vowel'),
('आ', 'aa', 'aa (like in "father")', 'Vowel'),
('इ', 'i', 'i (like in "it")', 'Vowel'),
('ई', 'ee', 'ee (like in "bee")', 'Vowel'),
('उ', 'u', 'u (like in "put")', 'Vowel'),
('ऊ', 'oo', 'oo (like in "moon")', 'Vowel'),
('ऋ', 'ru', 'ri (like in "rip")', 'Vowel'),
('ए', 'e', 'ay (like in "say")', 'Vowel'),
('ऐ', 'ai', 'ai (like in "aisle")', 'Vowel'),
('ओ', 'o', 'o (like in "go")', 'Vowel'),
('औ', 'au', 'ow (like in "cow")', 'Vowel'),
('अं', 'am', 'ung (nasal sound)', 'Vowel'),
('अः', 'ah', 'ah (visarga)', 'Vowel');

-- Insert Consonants (व्यंजन)
-- Velar consonants (कवर्ग)
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('क', 'ka', 'kuh', 'Consonant'),
('ख', 'kha', 'khuh (aspirated k)', 'Consonant'),
('ग', 'ga', 'guh', 'Consonant'),
('घ', 'gha', 'ghuh (aspirated g)', 'Consonant'),
('ङ', 'nga', 'nguh (like "ng" in "sing")', 'Consonant');

-- Palatal consonants (चवर्ग)
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('च', 'cha', 'chuh', 'Consonant'),
('छ', 'chha', 'chhuh (aspirated ch)', 'Consonant'),
('ज', 'ja', 'juh', 'Consonant'),
('झ', 'jha', 'jhuh (aspirated j)', 'Consonant'),
('ञ', 'nya', 'nyuh (like "ny" in "canyon")', 'Consonant');

-- Retroflex consonants (टवर्ग)
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('ट', 'ta', 'tuh (retroflex)', 'Consonant'),
('ठ', 'tha', 'thuh (retroflex aspirated)', 'Consonant'),
('ड', 'da', 'duh (retroflex)', 'Consonant'),
('ढ', 'dha', 'dhuh (retroflex aspirated)', 'Consonant'),
('ण', 'na', 'nuh (retroflex)', 'Consonant');

-- Dental consonants (तवर्ग)
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('त', 'ta', 'tuh (dental)', 'Consonant'),
('थ', 'tha', 'thuh (dental aspirated)', 'Consonant'),
('द', 'da', 'duh (dental)', 'Consonant'),
('ध', 'dha', 'dhuh (dental aspirated)', 'Consonant'),
('न', 'na', 'nuh (dental)', 'Consonant');

-- Labial consonants (पवर्ग)
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('प', 'pa', 'puh', 'Consonant'),
('फ', 'pha', 'phuh (aspirated p)', 'Consonant'),
('ब', 'ba', 'buh', 'Consonant'),
('भ', 'bha', 'bhuh (aspirated b)', 'Consonant'),
('म', 'ma', 'muh', 'Consonant');

-- Semivowels and Sibilants
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('य', 'ya', 'yuh', 'Consonant'),
('र', 'ra', 'ruh', 'Consonant'),
('ल', 'la', 'luh', 'Consonant'),
('व', 'va', 'vuh (or wuh)', 'Consonant'),
('श', 'sha', 'shuh (palatal)', 'Consonant'),
('ष', 'sha', 'shuh (retroflex)', 'Consonant'),
('स', 'sa', 'suh', 'Consonant'),
('ह', 'ha', 'huh', 'Consonant');

-- Additional characters
INSERT INTO MarathiCharacters (MarathiChar, EnglishTranslation, Pronunciation, CharacterType) VALUES
('ळ', 'la', 'luh (retroflex l)', 'Consonant'),
('क्ष', 'ksha', 'kshuh', 'Consonant'),
('ज्ञ', 'jnya', 'gyuh', 'Consonant');

-- Verify the data
SELECT CharacterType, COUNT(*) as Count
FROM MarathiCharacters
GROUP BY CharacterType;

SELECT * FROM MarathiCharacters ORDER BY Id;
