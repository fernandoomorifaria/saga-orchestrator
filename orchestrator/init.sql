-- So, I had this idea to use database enums but sum types don't play well with it and there's no easy way to handle this
-- CREATE TYPE state AS ENUM ('pending', 'completed', 'failed');

CREATE TABLE saga (
    id SERIAL PRIMARY KEY,
    saga_id UUID NOT NULL,
    state VARCHAR(255) NOT NULL,
    "order" JSONB NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    last_updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);
