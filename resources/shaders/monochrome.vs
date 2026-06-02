#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec4 vertexColor;

uniform mat4 mvp;
uniform vec2 virtualResolution;

noperspective out vec2 fragTexCoord;
out vec4 fragColor;

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;

    vec4 clipPosition = mvp*vec4(vertexPosition, 1.0);
    vec3 ndcPosition = clipPosition.xyz/clipPosition.w;

    vec2 screenPosition = (ndcPosition.xy*0.5 + 0.5)*virtualResolution;
    screenPosition = floor(screenPosition + 0.5);
    ndcPosition.xy = (screenPosition/virtualResolution)*2.0 - 1.0;

    gl_Position = vec4(ndcPosition*clipPosition.w, clipPosition.w);
}
