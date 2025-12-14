CREATE TABLE wallet (
    id SERIAL PRIMARY KEY,
    customer_id INTEGER NOT NULL UNIQUE,
    balance NUMERIC(18, 2) NOT NULL DEFAULT 0.00,
    CONSTRAINT positive_balance CHECK (balance >= 0)
);

INSERT INTO wallet (customer_id, balance)
VALUES (1, 10.00);
