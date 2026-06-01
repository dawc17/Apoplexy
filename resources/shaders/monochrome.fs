#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform vec2 resolution;
uniform float pixelSize;
uniform float threshold;

float bayer4(vec2 p) {
  int x = int(mod(p.x, 4.0));
  int y = int(mod(p.y, 4.0));
  int index = y * 4 + x;

  float values[16] = float[16](
      0.0, 8.0, 2.0, 10.0,
      12.0, 4.0, 14.0, 6.0,
      3.0, 11.0, 1.0, 9.0,
      15.0, 7.0, 13.0, 5.0
  );

  return (values[index] / 16.0) - 0.5;
}

void main() {
  vec2 pixelCoord = fragTexCoord * resolution;
  vec2 blockCoord = floor(pixelCoord / pixelSize) * pixelSize;
  vec2 uv = (blockCoord + pixelSize * 0.5) / resolution;

  vec4 color = texture(texture0, uv) * colDiffuse * fragColor;
  float luma = dot(color.rgb, vec3(0.299, 0.587, 0.114));

  float dither = bayer4(floor(pixelCoord / pixelSize)) * 0.10;
  float ink = 1.0 - step(threshold + dither, luma);

  finalColor = vec4(vec3(ink), color.a);
}
