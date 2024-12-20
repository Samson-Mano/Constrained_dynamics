#version 330 core
uniform sampler2D u_Texture;

in vec2 v_textureCoord;
in float v_deflvalue;

out vec4 f_Color; // fragment's final color (out to the fragment shader)

void main()
{
	vec4 texColor = texture(u_Texture, v_textureCoord);
	f_Color = vec4(1.0f,0.0f,0.0f,1.0f) * texColor;
}