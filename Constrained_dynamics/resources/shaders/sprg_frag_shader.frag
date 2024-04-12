#version 330 core
in float v_deflvalue;

out vec4 f_Color; // fragment's final color (out to the fragment shader)

void main()
{
	f_Color = vec4(0.0f,0.0f,1.0f,1.0f);
}