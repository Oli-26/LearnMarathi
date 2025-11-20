-- Use the LearnMarathi database
USE LearnMarathi;

-- Create the BasicWords table
CREATE TABLE IF NOT EXISTS BasicWords (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    MarathiWord VARCHAR(100) NOT NULL,
    EnglishTranslation VARCHAR(100) NOT NULL,
    Pronunciation VARCHAR(150) NOT NULL,
    Category VARCHAR(50) NOT NULL
);

-- Clear existing data if any
TRUNCATE TABLE BasicWords;

-- Greetings & Common Phrases
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('नमस्कार', 'Hello/Namaste', 'namaskaar', 'Greetings'),
('धन्यवाद', 'Thank you', 'dhanyavaad', 'Greetings'),
('कृपया', 'Please', 'krupayaa', 'Greetings'),
('हो', 'Yes', 'ho', 'Greetings'),
('नाही', 'No', 'naahi', 'Greetings'),
('माफ करा', 'Sorry/Excuse me', 'maaf karaa', 'Greetings'),
('शुभ प्रभात', 'Good morning', 'shubh prabhaat', 'Greetings'),
('शुभ रात्री', 'Good night', 'shubh raatri', 'Greetings'),
('पुन्हा भेटू', 'See you again', 'punhaa bhetu', 'Greetings'),
('कसे आहात?', 'How are you?', 'kase aahaat?', 'Greetings');

-- Numbers (1-10)
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('एक', 'One', 'ek', 'Numbers'),
('दोन', 'Two', 'don', 'Numbers'),
('तीन', 'Three', 'teen', 'Numbers'),
('चार', 'Four', 'chaar', 'Numbers'),
('पाच', 'Five', 'paach', 'Numbers'),
('सहा', 'Six', 'sahaa', 'Numbers'),
('सात', 'Seven', 'saat', 'Numbers'),
('आठ', 'Eight', 'aath', 'Numbers'),
('नऊ', 'Nine', 'nau', 'Numbers'),
('दहा', 'Ten', 'dahaa', 'Numbers');

-- Colors
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('लाल', 'Red', 'laal', 'Colors'),
('निळा', 'Blue', 'nilaa', 'Colors'),
('हिरवा', 'Green', 'hiravaa', 'Colors'),
('पिवळा', 'Yellow', 'pivalaa', 'Colors'),
('काळा', 'Black', 'kaalaa', 'Colors'),
('पांढरा', 'White', 'paandharaa', 'Colors'),
('केशरी', 'Orange', 'keshari', 'Colors'),
('जांभळा', 'Purple', 'jaambhalaa', 'Colors'),
('तपकिरी', 'Brown', 'tapkiri', 'Colors'),
('गुलाबी', 'Pink', 'gulaabi', 'Colors');

-- Family Members
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('आई', 'Mother', 'aai', 'Family'),
('बाबा', 'Father', 'baabaa', 'Family'),
('भाऊ', 'Brother', 'bhaau', 'Family'),
('बहीण', 'Sister', 'baheen', 'Family'),
('आजी', 'Grandmother', 'aaji', 'Family'),
('आजोबा', 'Grandfather', 'aajobaa', 'Family'),
('मामा', 'Maternal uncle', 'maamaa', 'Family'),
('काका', 'Paternal uncle', 'kaakaa', 'Family'),
('मुलगा', 'Son/Boy', 'mulgaa', 'Family'),
('मुलगी', 'Daughter/Girl', 'mulgi', 'Family');

-- Common Objects
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('पाणी', 'Water', 'paani', 'Objects'),
('अन्न', 'Food', 'anna', 'Objects'),
('घर', 'House', 'ghar', 'Objects'),
('पुस्तक', 'Book', 'pustak', 'Objects'),
('टेबल', 'Table', 'tebal', 'Objects'),
('खुर्ची', 'Chair', 'khurchi', 'Objects'),
('दार', 'Door', 'daar', 'Objects'),
('खिडकी', 'Window', 'khidki', 'Objects'),
('गाडी', 'Car/Vehicle', 'gaadi', 'Objects'),
('फोन', 'Phone', 'phone', 'Objects');

-- Days of the Week
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('सोमवार', 'Monday', 'somvaar', 'Days'),
('मंगळवार', 'Tuesday', 'mangalvaar', 'Days'),
('बुधवार', 'Wednesday', 'budhvaar', 'Days'),
('गुरुवार', 'Thursday', 'guruvaar', 'Days'),
('शुक्रवार', 'Friday', 'shukravaar', 'Days'),
('शनिवार', 'Saturday', 'shanivaar', 'Days'),
('रविवार', 'Sunday', 'ravivaar', 'Days');

-- Basic Verbs
INSERT INTO BasicWords (MarathiWord, EnglishTranslation, Pronunciation, Category) VALUES
('जाणे', 'To go', 'jaane', 'Verbs'),
('येणे', 'To come', 'yene', 'Verbs'),
('खाणे', 'To eat', 'khaane', 'Verbs'),
('पिणे', 'To drink', 'pine', 'Verbs'),
('बोलणे', 'To speak', 'bolane', 'Verbs'),
('वाचणे', 'To read', 'vaachane', 'Verbs'),
('लिहिणे', 'To write', 'lihine', 'Verbs'),
('पाहणे', 'To see/watch', 'paahane', 'Verbs'),
('ऐकणे', 'To listen/hear', 'aikane', 'Verbs'),
('झोपणे', 'To sleep', 'jhopane', 'Verbs');

-- Verify the data
SELECT Category, COUNT(*) as Count
FROM BasicWords
GROUP BY Category
ORDER BY Category;

SELECT * FROM BasicWords ORDER BY Category, Id;
