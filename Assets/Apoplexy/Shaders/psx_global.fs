#version 330

noperspective in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform vec2 screenSize;
uniform float time;
uniform float intensity;
uniform float pixelScale;
uniform float fixedVerticalResolution;
uniform float useFixedVerticalResolution;
uniform float colorSteps;
uniform float ditherStrength;
uniform float ditherScale;
uniform float scanlineStrength;
uniform float vignetteStrength;
uniform float saturation;
uniform float contrast;
uniform float colorBleed;
uniform vec3 colorTint;
uniform float gammaValue;
uniform float blackLevel;
uniform float chromaticOffset;
uniform float noiseStrength;
uniform float horizontalJitter;
uniform float curvature;
uniform vec3 fogColor;
uniform float fogAmount;

out vec4 finalColor;

float smoothStrength(float value)
{
    value = clamp(value, 0.0, 1.0);
    return value*value*(3.0 - 2.0*value);
}

float hash21(vec2 p)
{
    p = fract(p*vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x*p.y);
}

float bayer4(vec2 pixel)
{
    vec2 p = floor(mod(abs(pixel), 4.0));
    float index = p.y*4.0 + p.x;
    float value = 0.0;

    if (index < 0.5) value = 0.0;
    else if (index < 1.5) value = 8.0;
    else if (index < 2.5) value = 2.0;
    else if (index < 3.5) value = 10.0;
    else if (index < 4.5) value = 12.0;
    else if (index < 5.5) value = 4.0;
    else if (index < 6.5) value = 14.0;
    else if (index < 7.5) value = 6.0;
    else if (index < 8.5) value = 3.0;
    else if (index < 9.5) value = 11.0;
    else if (index < 10.5) value = 1.0;
    else if (index < 11.5) value = 9.0;
    else if (index < 12.5) value = 15.0;
    else if (index < 13.5) value = 7.0;
    else if (index < 14.5) value = 13.0;
    else value = 5.0;

    return (value + 0.5)/16.0;
}

vec2 applyCurvature(vec2 uv)
{
    float curve = smoothStrength(curvature);
    vec2 centered = uv*2.0 - 1.0;
    float radius = dot(centered, centered);
    centered *= 1.0 + radius*curve*0.12;
    return centered*0.5 + 0.5;
}

vec3 adjustColor(vec3 color)
{
    float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
    color = mix(vec3(luminance), color, saturation);
    color = (color - 0.5)*contrast + 0.5;
    color = max(color - blackLevel, 0.0)/max(1.0 - blackLevel, 0.0001);
    color *= colorTint;
    color = pow(clamp(color, 0.0, 1.0), vec3(1.0/max(gammaValue, 0.001)));
    return color;
}

void main()
{
    vec2 safeScreenSize = max(screenSize, vec2(1.0));
    vec2 uv = applyCurvature(fragTexCoord);
    float scale = max(pixelScale, 1.0);

    if (useFixedVerticalResolution > 0.5) {
        scale = max(safeScreenSize.y/max(fixedVerticalResolution, 1.0), 1.0);
    }

    if (horizontalJitter > 0.0) {
        float lineNoise = hash21(vec2(floor(gl_FragCoord.y), floor(time*24.0)));
        uv.x += (lineNoise - 0.5)*horizontalJitter*(scale/safeScreenSize.x)*4.0;
    }

    uv = clamp(uv, 0.0, 1.0);

    vec2 pixelUv = (floor(uv*safeScreenSize/scale) + 0.5)*scale/safeScreenSize;
    vec4 original = texture(texture0, fragTexCoord)*colDiffuse*fragColor;
    vec4 sampled = texture(texture0, pixelUv)*colDiffuse*fragColor;

    if (chromaticOffset > 0.0) {
        vec2 chroma = vec2((scale/safeScreenSize.x)*chromaticOffset*3.0, 0.0);
        sampled.r = texture(texture0, clamp(pixelUv + chroma, 0.0, 1.0)).r;
        sampled.b = texture(texture0, clamp(pixelUv - chroma, 0.0, 1.0)).b;
    }

    if (colorBleed > 0.0) {
        vec2 bleedOffset = vec2(scale/safeScreenSize.x, 0.0);
        vec3 left = texture(texture0, clamp(pixelUv - bleedOffset, 0.0, 1.0)).rgb;
        vec3 right = texture(texture0, clamp(pixelUv + bleedOffset, 0.0, 1.0)).rgb;
        sampled.rgb = mix(sampled.rgb, vec3(left.r, sampled.g, right.b), clamp(colorBleed, 0.0, 1.0));
    }

    float dither = bayer4(gl_FragCoord.xy/max(ditherScale, 0.001));
    float steps = max(colorSteps, 2.0);

    sampled.rgb = adjustColor(sampled.rgb);
    sampled.rgb += (hash21(gl_FragCoord.xy + floor(time*30.0)) - 0.5)*noiseStrength*0.08;
    sampled.rgb = mix(sampled.rgb, fogColor, clamp(fogAmount, 0.0, 1.0));
    sampled.rgb += (dither - 0.5)*ditherStrength/steps;

    float quantizeLevels = max(steps - 1.0, 1.0);
    sampled.rgb = floor(clamp(sampled.rgb, 0.0, 1.0)*quantizeLevels + dither*ditherStrength)/quantizeLevels;

    float scanline = sin(gl_FragCoord.y*3.14159265);
    sampled.rgb *= 1.0 - clamp((1.0 - scanline)*0.5*scanlineStrength, 0.0, 1.0);

    vec2 centered = fragTexCoord*2.0 - 1.0;
    float vignette = clamp(dot(centered, centered)*vignetteStrength, 0.0, 1.0);
    sampled.rgb *= 1.0 - vignette;

    finalColor = vec4(mix(original.rgb, sampled.rgb, clamp(intensity, 0.0, 1.0)), original.a);
}
