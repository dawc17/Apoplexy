#version 330

noperspective in vec2 fragTexCoord;
in vec3 fragNormal;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform vec3 lightDirection;
uniform float ambientStrength;
uniform float diffuseStrength;
uniform vec3 pointLightContribution;

out vec4 finalColor;

void main() {
    vec4 color = texture(texture0, fragTexCoord.xy) * colDiffuse * fragColor;

    vec3 normal = normalize(fragNormal);
    vec3 light = normalize(lightDirection);
    float diffuse = max(dot(normal, light), 0.0);
    vec3 lighting = vec3(ambientStrength + diffuseStrength * diffuse) +
                    pointLightContribution;

    finalColor = vec4(clamp(color.rgb * lighting, 0.0, 1.0), color.a);
}
