extends Node3D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var mesh := TerrainMeshGenerator.generate_from_heightmap(
	heightmap_image,
	Vector2(terrain_width, terrain_depth)
)

$SnapMesh.mesh = mesh


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
