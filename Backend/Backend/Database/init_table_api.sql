-- YDB Table API schema init
-- Using official YDB .NET SDK (https://github.com/ydb-platform/ydb-dotnet-sdk)
-- Table API with individual columns

CREATE TABLE users (
    id Utf8,
    username Utf8,
    email Utf8,
    password_hash Utf8,
    image_url Utf8?,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);

CREATE TABLE recipes (
    id Utf8,
    title Utf8,
    description Utf8?,
    servings Int32?,
    prep_time Utf8?,
    cook_time Utf8?,
    image_url Utf8?,
    created_at Timestamp?,
    updated_at Timestamp?,
    PRIMARY KEY (id)
);

CREATE TABLE recipe_ingredients (
    recipe_id Utf8,
    ingredient_index Uint32,
    name Utf8,
    quantity Utf8,
    PRIMARY KEY (recipe_id, ingredient_index)
);

CREATE TABLE recipe_steps (
    recipe_id Utf8,
    step_index Uint32,
    instruction Utf8,
    PRIMARY KEY (recipe_id, step_index)
);

CREATE TABLE user_favorite_recipes (
    user_id Utf8,
    recipe_id Utf8,
    added_at Timestamp,
    PRIMARY KEY (user_id, recipe_id)
);

CREATE TABLE user_added_recipes (
    user_id Utf8,
    recipe_id Utf8,
    added_at Timestamp,
    PRIMARY KEY (user_id, recipe_id)
);

CREATE TABLE selections (
    id Utf8,
    title Utf8?,
    PRIMARY KEY (id)
);

CREATE TABLE selection_recipes (
    selection_id Utf8,
    recipe_id Utf8,
    recipe_index Uint32,
    PRIMARY KEY (selection_id, recipe_index)
);
