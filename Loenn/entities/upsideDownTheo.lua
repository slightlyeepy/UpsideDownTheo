local drawableSprite = require("structs.drawable_sprite")

local entity = {}

entity.name = "UpsideDownTheo/UpsideDownTheo"
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
    sprite.scaleY = -1

    return sprite
end

return entity
