shader_type spatial;

void fragment() {
	ALBEDO = vec3(1.0f, 1.0f, 1.0f);
}

void light() {
	DIFFUSE_LIGHT += clamp(dot(NORMAL, LIGHT), 0.0, 1.0) * ATTENUATION * LIGHT_COLOR;
}