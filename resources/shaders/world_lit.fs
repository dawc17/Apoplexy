#version 330

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float radius;
};

in vec3 fragPosition;
in vec3 fragNormal;
in vec4 fragColor;

uniform vec4 colDiffuse;
uniform vec3 ambientColor;
uniform float ambientIntensity;
uniform vec3 sunDirection;
uniform vec3 sunColor;
uniform float sunIntensity;
uniform int pointLightCount;
uniform PointLight pointLights[8];

out vec4 finalColor;

void main()
{
    vec4 baseColor = colDiffuse*fragColor;
    vec3 normal = normalize(fragNormal);

    vec3 lighting = ambientColor*ambientIntensity;

    float sunAmount = max(dot(normal, normalize(sunDirection)), 0.0);
    lighting += sunColor*sunAmount*sunIntensity;

    for (int i = 0; i < pointLightCount; ++i) {
        vec3 toLight = pointLights[i].position - fragPosition;
        float distanceToLight = length(toLight);
        float radius = max(pointLights[i].radius, 0.001);
        float falloff = clamp(1.0 - distanceToLight/radius, 0.0, 1.0);
        falloff *= falloff;

        if (falloff <= 0.0 || distanceToLight <= 0.001) {
            continue;
        }

        float diffuse = max(dot(normal, toLight/distanceToLight), 0.0);
        lighting += pointLights[i].color*diffuse*pointLights[i].intensity*falloff;
    }

    finalColor = vec4(clamp(baseColor.rgb*lighting, 0.0, 1.0), baseColor.a);
}
