-- YDB Document API schema init
-- Using official YDB .NET SDK (https://github.com/ydb-platform/ydb-dotnet-sdk)
-- Document API uses JSON documents stored in tables

CREATE TABLE users (
    id Utf8,
    data Json,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);

CREATE TABLE recipes (
    id Utf8,
    data Json,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);

CREATE TABLE selections (
    id Utf8,
    data Json,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);


