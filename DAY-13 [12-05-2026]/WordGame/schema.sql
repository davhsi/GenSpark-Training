CREATE TABLE users (
    userid SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    password  VARCHAR(255) NOT NULL
);

CREATE TABLE scores (
    id SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL,
    score INTEGER NOT NULL,
    FOREIGN KEY (userid) REFERENCES users(userid)
);

CREATE TABLE words (
    id SERIAL PRIMARY KEY,
    word VARCHAR(255) NOT NULL
);


INSERT INTO words (word) VALUES ('APPLE');
INSERT INTO words (word) VALUES ('MANGO');
INSERT INTO words (word) VALUES ('GRAPE');
INSERT INTO words (word) VALUES ('TRAIN');
INSERT INTO words (word) VALUES ('PLANT');
INSERT INTO words (word) VALUES ('BRAIN');