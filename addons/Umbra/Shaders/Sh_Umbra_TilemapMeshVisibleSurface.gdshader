shader_type spatial;

#define MAX_ATLAS_COUNT 16

uniform sampler2D mainTexture[MAX_ATLAS_COUNT] : filter_nearest, repeat_disable;
uniform sampler2D normalMapTexture[MAX_ATLAS_COUNT] : filter_nearest, repeat_disable;

varying flat int atlas;

// ATLAS SAMPLING

void vertex() {
	atlas = int(round(CUSTOM0.r));
}

void fragment() {
	vec4 mainColor = texture(mainTexture[atlas], UV);
	ALBEDO = vec3(1);
	ALPHA = mainColor.a * COLOR.a;
	ALPHA_SCISSOR_THRESHOLD = 0.5;
	NORMAL_MAP = texture(normalMapTexture[atlas], UV).rgb;

	//ALBEDO = result.rgb;
	//ALBEDO *= vec3(UV.r, UV.g, 0);
	//ALBEDO = vec3(float(atlas), 0.0f, 0.0f);
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