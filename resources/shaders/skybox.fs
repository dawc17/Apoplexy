#version 330

in vec3 fragPosition;

uniform samplerCube environmentMap;

out vec4 finalColor;

void main()
{
    finalColor = vec4(texture(environmentMap, fragPosition).rgb, 1.0);
}
