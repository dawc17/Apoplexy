#version 330

in vec3 fragPosition;

uniform samplerCube environmentMap;

out vec4 finalColor;

void main()
{
    finalColor = vec4(texture(environmentMap, vec3(fragPosition.x, -fragPosition.y, fragPosition.z)).rgb, 1.0);
}
