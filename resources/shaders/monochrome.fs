#version 330

noperspective in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform float colorLevels;
uniform float ditherStrength;

out vec4 finalColor;

float bayer4(vec2 pixelPosition) {
    int x = int(mod(pixelPosition.x, 4.0));
    int y = int(mod(pixelPosition.y, 4.0));
    int index = y * 4 + x;

    float values[16] = float[16](
        0.0, 8.0, 2.0, 10.0,
        12.0, 4.0, 14.0, 6.0,
        3.0, 11.0, 1.0, 9.0,
        15.0, 7.0, 13.0, 5.0
    );

    return values[index] / 16.0 - 0.5;
}

float quantize(float value, float levels) {
    if (levels <= 1.0) {
        return step(0.5, value);
    }

    return floor(value * (levels - 1.0) + 0.5) / (levels - 1.0);
}

void main() {
    vec4 color = texture(texture0, fragTexCoord.xy) * colDiffuse * fragColor;

    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));

    float dither = bayer4(floor(gl_FragCoord.xy)) * ditherStrength;

    float value = clamp(gray + dither, 0.0, 1.0);
    float bw = quantize(value, colorLevels);

    finalColor = vec4(vec3(bw), color.a);
}
