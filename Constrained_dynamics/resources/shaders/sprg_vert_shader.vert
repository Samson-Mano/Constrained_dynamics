#version 330 core

uniform mat4 modelMatrix;
uniform mat4 panTranslation;
uniform float zoomscale;
uniform float deflscale;
uniform float transparency = 1.0f;
uniform float geom_scale;

layout(location = 0) in vec2 pt_position;
layout(location = 1) in float node_defl;

out float v_deflvalue;

void main()
{
	// apply zoom scaling and Rotation to model matrix
	mat4 scalingMatrix = mat4(1.0) * zoomscale;
	scalingMatrix[3][3] = 1.0f;
	mat4 scaledModelMatrix = scalingMatrix * modelMatrix;
	
	// Final position passed to fragment shader
	gl_Position = scaledModelMatrix * vec4(pt_position, 0.0f, 1.0f) * panTranslation;
		
	// Color value [0 - 1]
	v_deflvalue = node_defl;
	
}