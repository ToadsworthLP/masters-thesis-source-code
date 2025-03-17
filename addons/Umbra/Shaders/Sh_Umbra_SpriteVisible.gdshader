shader_type spatial;

uniform sampler2D mainTexture : source_color, filter_nearest;
uniform sampler2D normalMapTexture : hint_normal, filter_nearest;

void fragment() {
	vec4 sourceColor = texture(mainTexture, UV);

	ALBEDO = vec3(1.0f, 1.0f, 1.0f) * COLOR.rgb;
	ALPHA = sourceColor.a * COLOR.a;
	ALPHA_SCISSOR_THRESHOLD = 0.5f;

	NORMAL_MAP = texture(normalMapTexture, UV).rgb;
}

// LIGHTING

// Triangle Wave
float T(float z) {
    return z >= 0.5 ? 2.-2.*z : 2.*z;
}

// R dither mask
float intensity(ivec2 pixel) {
    const float a1 = 0.75487766624669276;
    const float a2 = 0.569840290998;
    return fract(a1 * float(pixel.x) + a2 * float(pixel.y));
}

float dither(float gray, int grayLevelCount, vec4 fragmentCoordinates) {
    // Calculated noised gray value
    float noised = (2./float(grayLevelCount)) * T(intensity(ivec2(fragmentCoordinates.xy))) + gray - (1./float(grayLevelCount));
    // Clamp to the number of gray levels we want
    return clamp(floor(float(grayLevelCount) * noised) / (float(grayLevelCount)-1.), 0.f, 1.f);
}

void light() {
    // DIFFUSE_LIGHT += dither(clamp(dot(NORMAL, LIGHT), 0.0, 1.0), 64, FRAGCOORD) * ATTENUATION * LIGHT_COLOR;
	DIFFUSE_LIGHT += clamp(dot(NORMAL, LIGHT), 0.0, 1.0) * ATTENUATION * LIGHT_COLOR;
}