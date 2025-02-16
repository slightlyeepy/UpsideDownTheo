local drawableSprite = require("structs.drawable_sprite")

local entity = {}

entity.name = "UpsideDownTheo/LessInconvenientTheo"
entity.depth = 100
entity.placements = {
	name = "entity",
}

-- offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function entity.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return entity
